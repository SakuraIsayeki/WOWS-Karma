using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using System.Threading;
using WowsKarma.Common;

namespace WowsKarma.Api.Services.Replays;

public class ReplaysIngestService
{
	public const string ReplayBlobContainer = "replays";
	public const int MaxReplaySize = 5242880;

	private readonly BlobServiceClient _serviceClient;
	private readonly BlobContainerClient _containerClient;

	public ReplaysIngestService(IConfiguration configuration)
	{
		string connectionString = configuration[$"API:{Startup.ApiRegion.ToRegionString()}:Azure:Storage:ConnectionString"];
		_serviceClient = new(connectionString);
		_containerClient = _serviceClient.GetBlobContainerClient(ReplayBlobContainer);
	}

	public async Task<Uri> UploadReplayFileAsync(IFormFile replayFile, CancellationToken ct)
	{
		// Over 5MB is too much for a WOWS Replay file.
		if (replayFile.Length is 0 or > MaxReplaySize)
		{
			throw new ArgumentOutOfRangeException(nameof(replayFile));
		}

		await _containerClient.UploadBlobAsync(replayFile.FileName, replayFile.OpenReadStream(), ct);

		return _containerClient.GetBlobClient(replayFile.FileName).Uri;
	}
}
