using Microsoft.Azure.Cosmos;

namespace Categories.CosmosDb;

public class CosmosStorage : IStorage
{
    private readonly CosmosClient _client;

    public CosmosStorage(CosmosClient client)
    {
        _client = client;
    }

    public bool Store(CategoryForest forest)
    {
        var database = _client.GetDatabase("product-catalogue");
        var container = database.GetContainer("categories");


        var forestDict = forest.Get().Values;
        foreach (var categoryTree in forestDict)
        {
            var treeDict = categoryTree.Get();
            var root = treeDict.Values.First(n => n.IsRoot is true);
            var partitionKey = root.Id;

            foreach (var categoryFull in treeDict.Values)
            {
                var nestedCategoryDtos = new List<NestedCategoryDTO>();
                foreach (var ancestor in categoryFull.Ancestors)
                {
                    nestedCategoryDtos.Add(new NestedCategoryDTO(){Id = ancestor.Id, Name = ancestor.Name});
                }
                var categoryDto = new CategoryDTO()
                {
                    Id = categoryFull.Id,
                    Name = categoryFull.Name,
                    PartitionKey = partitionKey,
                    Parent = new NestedCategoryDTO()
                        { Id = categoryFull?.Parent?.Id, Name = categoryFull?.Parent?.Name },
                    Ancestors = nestedCategoryDtos
                };
            }
        }


        //  upsert each node (irrespective of whether it is root or not)

        // not yet in scope: deletes.
    }

    public async Task<CategoryForest> ReadAll()
    {
        var database = _client.GetDatabase("product-catalogue");
        var container = database.GetContainer("categories");
        
        // read all from the container

        using var feedIterator = container.GetItemQueryIterator<CategoryDTO>(
            queryText: "SELECT * FROM categories"
        );

        List<CategoryDTO> allCategories = new ();
        
        while (feedIterator.HasMoreResults)
        {
            var results = await feedIterator.ReadNextAsync();

            foreach (var result in results)
            {
                allCategories.Add(result);
            }
        }

        var forest = new CategoryForest();

        var grouping = allCategories.ToLookup(k => k.PartitionKey);

        // find roots
        foreach (var group in grouping)
        {
            List<Category> newTree = new();
            var root = group.First(c => c.IsRoot is true);

            var rootCat = new Category()
            {
                Id = root.Id,
                Name = root.Name,
                IsRoot = root.IsRoot
            };
            newTree.Add(rootCat);

            // Get an ordered list of CategoryDTOs ordered by Ancestor count (indicates depth in the tree)
            var nonRootCategories = group.Where(c => c.IsRoot is not true).OrderBy(c => c.Ancestors.Count);
            
            // Group the above ordered list by the Parent ID (the root with Parent ID of null is already removed) 
            var parenting = nonRootCategories.GroupBy((c => c.Parent.Id));
            
            // push this into the domain
            foreach (var childCategory in nonRootCategories)
            {
                var cat = new Category()
                {
                    Id = childCategory.Id,
                    Name = childCategory.Name,
                    Parent = new Category()
                    {
                        Id = childCategory.Parent.Id,
                        Name = childCategory.Parent.Name
                    }
                };
                newTree.Add(cat);
            }
            
            forest.AddTree(newTree);
        }   

        return new CategoryForest();
    }
}