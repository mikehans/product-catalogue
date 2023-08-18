namespace Categories;

public interface IStorage
{
    public bool Store(CategoryForest forest);
    public Task<CategoryForest> ReadAll();
}