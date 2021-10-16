namespace Shared.Rest
{
	public interface IRestApiProblem
	{
		int? StatusCode { get; }
		string Title { get; }
		string Details { get; }
		string Type { get; }
		string Instance { get; }
	}
}