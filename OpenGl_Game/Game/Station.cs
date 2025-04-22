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
    public EngineObject StationObject { get; set; }

    public Dictionary<int, ButtonHandler> Buttons { get; set; }
    public Dictionary<int, ScreenHandler> Screens { get; set; }
    
    public static float BatteryPercentage { get; set; }
    
    public Station()
    {
        Buttons = [];
        Screens = [];
        BatteryPercentage = 0.5f;
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(3f), Vector3.Zero, Vector3.One * 1.25f),
            MeshConstructor.LoadObjFromFileAssimp(@"station\stationV3_3.obj"),
            new Material(new Vector3(1f))
        );
        StationObject.Material.Shininess = 1f;
        StationObject.Transform.Quaternion = Quaternion.FromEulerAngles(MathF.PI * Vector3.UnitY) * StationObject.Transform.Quaternion;
        StationObject.Transform.Position = -StationObject.MeshData.BoundingBox.Origin;
        StationObject.Transform.Position.Y += 1f;

        StationObject.IsSelectable = false;
        
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new LaserButton());
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new SliderButton());
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new ObjectiveScreen(new Vector2i(600, 600)));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new ScreenPowerButton(Screens.Last().Value, new Transform(
            new Vector3(-2.3049996f, -0.30045396f, -0.48764002f),
            new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
            new Vector3(0.05f)
        )));
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new BatteryScreen(new Vector2i(600, 600)));
        //Buttons.Add(EngineObject.ObjectIdCounter + 1, new ScreenPowerButton(Screens.Last().Value, new Transform(
        //    new Vector3(-2.3049996f, -0.30045396f, 0.68764002f),
        //    new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
        //    new Vector3(0.05f)
        //)));
    }
    
}