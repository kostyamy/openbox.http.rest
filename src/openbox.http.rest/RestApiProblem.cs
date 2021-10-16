namespace Shared.Rest
{
	public class RestApiProblem : IRestApiProblem
	{
		public int? StatusCode { get; }
		public string Title { get; }
		public string Details { get; }
		public string Type { get; }
		public string Instance { get; }

		public RestApiProblem(int? statusCode, string details = null, string title = null, string type = null, string instance = null)
		{
			StatusCode = statusCode;
			Details = details;
			Title = title;
			Type = type;
			Instance = instance;
		}
	}
}
