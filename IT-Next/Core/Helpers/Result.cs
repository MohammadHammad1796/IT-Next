namespace IT_Next.Core.Helpers;

public class Result
{
    public bool IsSucceeded { get; set; }

    public Result(bool isSucceeded)
    {
        IsSucceeded = isSucceeded;
    }

    public Result()
    {
    }
}
