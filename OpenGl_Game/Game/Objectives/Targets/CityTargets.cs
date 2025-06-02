using OpenGl_Game.Engine;
using OpenGl_Game.Game.Objectives.Targets;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Objectives.Targets;

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

    public City FindCityOnCoords(Vector2 coords, float radiusKm, int minimumPop, bool popScaling = false)
    {
        var cities = CitiesWithPop(minimumPop);
        
        return cities.Find(c =>
            (coords - c.Coordinates).Length / 360f * Earth.Circumference <= (popScaling ? PopToRadius(c.Population) * 5f : radiusKm)
            //(popScaling ?  c.Population / 37732000f * 2f + 1.25f : 1f)
            //(popScaling ? MathF.Log(c.Population / 37732000f / 1.5f, 2.5f) * 0.5f + 3.5f : 1f)
        );
    }

    public static float PopToRadius(int population)
    {
        return (6.5f / 29900000f) * population + (1f - (6.5f * 100000f) / 29900000f);
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