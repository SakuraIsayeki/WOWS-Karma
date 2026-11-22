using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using TUnit.Aspire;
using WowsKarma.Api.Data;
using WowsKarma.Api.Data.Models;
using WowsKarma.Api.Data.Models.Auth;

namespace WowsKarma.Api.E2E.Support;

/// <summary>Adds database seeding and authenticated clients to TUnit's Aspire fixture.</summary>
public sealed class ApiFixture : AspireFixture<TestHostSettings>
{
    private NpgsqlDataSource? _source;
    private string _connection = null!;
    private int _nextId = 10000;
    private string? _resultsDirectory;

    protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(4);
    protected override bool EnableTelemetryCollection => false;
    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.Named;

    // The API health check runs after startup migrations, blob initialization, and dependency readiness.
    protected override IEnumerable<string> ResourcesToWaitFor() => ["api"];

    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        // TUnit can change the working directory. Resolve artifact paths from the host project.
        _resultsDirectory = Path.GetFullPath("../TestResults", builder.AppHostDirectory);
        builder.Services.AddLogging(logging => logging.ClearProviders().AddConsole());
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _connection = await GetConnectionStringAsync("api-db", RunCancellationToken)
            ?? throw new InvalidOperationException("Missing test database connection.");
        _source = new NpgsqlDataSourceBuilder(_connection).ConfigureApiDbDataSourceBuilder().Build();
    }

    public ApiDbContext Database() => new(new DbContextOptionsBuilder<ApiDbContext>()
        .UseNpgsql(_source!, o => o.MapApplicationEnums()).Options);

    public AuthDbContext AuthDatabase() => new(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql(_connection).Options);

    public HttpClient Client(TestAccount? account = null, bool cookie = false)
    {
        var client = CreateHttpClient("api", "http");
        client.Timeout = TimeSpan.FromSeconds(30);
        if (account is not null)
        {
            if (cookie) client.DefaultRequestHeaders.Add("Cookie", $"Auth_EU={account.Token}");
            else client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", account.Token);
        }
        return client;
    }

    /// <summary>Seeds the API and auth databases and signs a token with the matching seed.</summary>
    public async Task<TestAccount> AccountAsync(bool banned = false, params string[] roles)
    {
        var id = (uint)Interlocked.Increment(ref _nextId);
        var seed = Guid.NewGuid();
        await using var db = Database();
        db.Players.Add(new Player
        {
            Id = id, Username = $"E2E_{id}", UpdatedAt = DateTimeOffset.UtcNow,
            WgAccountCreatedAt = DateTimeOffset.UtcNow.AddYears(-2),
            LastBattleTime = DateTimeOffset.UtcNow, PostsBanned = banned
        });
        await db.SaveChangesAsync();
        await using var auth = AuthDatabase();
        auth.Users.Add(new User { Id = id, SeedToken = seed, Roles = await auth.Roles.Where(r => roles.Contains(r.InternalName)).ToListAsync() });
        await auth.SaveChangesAsync();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()), new(ClaimTypes.Name, $"E2E_{id}"), new("seed", seed.ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var token = new JwtSecurityToken("e2e", "e2e", claims, expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestHostSettings.SigningKey)), SecurityAlgorithms.HmacSha256));
        return new TestAccount(id, new JwtSecurityTokenHandler().WriteToken(token));
    }

    /// <summary>Seeds a post for scenarios that exercise endpoints independently of post creation.</summary>
    public async Task<Guid> PostAsync(TestAccount author, TestAccount target, bool locked = false)
    {
        await using var db = Database();
        var post = new Post { Id = Guid.NewGuid(), AuthorId = author.Id, PlayerId = target.Id,
            Title = "Baseline post", Content = "A baseline post with deterministic content.", ModLocked = locked };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return post.Id;
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (_source is not null)
            {
                await _source.DisposeAsync();
                // Look up the resource object so Aspire resolves the executable instance's log stream.
                var api = App.Services.GetRequiredService<DistributedApplicationModel>().Resources.Single(resource => resource.Name == "api");
                var logs = App.Services.GetRequiredService<ResourceLoggerService>();
                var directory = _resultsDirectory!;
                Directory.CreateDirectory(directory);
                await using var writer = new StreamWriter(Path.Combine(directory, "api.log"));
                await foreach (var batch in logs.GetAllAsync(api))
                {
                    foreach (var line in batch)
                        await writer.WriteLineAsync(line.Content);
                }
            }
        }
        finally
        {
            await base.DisposeAsync();
        }
    }
}

public sealed record TestAccount(uint Id, string Token);
