namespace Categories;

public class CategoryBasic
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class Category : CategoryBasic
{
    public bool? IsRoot { get; init; }
    public Category? Parent { get; set; }
    public bool? IsDeleting { get; set; }
    public ICollection<Category>? Ancestors { get; init; }

}
