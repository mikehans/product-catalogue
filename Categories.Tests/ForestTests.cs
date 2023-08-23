using NUnit.Framework.Internal;

namespace Categories.Tests;

[TestFixture]
public class ForestTests
{
    [Test]
    public void CreateForest_ShouldReturnForestWithTwoTrees()
    {
        var list1 = new List<Category>();
        var root1 = new Category
        {
            Id = "root1",
            Name = "Root 1",
            IsRoot = true
        };
        list1.Add(root1);
        var i1 = new Category
        {
            Id = "cat1",
            Name = "Category 1",
            Parent = root1
        };
        list1.Add(i1);
        var i2 = new Category
        {
            Id = "cat2",
            Name = "Category 2",
            Parent = root1
        };
        list1.Add(i2);
        var i3 = new Category
        {
            Id = "cat1Sub1",
            Name = "Cat 1 Sub 1",
            Parent = i1
        };
        list1.Add(i3);

        var list2 = new List<Category>();
        var root2 = new Category()
        {
            Id = "root2",
            Name = "Root 2",
            IsRoot = true
        };
        list2.Add(root2);
        var t1 = new Category()
        {
            Id = "cat1",
            Name = "Category 1 Root 2",
            Parent = root2
        };
        list2.Add(t1);
        
        Assert.Fail("Empty test");
    }

    [Test]
    public void CreateTree_ShouldReturnGivenValidInput()
    {
        // Note that this tree must have its nodes inserted so that the parent exists before the children
        var newTree = new List<Category>();

        var root = new Category
        {
            Id = "root",
            Name = "Men's",
            IsRoot = true
        };
        newTree.Add(root);

        var cat1 = new Category()
        {
            Id = "cat1",
            Name = "Shirts",
            Parent = root
        };
        newTree.Add(cat1);

        newTree.Add(new()
        {
            Id = "cat1a",
            Name = "T-shirts",
            Parent = cat1
        });
        
        var sut = new CategoryForest();
        sut.AddTree(newTree);
        Assert.That(sut.Get(), Does.ContainKey("Men's"));
    }
    
}