﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Categories.Tests")]

namespace Categories;

/*
 * What does this need to do?
 * - create a new tree with a root (ctor)
 * - add an item to the tree (given a parent) 
 * - get the whole tree
 * - get the root
 * - find children given a parent
 * - validate
 *      - Insert: if parent exists
 *      - Delete: if no children exist
 */
/// <summary>
/// Represents a single category hierarchy.
/// Internally, maintains a Dictonary (of string, CategoryFull).
/// The key of each node is the ID.
/// This is intended to be accessed only through the Aggregate Root (a CategoryForest).
/// </summary>
public class CategoryTree
{
    private readonly Dictionary<string, CategoryFull> _hierarchy;

    /// <summary>
    /// Constructor. Initialises the internal dictionary with a root node.
    /// </summary>
    /// <param name="rootCategory"></param>
    internal CategoryTree(CategoryBasic rootCategory)
    {
        var category = new CategoryFull
        {
            Id = rootCategory.Id,
            Name = rootCategory.Name,
            IsRoot = true
        };
        _hierarchy = new Dictionary<string, CategoryFull> { { rootCategory.Id, category } };
    }

    /// <summary>
    /// Returns the internal dictionary.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, CategoryFull> Get()
    {
        return _hierarchy;
    }

    /// <summary>
    /// Returns the root node of the tree.
    /// </summary>
    /// <returns>The complete root node</returns>
    internal CategoryFull GetRoot()
    {
        var values = _hierarchy.Select(kvp => kvp.Value);
        var rootCategory = values.First(v => v.IsRoot == true);

        return rootCategory;
    }

    /// <summary>
    /// Finds nodes in the tree given a parent node ID.
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns>A list of IDs, enabling full retrieval by key</returns>
    private List<string> FindNodesWithParentId(string parentId)
    {
        var values = _hierarchy.Values.ToList();
        var foundItems = values.FindAll(item => item.Parent.Id == parentId).Select(item => item.Id);
        return foundItems.ToList();
    }

    internal bool ValidateInsertIfParentExists(string parentId)
    {
        return FindNodesWithParentId(parentId).Count == 0;
    }

    internal bool ValidateDeleteIfNoChildren(string itemId)
    {
        var items = _hierarchy.Values.ToList();

        var foundItem = items.Find(item => item.Id == itemId);

        return FindNodesWithParentId(itemId).Count == 0;
    }

    /// <summary>
    /// Inserts a node (non-root). The parent must already exist in the tree.
    /// </summary>
    /// <param name="childCategory">The new node being added</param>
    /// <param name="parent">The parent node</param>
    /// <returns></returns>
    internal string InsertChild(Category childCategory, Category parent)
    {
        var resultCategory = new CategoryFull
        {
            Id = childCategory.Id,
            Name = childCategory.Name,
            Ancestors = new List<Category>(),
            Parent = parent
        };
        resultCategory.Ancestors.Add(parent);

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

        return childCategory.Id;
    }
}