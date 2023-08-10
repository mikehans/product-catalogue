using System.Runtime.Serialization.Json;
using NUnit.Framework;

namespace Categories.Tests;

[TestFixture]
public class CategoryTests
{
    [Test]
    public void CreatingANewCategoryHierarchy_ShouldIncludeRootCategory()
    {
        var category = new CategoryBriefForm
        {
            Id = "TestRootCategory",
            Name = "A Test Category"
        };

        CategoryHierarchy sut = new CategoryHierarchy(category);

        Assert.That(sut.Get(), Has.Count.EqualTo(1));

        var retrieved = sut.Get();
        var retrievedRoot = retrieved["TestRootCategory"];
        Assert.That(retrievedRoot.IsRoot, Is.True);

        var root = sut.GetRoot();
        Assert.That(root.Id, Is.EqualTo("TestRootCategory"));
    }

    [Test]
    public void AddingSubCategoriesHierarchyToRootCategory_ShouldDoStuff()
    {
        var sut = BuildHierarchy();

        var retrieved = sut.Get();
        Assert.That(retrieved, Has.Count.EqualTo(5));

        var retrievedRootCategory = retrieved["TestRootCategory"];
        Assert.Multiple(() =>
        {
            Assert.That(retrievedRootCategory.Parent, Is.Null);
            Assert.That(retrievedRootCategory.Ancestors, Is.Null);
        });

        var retrievedSubCat1 = retrieved["SubCat1"];
        Assert.Multiple(() =>
        {
            Assert.That(retrievedSubCat1.Parent.Id, Is.EqualTo("TestRootCategory"),
                "Expectation on subCat1 parent failed");
            Assert.That(retrievedSubCat1.Ancestors, Has.Count.EqualTo(1), "Expectation on SubCat1 ancestors failed");
        });

        var retrievedSubCat1Cat1 = retrieved["SubCat1Cat1"];
        Assert.Multiple(() =>
        {
            Assert.That(retrievedSubCat1Cat1.Parent.Id, Is.EqualTo("SubCat1"),
                "Expectation on SubCat1Cat1 parent failed");
            Assert.That(retrievedSubCat1Cat1.Ancestors, Has.Count.EqualTo(2),
                "Expectation on SubCat1Cat1 ancestors failed");
        });
    }
    
    public CategoryHierarchy BuildHierarchy()
    {
        var rootCategory = new CategoryBriefForm
        {
            Id = "TestRootCategory",
            Name = "A Test Root Category"
        };

        var subcat1 = new CategoryBriefForm
        {
            Id = "SubCat1",
            Name = "Sub Cat 1"
        };

        var subcat2 = new CategoryBriefForm
        {
            Id = "SubCat2",
            Name = "Sub Cat 2"
        };

        var subcat1Cat1 = new CategoryBriefForm
        {
            Id = "SubCat1Cat1",
            Name = "Sub Cat 1 Cat 1"
        };
        var subcat1Cat2 = new CategoryBriefForm
        {
            Id = "SubCat1Cat2",
            Name = "Sub Cat 1 Cat 2"
        };

        var hierarchy = new CategoryHierarchy(rootCategory);
        hierarchy.InsertChild(subcat1, rootCategory);
        hierarchy.InsertChild(subcat2, rootCategory);
        hierarchy.InsertChild(subcat1Cat1, subcat1);
        hierarchy.InsertChild(subcat1Cat2, subcat1);

        return hierarchy;
    }
}