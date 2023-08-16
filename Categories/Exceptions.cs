namespace Categories;

public class InvalidNumberOfRootNodesException : Exception
{
    public InvalidNumberOfRootNodesException()
        : base("There must be 1 and only 1 root for a new tree")
    {
    }
}