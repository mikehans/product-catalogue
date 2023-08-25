namespace Categories.CosmosDb;

public class CategoryDTO
{
    public string id { get; set; }
    public string RootId { get; set; }
    public string Name { get; set; }
    public bool? IsRoot { get; set; }
    public NestedCategoryDTO? Parent { get; set; }
    public ICollection<NestedCategoryDTO>? Ancestors { get; set; }
}

public class NestedCategoryDTO
{
    public string id { get; set; }
    public string Name { get; set; }
}