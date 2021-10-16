using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Rest
{
	public class RestApiClient : IRestApiClient
	{
		private const string RequestFailed = "RESTful api request failed.";

		private readonly HttpClient _httpClient;

		public virtual HttpClient HttpClient => _httpClient;

		#region Public properties
		public virtual Uri BaseUri { get; protected set; }

		public IDictionary<string, string> DefaultHeaders { get; } = new Dictionary<string, string>
		{
			{ "Accept", "application/json, application/problem+json" },
		};
		
		public Func<HttpRequestMessage, CancellationToken, Task> RequestHandler { get; set; }
		
		public Func<HttpResponseMessage, CancellationToken, Task> ResponseHandler { get; set; }

		public JsonSerializerOptions JsonSettings { get; set; } = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		/// <summary>
		/// Creates new instance of the rest api client
		/// </summary>
		/// <param name="httpClient">The http client</param>
		/// <param name="baseUri">The base uri for all endpoints</param>
		public RestApiClient(HttpClient httpClient, string baseUri)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			BaseUri = new Uri(baseUri ?? string.Empty, UriKind.RelativeOrAbsolute);
		}

		#endregion

		#region Async methods

		public async Task<HttpStatusCode> GetAsync(string uri, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			await SendAsync<HttpStatusCode>(HttpMethod.Get, uri, null, validateResponse, cancellationToken);

		public async Task<TResult> GetAsync<TResult>(string uri, CancellationToken cancellationToken = default) =>
			await SendAsync<TResult>(HttpMethod.Get, uri, null, true, cancellationToken);

		public async Task<HttpStatusCode> PostAsync(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			await SendAsync<HttpStatusCode>(HttpMethod.Post, uri, content, validateResponse, cancellationToken);

		public async Task<TResult> PostAsync<TResult>(string uri, object content = null, CancellationToken cancellationToken = default) =>
			await SendAsync<TResult>(HttpMethod.Post, uri, content, true, cancellationToken);

		public async Task<HttpStatusCode> PutAsync(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			await SendAsync<HttpStatusCode>(HttpMethod.Put, uri, content, validateResponse, cancellationToken);

		public async Task<TResult> PutAsync<TResult>(string uri, object content = null, CancellationToken cancellationToken = default) =>
			await SendAsync<TResult>(HttpMethod.Put, uri, content, true, cancellationToken);

		public async Task<HttpStatusCode> DeleteAsync(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			await SendAsync<HttpStatusCode>(HttpMethod.Delete, uri, content, validateResponse, cancellationToken);

		public async Task<TResult> DeleteAsync<TResult>(string uri, object content = null, CancellationToken cancellationToken = default) =>
			await SendAsync<TResult>(HttpMethod.Delete, uri, content, true, cancellationToken);

		#endregion

		#region Sync methods

		public HttpStatusCode Get(string uri, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			Send<HttpStatusCode>(HttpMethod.Get, uri, null, validateResponse, cancellationToken);

		public TResult Get<TResult>(string uri, CancellationToken cancellationToken = default) =>
			Send<TResult>(HttpMethod.Get, uri, null, true, cancellationToken);

		public HttpStatusCode Post(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			Send<HttpStatusCode>(HttpMethod.Post, uri, content, validateResponse, cancellationToken);

		public TResult Post<TResult>(string uri, object content = null, CancellationToken cancellationToken = default) =>
			Send<TResult>(HttpMethod.Post, uri, content, true, cancellationToken);

		public HttpStatusCode Put(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			Send<HttpStatusCode>(HttpMethod.Put, uri, content, validateResponse, cancellationToken);

		public TResult Put<TResult>(string uri, object content = null, CancellationToken cancellationToken = default) =>
			Send<TResult>(HttpMethod.Put, uri, content, true, cancellationToken);

		public HttpStatusCode Delete(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default) =>
			Send<HttpStatusCode>(HttpMethod.Delete, uri, content, validateResponse, cancellationToken);

		public TResult Delete<TResult>(string uri, object content = null, CancellationToken cancellationToken = default) =>
			Send<TResult>(HttpMethod.Delete, uri, content, true, cancellationToken);

		#endregion

		#region Protected overrides

		protected virtual Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string relativeUri, object content, CancellationToken cancellationToken)
		{
			var requestUri = new Uri(BaseUri, relativeUri);
			var request = new HttpRequestMessage(method, requestUri);

			if (!(content is null))
			{
				request.Content = new StringContent(JsonSerializer.Serialize(content, JsonSettings), System.Text.Encoding.UTF8, "application/json");
				cancellationToken.ThrowIfCancellationRequested();
			}

			return Task.FromResult(request);
		}

		protected virtual Task AddRequestHeadersAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (DefaultHeaders.Count > 0)
			{
				foreach (var header in DefaultHeaders)
				{
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
			}

			return Task.CompletedTask;
		}

		protected virtual Task ValidateResponseAsync(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
		{
			if (!response.IsSuccessStatusCode)
			{
				throw new RestApiException(response.StatusCode, response.GetProblem());
			}

			return Task.CompletedTask;
		}

		#endregion

		#region Internal methods

		private async Task<TResult> SendAsync<TResult>(HttpMethod method, string uri, object content, bool validateResponse, CancellationToken cancellationToken)
		{
			int? statusCode = null;
			try
			{
				using (var request = await CreateRequestAsync(method, uri, content, cancellationToken))
				{
					cancellationToken.ThrowIfCancellationRequested();
					await AddRequestHeadersAsync(request, cancellationToken);

					if (!(RequestHandler is null))
					{
						cancellationToken.ThrowIfCancellationRequested();
						await RequestHandler(request, cancellationToken);
					}

					cancellationToken.ThrowIfCancellationRequested();
					using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false)) //TODO: perhaps we can remove .ConfigureAwait(false) 
					{
						statusCode = (int)response.StatusCode;

						cancellationToken.ThrowIfCancellationRequested();

						if (ResponseHandler is null)
						{
							if (validateResponse)
							{
								await ValidateResponseAsync(request, response, cancellationToken);
								cancellationToken.ThrowIfCancellationRequested();
							}
						}
						else
						{
							await ResponseHandler(response, cancellationToken);
							cancellationToken.ThrowIfCancellationRequested();
						}

						return typeof(HttpStatusCode) == typeof(TResult)
							? (TResult)(object)statusCode
							: Deserialize<TResult>(await response.Content.ReadAsStringAsync(), JsonSettings);
					}
				}
			}
			catch (Exception exception) when (!(exception is RestApiException))
			{
				throw new RestApiException(new RestApiProblem(statusCode, exception.Message, RequestFailed, method.ToString(), uri), exception);
			}
		}

		private TResult Send<TResult>(HttpMethod method, string uri, object content, bool validateResponse, CancellationToken cancellationToken)
		{
			int? statusCode = null;
			try
			{
				using (var request = WaitForResult(CreateRequestAsync(method, uri, content, cancellationToken)))
				{
					AddRequestHeadersAsync(request, cancellationToken).Wait(cancellationToken);

					if (!(RequestHandler is null))
					{
						RequestHandler(request, cancellationToken).Wait(cancellationToken);
					}

					using (var response = WaitForResult(_httpClient.SendAsync(request, cancellationToken)))
					{
						statusCode = (int)response.StatusCode;

						if (ResponseHandler is null)
						{
							if (validateResponse)
								ValidateResponseAsync(request, response, cancellationToken).Wait(cancellationToken);
						}
						else
						{
							ResponseHandler(response, cancellationToken).Wait(cancellationToken);
						}

						if (typeof(TResult) == typeof(HttpResponseMessage) || typeof(TResult) == typeof(HttpStatusCode))
						{
							return (TResult)(object)statusCode;
						}

						var json = WaitForResult(response.Content?.ReadAsStringAsync());
						return Deserialize<TResult>(json, JsonSettings);
					}
				}
			}
			catch (Exception exception) when (!(exception is RestApiException))
			{
				throw new RestApiException(new RestApiProblem(statusCode, exception.Message, RequestFailed, method.ToString(), uri), exception);
			}
		}

		private static T Deserialize<T>(string json, JsonSerializerOptions jsonSettings) =>
			string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, jsonSettings);

		private static TResult WaitForResult<TResult>(Task<TResult> task) =>
			task.ConfigureAwait(false).GetAwaiter().GetResult();

		#endregion
	}
}
