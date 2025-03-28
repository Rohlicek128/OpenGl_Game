using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Buttons;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Station
{
    public EngineObject StationObject;

    public Dictionary<int, ButtonHandler> Buttons;
    
    public Station()
    {
        Buttons = [];
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(3f)),
            MeshConstructor.LoadObjFromFileAssimp(@"station\station_v05.obj", true),
            new Material(new Vector3(1f))
        );
        StationObject.Material.Shininess = 1f;
        StationObject.Transform.Quaternion = Quaternion.FromEulerAngles(MathF.PI * Vector3.UnitY) * StationObject.Transform.Quaternion;
        StationObject.Transform.Position = -StationObject.MeshData.BoundingBox.Origin;
        StationObject.Transform.Position.X -= 6f;

        StationObject.IsSelectable = false;
        
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new LaserButton());
    }
    
}