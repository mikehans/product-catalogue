// See https://aka.ms/new-console-template for more information

using Categories;
var hierarchyCollection = new HierarchyCollection();


var clothingRoot = new CategoryBriefForm()
{   
    Id = Guid.NewGuid().ToString(),
    Name = "Clothing"
};

var hierarchy = new CategoryHierarchy(clothingRoot);
Console.WriteLine("Added root category");

var mens = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "Men's"
};

hierarchyCollection.Add(hierarchy);

var addedMens = hierarchy.InsertChild(mens, clothingRoot);

Console.WriteLine("Added mens category");

var mensShirts = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "Shirts"
};

var addedMensShirts = hierarchy.InsertChild(mensShirts, mens);

Console.WriteLine("Added mens shirts category");

var mensTShirts = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "T-Shirts"
};
var addedMensTShirts = hierarchy.InsertChild(mensTShirts, mensShirts);


var electronicsRoot = new CategoryBriefForm
{   
    Id = Guid.NewGuid().ToString(),
    Name = "Electronics"
};
var electronicsHierarchy = new CategoryHierarchy(electronicsRoot);
hierarchyCollection.Add(electronicsHierarchy);

var avEquipment = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "AV Equipment"
};
electronicsHierarchy.InsertChild(avEquipment, electronicsRoot);

var tvEquipment = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "TVs"
};
electronicsHierarchy.InsertChild(tvEquipment, avEquipment);

var hiFiEquipment = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "Hi-Fi"
};
electronicsHierarchy.InsertChild(hiFiEquipment, avEquipment);

var reallyBigTvs = new CategoryBriefForm
{
    Id = Guid.NewGuid().ToString(),
    Name = "Really big TVs"
};
electronicsHierarchy.InsertChild(reallyBigTvs, tvEquipment);

Console.WriteLine("DONE");
Console.ReadLine();
