namespace Categories.Tests;

[TestFixture]
public class MultiHierarchyTests
{
    [Test]
    public void AddTwoHierarchies_ShouldReturnTwoSeparateHierarchies()
    {
        var hierarchy1 = BuildHierarchy(1);
        var hierarchy2 = BuildHierarchy(2);

        var sut = new HierarchyCollection();

        sut.Add(hierarchy1);
        sut.Add(hierarchy2);

        var h = sut.Get();
        Assert.That(h, Has.Count.EqualTo(2));

        var rootCategories = sut.GetRootCategories();
        Assert.That(rootCategories, Has.Count.EqualTo(2));
    }
    
    public CategoryHierarchy BuildHierarchy(int hierarchyNumber)
    {
        var rootCategory = new CategoryBriefForm
        {
            Id = $"Cat{hierarchyNumber}-TestRootCategory",
            Name = $"Cat{hierarchyNumber}- A Test Root Category"
        };

        var subcat1 = new CategoryBriefForm
        {
            Id = $"Cat{hierarchyNumber}-SubCat1",
            Name = $"Cat{hierarchyNumber}- Sub Cat 1"
        };

        var subcat2 = new CategoryBriefForm
        {
            Id = $"Cat{hierarchyNumber}-SubCat2",
            Name = $"Cat{hierarchyNumber}- Sub Cat 2"
        };

        var subcat1Cat1 = new CategoryBriefForm
        {
            Id = $"Cat{hierarchyNumber}-SubCat1Cat1",
            Name = $"Cat{hierarchyNumber}- Sub Cat 1 Cat 1"
        };
        var subcat1Cat2 = new CategoryBriefForm
        {
            Id = $"Cat{hierarchyNumber}-SubCat1Cat2",
            Name = $"Cat{hierarchyNumber}- Sub Cat 1 Cat 2"
        };

        var hierarchy = new CategoryHierarchy(rootCategory);
        hierarchy.InsertChild(subcat1, rootCategory);
        hierarchy.InsertChild(subcat2, rootCategory);
        hierarchy.InsertChild(subcat1Cat1, subcat1);
        hierarchy.InsertChild(subcat1Cat2, subcat1);

        return hierarchy;
    }
}