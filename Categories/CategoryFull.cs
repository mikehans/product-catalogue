﻿namespace Categories;

public class CategoryBasic
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class Category : CategoryBasic
{
    public bool? IsRoot { get; init; }
    public Category? Parent { get; set; }
    internal bool? IsDeleting { get; set; }
}

public class CategoryFull: Category
{
    public ICollection<Category>? Ancestors { get; init; }
}