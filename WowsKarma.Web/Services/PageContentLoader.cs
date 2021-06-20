using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WowsKarma.Web.Services
{
	public class PageContentLoader : IHostedService, IDisposable
	{
		public static string WebRootPageContentsPath { get; } = Path.Join("assets");

		private readonly ILogger<PageContentLoader> logger;
		private readonly IWebHostEnvironment env;
		private readonly IDistributedCache cache;
		private readonly FileSystemWatcher watcher;
		private bool disposedValue;

		public PageContentLoader(ILogger<PageContentLoader> logger, IWebHostEnvironment env, IDistributedCache cache)
		{
			this.logger = logger;
			this.env = env;
			this.cache = cache;

			watcher = new(env.WebRootPath, "*.html")
			{
				EnableRaisingEvents = true,
				IncludeSubdirectories = true
			};
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public async Task<MarkupString> LoadMarkupAsync(string path)
		{
			string content = await cache.GetStringAsync(path);

			if (string.IsNullOrEmpty(content))
			{
				using StreamReader streamReader = new(env.WebRootFileProvider.GetFileInfo(path).CreateReadStream(), Encoding.UTF8);
				content = await streamReader.ReadToEndAsync();
				await cache.SetStringAsync(path, content);

				logger.LogDebug("Fetched content for {path} from file.", path);
			}
			else
			{
				logger.LogDebug("Fetched content for {path} from cache.", path);
			}

			return (MarkupString)content;
		}

		public async Task PopulatePageCacheAsync(string path, CancellationToken cancellationToken)
		{
			foreach (string item in Directory.GetFiles(path, "*.html", SearchOption.AllDirectories))
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return;
				}

				using FileStream fileStream = File.OpenRead(item);
				using StreamReader streamReader = new(fileStream);

				await cache.SetStringAsync(item[env.WebRootPath.Length..], await streamReader.ReadToEndAsync(), cancellationToken);
			}

			logger.LogInformation("Populated cache.");
		}

		private async void OnFileUpdated(object _, FileSystemEventArgs e)
		{
			logger.LogInformation("File change detected ({type}) : {file}", e.ChangeType.ToString(), e.FullPath);

			string path = e.FullPath[(env.WebRootPath.Length + 1)..];

			await cache.SetStringAsync(path, string.Empty, CancellationToken.None);
			logger.LogDebug("Cleared cache entry: {path}", path);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					watcher.Dispose();
				}

				disposedValue = true;
			}
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Starting PageContentLoader background service. \nContents Path : {path}", env.WebRootPath);

			watcher.Changed += OnFileUpdated;
			watcher.Created += OnFileUpdated;
			watcher.Deleted += OnFileUpdated;
			watcher.Renamed += OnFileUpdated;

			await PopulatePageCacheAsync(Path.Combine(env.WebRootPath, WebRootPageContentsPath), cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Stopped PageContentLoader background service.", env.WebRootPath);

			watcher.Changed -= OnFileUpdated;
			watcher.Created -= OnFileUpdated;
			watcher.Deleted -= OnFileUpdated;
			watcher.Renamed -= OnFileUpdated;

			return Task.CompletedTask;
		}
	}
}