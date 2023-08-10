namespace Categories;

public class CategoryHierarchy
{
    private readonly Dictionary<string, Category> _hierarchy;

    public CategoryHierarchy(CategoryBriefForm rootCategory)
    {
        var category = new Category
        {
            Id = rootCategory.Id,
            Name = rootCategory.Name,
            IsRoot = true
        };
        _hierarchy = new Dictionary<string, Category> { { rootCategory.Id, category } };
    }

    public Dictionary<string, Category> Get()
    {
        return _hierarchy;
    }

    public Category GetRoot()
    {
        // IEnumerable<KeyValuePair<string,Category>> pairs = _hierarchy.Where(item => item.Value.IsRoot == true);
        var values = _hierarchy.Select(kvp => kvp.Value);
        var rootCategory = values.FirstOrDefault(v => v.IsRoot == true);

        return rootCategory;
    }

    // revise what a category is: 2 variants - 1 for when creating and one for when retrieving
    public Dictionary<string, Category> InsertChild(CategoryBriefForm childCategory, CategoryBriefForm parent)
    {
        var nestedParentCategory = new CategoryBriefForm
        {
            Id = parent.Id,
            Name = parent.Name
        };
        
        var resultCategory = new Category
        {
            Id = childCategory.Id,
            Name = childCategory.Name,
            Ancestors = new List<CategoryBriefForm>(),
            Parent = nestedParentCategory
        };
        resultCategory.Ancestors.Add(nestedParentCategory);

        var actualParent = _hierarchy[parent.Id];

        // Have to add each item because otherwise we are setting a reference, leading to wrong Ancestors for items
        if (actualParent.Ancestors is { Count: > 0 })
        {
            foreach (var ancestor in actualParent.Ancestors)
            {
                resultCategory.Ancestors.Add(ancestor);
            }
        }

        // Wrong: This leads to setting a reference, rather than the individual items
        // resultCategory.Ancestors = actualParent.Ancestors ?? new List<NestedCategory>();
        
        _hierarchy.Add(resultCategory.Id, resultCategory);

        return _hierarchy;
    }
}