using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WowsKarma.Api.Services.Authentication
{
	public class AccessKeySwaggerFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			operation.Parameters ??= new List<OpenApiParameter>();
			operation.Parameters.Add(new OpenApiParameter
			{
				Name = "Access-Key",
				In = ParameterLocation.Header,
				Required = false,
				Schema = new OpenApiSchema
				{
					Type = "String"
				}
			});
		}
	}
}
