﻿using System.Collections.ObjectModel;

namespace Categories;

/*
 * Aggregate root (forest containing trees)
 * - Create a new forest (ctor)
 * - Get the whole forest
 * - Get a Tree by its ID
 * - Get roots
 * - Add a new tree to the forest
 *      - use CategoryTree ctor
 *      - insert children
 *  - insert an item into a tree
 */

/// <summary>
/// Aggregate root. Maintains a forest of category trees.
/// Stores the forest in a Dictionary (of string, CategoryTree).
/// The key is the name used by the root element.
/// </summary>
public class CategoryForest
{
    private readonly Dictionary<string, CategoryTree> _forest = new();

    #region Queries

    public Dictionary<string, CategoryTree> Get()
    {
        return _forest;
    }

    /// <summary>
    /// Returns a CategoryTree by key.
    /// </summary>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public CategoryTree GetTreeByKey(string parentId)
    {
        return _forest[parentId];
    }

    /// <summary>
    /// Returns a collection of the root categories.
    /// Useful for cases like creating a site's top level navigation.
    /// </summary>
    /// <returns>A read only collection of top level categories</returns>
    public IReadOnlyCollection<Category> GetRootCategories()
    {
        var hierarchies = _forest.Select(h => h.Value);
        var roots = new List<Category>();

        foreach (var hierarchy in hierarchies)
        {
            roots.Add(hierarchy.GetRoot());
        }

        return roots;
    }

    /// <summary>
    /// Returns a node given the keys for the node and the parent
    /// </summary>
    /// <param name="key">The key of the item</param>
    /// <param name="treeId">The key of the parent tree</param>
    /// <returns>The full category</returns>
    public CategoryFull GetCategoryByKey(string key, string treeId)
    {
        var tree = _forest[treeId].Get();
        return tree[key];
    }

    #endregion

    #region Commands

    /// <summary>
    /// Adds an entire tree to the forest.
    /// </summary>
    /// <param name="newTreeItems"></param>
    /// <exception cref="ArgumentException">Emits when a tree with the same key (the root's name) exists</exception>
    public void AddTree(ICollection<Category> newTreeItems)
    {
        var root = FetchRootOfNewTree(newTreeItems);
        var newRoot = new CategoryBasic()
        {
            Id = root.Id,
            Name = root.Name
        };
        if (_forest.Any(x => x.Key == newRoot.Name))
        {
            throw new ArgumentException($"A tree with the key \"{newRoot.Name}\" already exists");
        }

        var newTree = new CategoryTree(newRoot);

        var nonRootItems = newTreeItems.Where(item => item.Parent != null).ToList();

        try
        {
            foreach (var item in nonRootItems)
            {
                newTree.InsertChild(item, item.Parent);
            }

            _forest.Add(newRoot.Name, newTree);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private Category FetchRootOfNewTree(ICollection<Category> newTreeItems)
    {
        var roots = newTreeItems.Where(item => item.IsRoot == true).ToList();
        if (roots.Count() != 1)
        {
            throw new InvalidNumberOfRootNodesException();
        }

        return roots.First();
    }

    #endregion
}