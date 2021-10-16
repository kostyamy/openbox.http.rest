using System;
using System.Threading;
using System.Threading.Tasks;
using Shared.UnitTesting;
using Shared.UnitTesting.MSTest;
using Shared.Rest;

namespace shared.rest.RestApiClientTests
{
	[Behavior]
	public class ConstructorWithErrors : Behavior<ConstructorWithErrors>
	{
		private static Exception _nullHttpClientException;

		protected override Task When(CancellationToken canellationToken)
		{
			_nullHttpClientException = ExpectException(() => new RestApiClient(null, "test-baseUri"));
			return Task.CompletedTask;
		}

		[Then]
		public void NullHttpClientThrowsArgumentNullException()
		{
			Asserts.IsTypeOf<ArgumentNullException>(_nullHttpClientException);
		}

		[Then]
		public void NullHttpClientThrowsArgumentNullExceptionWithParamName()
		{
			Asserts.AreEqual("httpClient", _nullHttpClientException.GetParamName());
		}
	}
}
