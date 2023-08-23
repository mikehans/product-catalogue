using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Categories.CosmosDb;

public class CosmosStorage : IStorage
{
    private readonly CosmosClient _client;
    private readonly IConfiguration _configuration;

    public CosmosStorage(CosmosClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<bool> Store(CategoryForest forest)
    {
        var container = GetContainer("categories");

        var forestDict = forest.Get().Values;
        foreach (var categoryTree in forestDict)
        {
            var treeDict = categoryTree.Get();
            var root = treeDict.Values.First(n => n.IsRoot is true);
            var partitionKey = root.Id;

            foreach (var categoryFull in treeDict.Values)
            {
                var categoryDto = new CategoryDTO()
                {
                    id = categoryFull.Id,
                    Name = categoryFull.Name,
                    RootId = partitionKey
                };

                if (categoryFull.IsRoot is true)
                {
                    categoryDto.IsRoot = true;
                }
                else
                {
                    categoryDto.Parent = new NestedCategoryDTO()
                        { id = categoryFull?.Parent?.Id, Name = categoryFull?.Parent?.Name };
                    categoryDto.Ancestors = BuildAncestorList(categoryFull);
                }

                try
                {
                    var itemResponse =
                        await container.UpsertItemAsync(categoryDto, new PartitionKey(categoryDto.RootId));
                    Console.WriteLine(itemResponse.StatusCode);
                }
                catch (CosmosException e)
                {
                    Console.WriteLine(e.ResponseBody);
                }
            }
        }

        return true;


        //  upsert each node (irrespective of whether it is root or not)

        // not yet in scope: deletes.
    }

    private static List<NestedCategoryDTO> BuildAncestorList(CategoryFull categoryFull)
    {
        var nestedCategoryDtos = new List<NestedCategoryDTO>();
        if (categoryFull.Ancestors is not null)
        {
            foreach (var ancestor in categoryFull.Ancestors)
            {
                nestedCategoryDtos.Add(new NestedCategoryDTO() { id = ancestor.Id, Name = ancestor.Name });
            }
        }

        return nestedCategoryDtos;
    }

    public async Task<CategoryForest> ReadAll()
    {
        var container = GetContainer("categories");

        var allCategories = await GetAllCategories(container);

        var forest = CreateForest(allCategories);

        return forest;
    }

    private static CategoryForest CreateForest(List<CategoryDTO> allCategories)
    {
        var forest = new CategoryForest();

        var grouping = allCategories.ToLookup(k => k.RootId);

        foreach (var group in grouping)
        {
            List<Category> newTree = new();

            AddRootToNewTree(group, newTree);

            // Get an ordered list of CategoryDTOs ordered by Ancestor count
            //     (indicates depth in the tree - creates insertion order)
            var nonRootCategories = group
                .Where(c => c.IsRoot is not true)
                .OrderBy(c => c.Ancestors.Count);

            AddChildNodesToTree(nonRootCategories, newTree);

            forest.AddTree(newTree);
        }

        return forest;
    }

    private static void AddChildNodesToTree(IOrderedEnumerable<CategoryDTO> nonRootCategories, List<Category> newTree)
    {
        foreach (var childCategory in nonRootCategories)
        {
            var cat = new Category()
            {
                Id = childCategory.id,
                Name = childCategory.Name,
                Parent = new Category()
                {
                    Id = childCategory.Parent.id,
                    Name = childCategory.Parent.Name
                }
            };
            newTree.Add(cat);
        }
    }

    private static void AddRootToNewTree(IGrouping<string, CategoryDTO> group, List<Category> newTree)
    {
        // find roots
        var root = group.First(c => c.IsRoot is true);

        Console.WriteLine(root);
        var rootCat = new Category()
        {
            Id = root.id,
            Name = root.Name,
            IsRoot = root.IsRoot
        };
        newTree.Add(rootCat);
    }

    private static async Task<List<CategoryDTO>> GetAllCategories(Container container)
    {
        using var feedIterator = container.GetItemQueryIterator<CategoryDTO>(
            queryText: "SELECT * FROM categories"
        );

        List<CategoryDTO> allCategories = new();

        while (feedIterator.HasMoreResults)
        {
            var results = await feedIterator.ReadNextAsync();

            foreach (var result in results)
            {
                allCategories.Add(result);
            }
        }

        return allCategories;
    }

    private Container GetContainer(string containerName)
    {
        var database = _client.GetDatabase(_configuration["DatabaseName"]);
        var container = database.GetContainer(containerName);
        return container;
    }
}