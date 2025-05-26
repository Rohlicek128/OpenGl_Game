using OpenGl_Game.Engine;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Targets;

public class CityTargets
{
    private static CityTargets _instance;
    public static CityTargets Instance
    {
        get
        {
            if (_instance == null) _instance = new CityTargets();
            return _instance;
        }
    }
    
    public List<City> Cities { get; set; }
    public List<Country> Countries { get; set; }

    private Dictionary<int, List<City>> _cachedCitiesByPop;

    public CityTargets()
    {
        Cities = [];
        Countries = [];
        _cachedCitiesByPop = [];
        LoadCities(RenderEngine.DirectoryPath + @"Assets\Dataset\worldcities.csv");
    }

    public List<City> CitiesWithPop(int minimumPop)
    {
        _cachedCitiesByPop.TryGetValue(minimumPop, out var result);
        
        result ??= Cities.Where(c => c.Population >= minimumPop).ToList();
        
        _cachedCitiesByPop.TryAdd(minimumPop, result);
        return result;
    }

    public City FindCityOnCoords(Vector2 coords, float size, int minimumPop, bool popScaling = false)
    {
        var cities = CitiesWithPop(minimumPop);
        
        return cities.Find(c => (coords - c.Coordinates).Length <= size * (popScaling ? c.Population / (37732000f - minimumPop) * 2f + 1f : 1f));
    }

    private void LoadCities(string path)
    {
        using var sr = new StreamReader(path);

        var line = sr.ReadLine();
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Remove(0, 1);
            line = line.Remove(line.Length - 1, 1);
            var elements = line.Split("\",\"");

            Country country;
            if (!Countries.Any(c => c.Name.Equals(elements[4])))
            {
                country = new Country(
                    elements[4],
                    elements[6],
                    Vector2.Zero
                );
                Countries.Add(country);
            }
            else
            {
                country = Countries.Find(c => c.Name.Equals(elements[4]));
            }

            var capital = elements[8] switch
            {
                "primary" => Capital.Primary,
                "admin" => Capital.Admin,
                "minor" => Capital.Minor,
                _ => Capital.None
            };

            float.TryParse(elements[9], out var population);
            
            Cities.Add(new City(
                int.Parse(elements[10]),
                elements[1],
                (int)population,
                new Vector2(float.Parse(elements[2]), float.Parse(elements[3])),
                country,
                elements[7],
                capital
            ));
        }
    }
}