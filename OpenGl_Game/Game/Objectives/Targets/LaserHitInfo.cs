using System.Text;
using OpenGl_Game.Game.Objectives.Targets;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Objectives.Targets;

public class LaserHitInfo
{
    private static uint _idCount;
    
    public uint Id { get; set; }
    public Vector2? StartCoords { get; set; }
    public Vector2? EndCoords { get; set; }
    public List<City> HitCities { get; set; }
    public List<Country> HitCountries { get; set; }
    public Objective? HitObjective { get; set; }

    public LaserHitInfo(Vector2? startCoords = null)
    {
        Id = _idCount;
        _idCount++;
        HitCities = [];
        HitCountries = [];
        StartCoords = startCoords;
    }

    public string GetCountriesToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < HitCountries.Count; i++)
        {
            sb.Append(HitCountries[i].Name);
            if (i < HitCountries.Count - 1) sb.Append(", ");
        }
        return sb.ToString();
    }
}