using Castle.Core.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Categories.CosmosDb.Tests;

public class CosmosStorageTests
{
    CosmosClient testClient;
    IConfigurationRoot testConfigRoot;
    Container container;

    [SetUp]
    public async Task Setup()
    {
        var configData = new Dictionary<string, string>
        {
            { "DatabaseName", "test-catalogue" },
            {
                "ConnectionString",
                "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
            }
        };

        testConfigRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();


        var emulatorDatabase = testConfigRoot.GetSection("DatabaseName");
        TestContext.WriteLine(emulatorDatabase.Value);

        var cosmosClientOptions = new CosmosClientOptions()
        {
            SerializerOptions = new CosmosSerializationOptions()
                { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true },
            AllowBulkExecution = true
        };

        var emulatorCnString = testConfigRoot.GetSection("ConnectionString");
        testClient = new CosmosClient(emulatorCnString.Value, cosmosClientOptions);

        // ensure the DB and container are available
        DatabaseResponse databaseResponse = await testClient.CreateDatabaseIfNotExistsAsync(emulatorDatabase.Value);
        TestContext.WriteLine("Database created: " + databaseResponse.Resource.LastModified.Value);

        var containerResponse =
            await databaseResponse.Database.CreateContainerIfNotExistsAsync("categories", "/rootId");

        container = containerResponse.Container;
    }

    [TearDown]
    public async Task TearDown()
    {
        await container.DeleteContainerAsync();
    }
    
    [Test]
    public async Task ReadAll_ShouldReturn2TreesWith3ItemsEach()
    {
        var forest = new CategoryForest();
        var rootCategory = new Category { Id = "root1", Name = "Root One", IsRoot = true };
        var cat1 = new Category() { Id = "cat1", Name = "Category One", Parent = rootCategory };
        var cat2 = new Category() { Id = "cat2", Name = "Category Two", Parent = cat1 };
        var list1 = new List<Category> { rootCategory, cat1, cat2 };

        var rootCat2 = new Category() { Id = "root2", Name = "Root Two", IsRoot = true };
        var cat21 = new Category() { Id = "cat21", Name = "Category Two One", Parent = rootCat2 };
        var cat22 = new Category() { Id = "cat22", Name = "Category Two Two", Parent = cat21 };
        var list2 = new List<Category> { rootCat2, cat21, cat22 };

        forest.AddTree(list1);
        forest.AddTree(list2);

        var sut = new CosmosStorage(testClient, testConfigRoot);
        await sut.Store(forest); // I don't yet get anything useful back out of this. (Should be a list of errors)
        
        // This part gets the items directly from the database. Superseded by forest.GetForestCount()
        // using FeedIterator<CategoryDTO> feed = container.GetItemQueryIterator<CategoryDTO>(
        //     queryText: "SELECT * FROM categories");
        //
        // int resultCount = 0;
        // while (feed.HasMoreResults)
        // {
        //     var readResult= await feed.ReadNextAsync();
        //     TestContext.WriteLine(readResult.Count);
        //     resultCount += readResult.Count;
        // }
        //
        // Assert.That(resultCount, Is.EqualTo(6));

        var readForest = await sut.ReadAll();
        var forestItems = readForest.Get();
        
        Assert.That(forest.GetForestCount(), Is.EqualTo(6), "Count does not equal 6");
        Assert.That(forestItems.Keys.Count, Is.EqualTo(2), "Expected 2 trees");
        Assert.That(forestItems, Does.ContainKey("Root One"));
        Assert.That(forestItems, Does.ContainKey("Root Two"));
    }

