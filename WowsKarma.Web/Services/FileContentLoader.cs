using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace WowsKarma.Web.Services
{
	public class FileContentLoader
	{
		public const string WebRootPageAssetsPath = "assets";

		public event EventHandler<string> OnCacheEviction;

		private readonly ILogger<FileContentLoader> logger;
		private readonly IDistributedCache cache;
		private readonly IFileProvider fileProvider;

		private readonly Dictionary<string, IChangeToken> tokens = new();

		public FileContentLoader(ILogger<FileContentLoader> logger, IWebHostEnvironment env, IDistributedCache cache)
		{
			this.logger = logger;
			this.cache = cache;
			fileProvider = env.WebRootFileProvider;
		}

		public async Task<string> LoadContent(string fileName, CancellationToken cancellationToken)
		{
			string fileContent;
			string filePath;

			try
			{
				filePath = fileProvider.GetFileInfo(fileName)?.PhysicalPath;

				if (filePath is null)
				{
					logger.LogWarning("No HTML content file exists with path or name {FileName}. Be sure to create one at this location.", fileName);
					return null;
				}
			}
			catch (Exception e)
			{
				logger.LogWarning(e, "No HTML content file exists with path or name {FileName}. Be sure to create one at this location.", fileName);
				return null;
			}




			// Try to obtain the file contents from the cache.
			if ((fileContent = await cache.GetStringAsync(filePath, cancellationToken)) is not null)
			{
				logger.LogDebug("Fetched content for {Path} from cache.", filePath);
				return fileContent;
			}

			// The cache doesn't have the entry, so obtain the file 
			// contents from the file itself.
			if ((fileContent = await GetContentFromFileAsync(filePath)) is not null)
			{
				// Obtain a change token from the file provider whose
				// callback is triggered when the file is modified.
				IChangeToken changeToken = fileProvider.Watch(fileName);
				changeToken.RegisterChangeCallback(TriggerCacheEviction, filePath);

				// Put the file contents into the cache.
				await cache.SetStringAsync(filePath, fileContent, cancellationToken);
				tokens.Add(filePath, changeToken);

				logger.LogDebug("Fetched content for {Path} from file.", filePath);
				return fileContent;
			}

			return null;
		}

		private static async Task<string> GetContentFromFileAsync(string filePath)
		{
			for (int runCount = 1; runCount < 4; runCount++)
			{
				try
				{
					using StreamReader fileStreamReader = File.OpenText(filePath);
					return await fileStreamReader.ReadToEndAsync();
				}
				catch (IOException e)
				{
					if (e.HResult is not -2147024864)
					{
						throw;
					}
					else
					{
						await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, runCount)));
					}
				}
			}

			return null;
		}

		private void TriggerCacheEviction(object state)
		{
			string filePath = state as string;

			tokens.Remove(filePath);
			cache.Remove(filePath);
			logger.LogInformation("Evicted file {File} from cache.", filePath);

			OnCacheEviction?.Invoke(this, filePath);
		}
	}
}