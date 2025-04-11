namespace IT_Next.Core.Helpers;

public class ResultWithResponse<TResponse> : Result
{
    public TResponse Response { get; set; }

    public ResultWithResponse(bool isSucceeded, TResponse response)
        : base(isSucceeded)
    {
        Response = response;
    }
}