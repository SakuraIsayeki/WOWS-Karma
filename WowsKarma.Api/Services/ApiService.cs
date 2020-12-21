﻿using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models.WorldOfWarships.Responses;
using Wargaming.WebAPI.Requests;
using WowsKarma.Common;

using ApiServiceProto = WowsKarma.Common.ApiService;

namespace WowsKarma.Api.Services
{
	public class ApiService : ApiServiceProto.ApiServiceBase
	{
		private readonly ILogger<ApiService> logger;
		private readonly WorldOfWarshipsHandler wowsApi;
		private readonly VortexApiHandler vortexApi;


		public ApiService(ILogger<ApiService> logger, WorldOfWarshipsHandler wowsApi, VortexApiHandler vortexApi)
		{
			this.logger = logger;
			this.wowsApi = wowsApi;
			this.vortexApi = vortexApi;
		}


		public override async Task<ListAccountsResponse> ListAccounts(ListAccountsRequest request, ServerCallContext context)
		{
			if (string.IsNullOrWhiteSpace(request.Search))
			{
				return null;
			}
			
			IEnumerable<AccountListing> results = await wowsApi.ListPlayersAsync(request.Search);
			
			if (results is null || results.Count() is 0)
			{
				return null;
			}

			ListAccountsResponse response = new();

			foreach (AccountListing listing in results)
			{
				response.Results.Add(new ListAccountsResponse.Types.Result { AccountId = listing.AccountId, Username = listing.Nickname });
			}

			return response;
		}

		public override async Task<GetAccountResponse> GetAccount(GetAccountRequest request, ServerCallContext context)
		{
			AccountInfo result = await vortexApi.FetchAccountAsync(request.AccountId);
			return new()
			{
				Account = new()
				{
					AccountId = request.AccountId,
					Username = result.Name,
					WgAccountCreatedAt = result.Statistics.Basic.CreatedAt,
					WgKarma = result.Statistics.Basic.Karma
				}
			};
		}
	}
}