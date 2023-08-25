using NUnit.Framework.Internal;

namespace Categories.Tests;

[TestFixture]
public class ForestTests
{
    [Test]
    [Description("The basic happy path test for two trees")]
    public void CreateForest_ShouldReturnForestWithTwoTrees_GivenValidInput()
    {
        #region TestData    
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

        #endregion
        
        var sut = new CategoryForest();
        sut.AddTree(list1);
        sut.AddTree(list2);

        Assert.That(sut.Get().Count(),Is.EqualTo(2), "Expected 2 trees in forest");
        Assert.That(sut.Get(), Does.ContainKey("Root 1"));
        Assert.That(sut.Get(), Does.ContainKey("Root 2"));
    }

    [Test]
    [Description("The basic happy path test")]
    public void CreateTree_ShouldReturnForestWithOneTree_GivenValidInput()
    {
        #region TestData
        // Note that this tree must have its nodes inserted in order, so that the parent exists before the children
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
        #endregion

        var sut = new CategoryForest();
        sut.AddTree(newTree);
        Assert.That(sut.Get().Count, Is.EqualTo(1), "Expected one tree in the forest.");
        Assert.That(sut.Get(), Does.ContainKey("Men's"));
    }
    
    [Test]
    public void TryDeleteNodeFromTree_ShouldDeleteLeafNode()
    {
        #region TestData
        // Note that this tree must have its nodes inserted in order, so that the parent exists before the children
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
        #endregion

        var sut = new CategoryForest();
        sut.AddTree(newTree);
        var treeByKey = sut.TryGetTreeByKey("Men's", out var foundTree);
        Assert.That(treeByKey, Is.True);
        Assert.That(foundTree.Get(), Does.ContainKey("cat1a"));

        var hasDeletedNode = sut.TryDeleteNodeFromTree("Men's", "cat1a", out string? reason);
        Assert.That(hasDeletedNode, Is.True, "Failed to delete the leaf node");
        Assert.That(reason, Is.EqualTo("Node successfully marked for deletion."));
    }
    
    [Test]
    public void TryDeleteNodeFromTree_ShouldFailToDeleteBranchNode()
    {
        #region TestData
        // Note that this tree must have its nodes inserted in order, so that the parent exists before the children
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
        #endregion

        var sut = new CategoryForest();
        sut.AddTree(newTree);
        var treeByKey = sut.TryGetTreeByKey("Men's", out var foundTree);
        Assert.That(treeByKey, Is.True);
        Assert.That(foundTree.Get(), Does.ContainKey("cat1"));

        var hasDeletedNode = sut.TryDeleteNodeFromTree("Men's", "cat1", out string? reason);
        Assert.That(hasDeletedNode, Is.False, "Deleted the leaf node");
        Assert.That(reason, Is.EqualTo("Could not delete node. Node has children."));
    }
}