using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Objectives.Targets;

public struct City
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    public int Population { get; set; }
    public Vector2 Coordinates { get; set; }
    
    public Country Country { get; set; }
    public string Region { get; set; }
    public Capital Capital { get; set; }
    
    public bool IsDestroyed { get; set; }

    public City(int id, string name, int population, Vector2 coordinates, Country country, string region, Capital capital)
    {
        Id = id;
        Name = name;
        Population = population;
        Coordinates = coordinates;
        Country = country;
        Region = region;
        Capital = capital;
        IsDestroyed = false;
    }
}

public enum Capital
{
    None, Minor, Admin, Primary
}