    [Test]
    public async Task Store_ShouldAdd5NodesThenDeleteLeafNode()
    {
        var forest = new CategoryForest();
        var rootCategory = new Category { Id = "root100", Name = "Root00 One", IsRoot = true };
        var cat1 = new Category() { Id = "cat100", Name = "Category00 One", Parent = rootCategory };
        var cat2 = new Category() { Id = "cat200", Name = "Category00 Two", Parent = cat1 };
        var list1 = new List<Category> { rootCategory, cat1, cat2 };

        var rootCat2 = new Category() { Id = "root200", Name = "Root00 Two", IsRoot = true };
        var cat21 = new Category() { Id = "cat2100", Name = "Category00 Two One", Parent = rootCat2 };
        var cat22 = new Category() { Id = "cat2200", Name = "Category00 Two Two", Parent = cat21 };
        var list2 = new List<Category> { rootCat2, cat21, cat22 };

        forest.AddTree(list1);
        forest.AddTree(list2);

        var sut = new CosmosStorage(testClient, testConfigRoot);
        await sut.Store(forest);
        
        // Verify Arrange
        TestContext.WriteLine("Verify arrange step.");
        var readForestInitial = await sut.ReadAll();
        var forestItemsInitial = readForestInitial.Get();
        var treeFoundInitial = forestItemsInitial.TryGetValue("Root00 Two", out var categoryTreeInitial);
        
        Assert.That(treeFoundInitial, Is.True, "Test Arrange step failed. Failed to get the tree.");
        
        var testCategoryInitial = categoryTreeInitial.Get();
        var retrieveSuccessfulInitial = testCategoryInitial.TryGetValue("cat2200", out var foundItemInitial);

        Assert.That(retrieveSuccessfulInitial, Is.True, "Test Arrange step failed. Failed to find the subject category after initial write.");
        
        // Act
        TestContext.WriteLine("Acting...");

        // TODO: There's a usage problem here - fair bit of process load on the person using it.
        var tryDeleteResult = forest.TryDeleteNodeFromTree("Root00 Two", "cat2200", out string resultReason);
        if (tryDeleteResult)
        {
            await sut.Store(forest);
        }

        var readForest = await sut.ReadAll();
        var forestItems = readForest.Get();
        var treeFound = forestItems.TryGetValue("Root00 Two", out var categoryTree);
        Assert.That(treeFound, Is.True, "Act step failed. Failed to get the tree");
        var category = categoryTree.Get();
        var retrieveSuccessful = category.TryGetValue("cat2200", out var foundItem);
        
        // Assert that we deleted a leaf from the tree
        Assert.That(retrieveSuccessful, Is.False, "Failed to delete the subject category.");
    }
    
    [Test]
        public async Task Store_ShouldAdd5NodesThenRejectDeleteOfBranchNode()
    {
        var forest = new CategoryForest();
        var rootCategory = new Category { Id = "root100", Name = "Root00 One", IsRoot = true };
        var cat1 = new Category() { Id = "cat100", Name = "Category00 One", Parent = rootCategory };
        var cat2 = new Category() { Id = "cat200", Name = "Category00 Two", Parent = cat1 };
        var list1 = new List<Category> { rootCategory, cat1, cat2 };

        var rootCat2 = new Category() { Id = "root200", Name = "Root00 Two", IsRoot = true };
        var cat21 = new Category() { Id = "cat2100", Name = "Category00 Two One", Parent = rootCat2 };
        var cat22 = new Category() { Id = "cat2200", Name = "Category00 Two Two", Parent = cat21 };
        var list2 = new List<Category> { rootCat2, cat21, cat22 };

        forest.AddTree(list1);
        forest.AddTree(list2);

        var sut = new CosmosStorage(testClient, testConfigRoot);
        await sut.Store(forest);
        
        // Verify Arrange
        TestContext.WriteLine("Verify arrange step.");
        var readForestInitial = await sut.ReadAll();
        var forestItemsInitial = readForestInitial.Get();
        var treeFoundInitial = forestItemsInitial.TryGetValue("Root00 Two", out var categoryTreeInitial);
        
        Assert.That(treeFoundInitial, Is.True, "Test Arrange step failed. Failed to get the tree.");
        
        var testCategoryInitial = categoryTreeInitial.Get();
        var retrieveSuccessfulInitial = testCategoryInitial.TryGetValue("cat2100", out var foundItemInitial);

        Assert.That(retrieveSuccessfulInitial, Is.True, "Test Arrange step failed. Failed to find the subject category after initial write.");
        
        // Act
        TestContext.WriteLine("Acting...");

        // TODO: There's a usage problem here - fair bit of process load on the person using it.
        var tryDeleteResult = forest.TryDeleteNodeFromTree("Root00 Two", "cat2100", out string resultReason);
        if (tryDeleteResult)
        {
            await sut.Store(forest);
        }

        var readForest = await sut.ReadAll();
        var forestItems = readForest.Get();
        var treeFound = forestItems.TryGetValue("Root00 Two", out var categoryTree);
        Assert.That(treeFound, Is.True, "Act step failed. Failed to get the tree");
        var category = categoryTree.Get();
        var retrieveSuccessful = category.TryGetValue("cat2100", out var foundItem);
        
        // Assert that we deleted a leaf from the tree
        Assert.That(retrieveSuccessful, Is.True, "Category is missing but should still exist.");
    }

}