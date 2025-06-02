using System.Text.Json;

namespace OpenGl_Game.Engine;

public class Settings
{
    private static Settings _instance;
    public static Settings Instance
    {
        get
        {
            if (_instance == null) _instance = LoadSettings();
            return _instance;
        }
    }
    
    public int WindowMode { get; set; }
    public float PlayerFov { get; set; }
    public float PlayerSensitivity { get; set; }
    public int EarthResolution { get; set; }
    public int TextureQuality { get; set; }
    public int MapCitiesMinPop { get; set; }

    private static Settings LoadSettings()
    {
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(RenderEngine.DirectoryPath + "settings.json"))!;
    }

    public static void ReloadSettings()
    {
        _instance = LoadSettings();
    }

    public void SaveSettings()
    {
        using var sw = new StreamWriter(RenderEngine.DirectoryPath + "settings.json");
        sw.WriteLine(JsonSerializer.Serialize(this));
        sw.Close();
    }
}