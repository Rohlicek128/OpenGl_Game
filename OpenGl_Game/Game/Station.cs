using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Station
{
    public EngineObject StationObject;

    public Dictionary<int, ButtonHandler> Buttons;
    public Dictionary<int, ScreenHandler> Screens;
    
    public Station()
    {
        Buttons = [];
        Screens = [];
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(3f), Vector3.Zero, Vector3.One * 1.25f),
            MeshConstructor.LoadObjFromFileAssimp(@"station\stationV1_1.obj"),
            new Material(new Vector3(1f))
        );
        StationObject.Material.Shininess = 1f;
        StationObject.Transform.Quaternion = Quaternion.FromEulerAngles(MathF.PI * Vector3.UnitY) * StationObject.Transform.Quaternion;
        StationObject.Transform.Position = -StationObject.MeshData.BoundingBox.Origin;
        StationObject.Transform.Position.Y += 1f;

        StationObject.IsSelectable = false;
        
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new LaserButton());
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new ObjectiveScreen(new Vector2i(600, 600)));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new ScreenPowerButton(Screens.Last().Value));
    }
    
}