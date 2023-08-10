namespace Categories;

public class Category
{
    public string Id { get; init; }
    public bool? IsRoot { get; init; }

    //public string PartitionKey { get; set; }
    public string Name { get; init; }
    public CategoryBriefForm? Parent { get; init; }
    public ICollection<CategoryBriefForm>? Ancestors { get; init; }
}

public class CategoryBriefForm
{
    public string Id { get; init; }
    public string Name { get; init; }
}