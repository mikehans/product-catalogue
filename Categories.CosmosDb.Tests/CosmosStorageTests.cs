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
                { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true }
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
        // await container.DeleteContainerAsync();
    }
    
    [Test]
    public async Task ReadAll_ShouldReturn2TreesWith3ItemsEach()
    {
        // Act
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
        await sut.Store(forest); // I don't yet get anything useful back out of this.
        
        using FeedIterator<CategoryDTO> feed = container.GetItemQueryIterator<CategoryDTO>(
            queryText: "SELECT * FROM categories");

        int resultCount = 0;
        while (feed.HasMoreResults)
        {
            var readResult= await feed.ReadNextAsync();
            TestContext.WriteLine(readResult.Count);
            resultCount += readResult.Count;
        }
        
        Assert.That(resultCount, Is.EqualTo(6));

        var readForest = await sut.ReadAll();
        // var forestItems = readForest.Get();
        // Assert.That(forestItems, Does.ContainKey("Root One"));
        // Assert.That(forestItems, Does.ContainKey("Root Two"));

    }
}