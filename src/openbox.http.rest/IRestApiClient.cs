using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Rest
{
	public interface IRestApiClient
	{
		Uri BaseUri { get; }
		IDictionary<string, string> DefaultHeaders { get; }
		Func<HttpRequestMessage, CancellationToken, Task> RequestHandler { get; }
		Func<HttpResponseMessage, CancellationToken, Task> ResponseHandler { get; }

		#region Async methods
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
		Task<TResult> GetAsync<TResult>(string uri, CancellationToken cancellationToken = default);
		Task<TResult> PostAsync<TResult>(string uri, object content = null, CancellationToken cancellationToken = default);
		Task<TResult> PutAsync<TResult>(string uri, object content = null, CancellationToken cancellationToken = default);
		Task<TResult> DeleteAsync<TResult>(string uri, object content = null, CancellationToken cancellationToken = default);
		#endregion

		#region Sync methods

		HttpStatusCode Get(string uri, bool validateResponse = true, CancellationToken cancellationToken = default);
		TResult Get<TResult>(string uri, CancellationToken cancellationToken = default);

		HttpStatusCode Post(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default);
		TResult Post<TResult>(string uri, object content = null, CancellationToken cancellationToken = default);

		HttpStatusCode Put(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default);
		TResult Put<TResult>(string uri, object content = null, CancellationToken cancellationToken = default);

		HttpStatusCode Delete(string uri, object content = null, bool validateResponse = true, CancellationToken cancellationToken = default);
		TResult Delete<TResult>(string uri, object content = null, CancellationToken cancellationToken = default);

		#endregion
	}
}
