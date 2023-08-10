namespace Categories;

public class HierarchyCollection
{
    private readonly Dictionary<string, CategoryHierarchy> _hierarchies;

    public HierarchyCollection()
    {
        _hierarchies = new Dictionary<string,CategoryHierarchy>();
    }

    public Dictionary<string, CategoryHierarchy> Get()
    {
        return _hierarchies;
    }

    public void Add(CategoryHierarchy hierarchy)
    {
        var root = hierarchy.GetRoot();
        _hierarchies.Add(root.Name, hierarchy);
    }

    public IReadOnlyCollection<Category> GetRootCategories()
    {
        var hierarchies = _hierarchies.Select(h => h.Value);
        var roots = new List<Category>();
        
        foreach (var hierarchy in hierarchies)
        {
            roots.Add(hierarchy.GetRoot());
        }

        return roots;
    }
}