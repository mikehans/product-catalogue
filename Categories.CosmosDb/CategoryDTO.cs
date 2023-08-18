namespace Categories.CosmosDb;

internal class CategoryDTO
{
    internal string Id { get; set; }
    internal string PartitionKey { get; set; }
    internal string Name { get; set; }
    internal bool? IsRoot { get; set; }
    internal NestedCategoryDTO? Parent { get; set; }
    internal ICollection<NestedCategoryDTO>? Ancestors { get; set; }
}

internal class NestedCategoryDTO
{
    internal string Id { get; set; }
    internal string Name { get; set; }
}