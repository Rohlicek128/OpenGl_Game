using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Targets;

public struct City
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    public int Population { get; set; }
    public Vector2 Coordinates { get; set; }
    
    public Country Country { get; set; }
    public string Region { get; set; }
    public Capital Capital { get; set; }

    public City(int id, string name, int population, Vector2 coordinates, Country country, string region, Capital capital)
    {
        Id = id;
        Name = name;
        Population = population;
        Coordinates = coordinates;
        Country = country;
        Region = region;
        Capital = capital;
    }
}

public enum Capital
{
    Primary, Admin, Minor, None
}