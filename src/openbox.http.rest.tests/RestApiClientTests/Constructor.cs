using System.Threading;
using System.Threading.Tasks;
using Shared.UnitTesting.MSTest;
using Shared.Rest;
using System.Net.Http;

namespace shared.rest.RestApiClientTests
{
	[Behavior]
	public class Constructor : Behavior<Constructor>
	{
		private static HttpClient _httpClient;
		private static IRestApiClient _restClientWithBaseUri;
		private static IRestApiClient _restClientWithoutBaseUri;

		protected override Task When(CancellationToken canellationToken)
		{
			_httpClient = new HttpClient();

			_restClientWithBaseUri = new RestApiClient(_httpClient, "http://test-base-uri.local");
			_restClientWithoutBaseUri = new RestApiClient(_httpClient, null);

			return Task.CompletedTask;
		}

		[Then]
		public void RestClientWithBaseUriCreated()
		{
			Asserts.IsTypeOf<RestApiClient>(_restClientWithBaseUri);
		}

		[Then]
		public void BaseUriIsSet()
		{
			Asserts.AreEqual("http://test-base-uri.local/", _restClientWithBaseUri.BaseUri.ToString());
		}

		[Then]
		public void BaseUriIsAbsolute()
		{
			Asserts.IsTrue(_restClientWithBaseUri.BaseUri.IsAbsoluteUri);
		}

		[Then]
		public void RestClientWithoutBaseUriCreated()
		{
			Asserts.IsTypeOf<RestApiClient>(_restClientWithoutBaseUri);
		}

		[Then]
		public void RestClientWithoutBaseUriBaseUriIsNotSet()
		{
			Asserts.AreEqual(string.Empty, _restClientWithoutBaseUri.BaseUri.ToString());
		}

		[Then]
		public void RestClientWithoutBaseUriBaseUriIsNotAbsolute()
		{
			Asserts.IsFalse(_restClientWithoutBaseUri.BaseUri.IsAbsoluteUri);
		}
	}
}
