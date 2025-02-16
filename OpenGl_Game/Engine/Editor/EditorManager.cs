using System.Text.Json;
using OpenGl_Game.Engine.Objects;

namespace OpenGl_Game.Engine.Editor;

public class EditorManager
{
    public List<EngineObject> Objects;

    public EditorManager()
    {
        Objects = [];
    }

    public void Save(EngineObject obj)
    {
        var json = JsonSerializer.Serialize(obj);
        using var sw = new StreamWriter(RenderEngine.DirectoryPath + @"Engine\Editor\Saved\" + obj.Name.Replace(" ", "") + ".json");
        sw.Write(json);
        sw.Flush();
        sw.Close();
    }
}