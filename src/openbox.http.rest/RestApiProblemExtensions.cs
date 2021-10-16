using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Rest
{
	public static class RestApiProblemExtensions
	{
		protected class ProblemBodyPoco
		{
			[JsonPropertyName("statusCode")]
			public int? StatusCode { get; set; }

			[JsonPropertyName("title")]
			public string Title { get; set; }

			[JsonPropertyName("details")]
			public string Details { get; set; }

			[JsonPropertyName("type")]
			public string Type { get; set; }

			[JsonPropertyName("instance")]
			public string Instance { get; set; }
		}

		private static  JsonSerializerOptions ProblemJsonSettings = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			AllowTrailingCommas = true
		};

		public static IRestApiProblem GetProblem(this HttpResponseMessage httpResponse)
		{
			if (httpResponse is null)
				return null;

			const string defaultTitle = "Request completed with status code {0}";

			var statusCode = (int)httpResponse.StatusCode;
			var type = httpResponse.Content?.Headers.ContentType?.MediaType?.ToLower();
			var instance = httpResponse.RequestMessage?.RequestUri?.ToString();

			string title, details;
			var isProblem = type == "application/problem+json";
			try
			{
				switch (type)
				{
					case "application/problem+json":
					case "application/json":
						var content = httpResponse.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						var body = JsonSerializer.Deserialize<ProblemBodyPoco>(content, ProblemJsonSettings);
						if (body == null
							|| (!isProblem && (!body.StatusCode.HasValue && body.Title == null && body.Details == null && body.Type == null && body.Instance == null)))
						{
							return null;
						}
						if (body.StatusCode.HasValue)
							statusCode = body.StatusCode.Value;
						title = body.Title;
						details = body.Details;
						if (body.Type != null)
							type = body.Type;
						break;

					case "text/plain":
						title = string.Format(defaultTitle, statusCode);
						details = httpResponse.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
						break;

					default:
						return null;
				}
			}
			catch (Exception exception)
			{
				title = string.Format(defaultTitle, statusCode);
				details = exception.Message;
			}

			return new RestApiProblem(statusCode, details, title, type, instance);
		}
	}
}
