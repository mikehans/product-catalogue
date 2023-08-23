namespace Categories;

public interface IStorage
{
    public Task<bool> Store(CategoryForest forest);
    public Task<CategoryForest> ReadAll();
}