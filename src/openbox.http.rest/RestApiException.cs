using System;
using System.Net;
using System.Net.Http;

namespace Shared.Rest
{
	public class RestApiException : HttpRequestException
	{
		public HttpStatusCode? StatusCode { get; }
		public object Error { get; private set; }
		public IRestApiProblem Problem => Error as IRestApiProblem;

		public RestApiException(string message, Exception exception)
			: base(message, exception)
		{
		}

		public RestApiException(IRestApiProblem problem, Exception exception)
			: base(problem?.Title, exception)
		{
			Error = problem;
		}

		public RestApiException(HttpStatusCode statusCode, IRestApiProblem problem)
			: base(problem?.Details ?? $"Request completed with status code {statusCode}")
		{
			StatusCode = statusCode;
			Error = problem;
		}
	}
}

