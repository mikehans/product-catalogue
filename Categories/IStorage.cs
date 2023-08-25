namespace Categories;

public interface IStorage
{
    public Task Store(CategoryForest forest);
    public Task<CategoryForest> ReadAll();
}