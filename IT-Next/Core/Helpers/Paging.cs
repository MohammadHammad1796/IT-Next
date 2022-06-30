namespace IT_Next.Core.Helpers;

public class Paging
{
    public int Number { get; set; }

    public int Size { get; set; }

    public Paging(int number, int size)
    {
        Number = number;
        Size = size;
    }
}