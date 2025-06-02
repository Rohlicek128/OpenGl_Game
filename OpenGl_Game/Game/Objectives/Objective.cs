using System.Text.Json;

namespace OpenGl_Game.Game.Objectives;

public class Objective
{
    public int Day { get; set; }
    
    public string Header { get; set; }
    public string Description { get; set; }
    public ObjectiveType Type { get; set; }
    
    public string Target { get; set; }
    public string Country { get; set; }
    public float TargetLongitude { get; set; }
    public float TargetLatitude { get; set; }
    public float Size { get; set; }
    
    public float Pay { get; set; }
    public bool IsCompleted;

    public Objective(int day, string header, string description, ObjectiveType type, string target, string country, float targetLongitude, float targetLatitude, float size, float pay)
    {
        Day = day;
        Header = header;
        Description = description;
        Type = type;
        Target = target;
        Country = country;
        TargetLongitude = targetLongitude;
        TargetLatitude = targetLatitude;
        Size = size;
        Pay = pay;
        
        IsCompleted = false;
    }

    public void Save(string path)
    {
        using var sw = new StreamWriter(path);
        sw.WriteLine(JsonSerializer.Serialize(this));
        sw.Close();
    }
}