using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Gauges;
using OpenGl_Game.Game.Gauges.Battery;
using OpenGl_Game.Game.Gauges.Speed;
using OpenGl_Game.Game.Gauges.Turn;
using OpenGl_Game.Game.Screens;
using OpenGl_Game.Game.Screens.Navigation;
using OpenGl_Game.Game.Screens.Objective;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Station
{
    public Vector2 Coordrinates { get; set; }
    public EngineObject StationObject { get; set; }

    public Dictionary<int, ButtonHandler> Buttons { get; set; }
    public Dictionary<int, ScreenHandler> Screens { get; set; }
    
    public static float BatteryPercentage { get; set; }

    private TimerManager _timerManager;
    
    public Station()
    {
        Buttons = [];
        Screens = [];
        BatteryPercentage = 0.5f;
        _timerManager = new TimerManager(1);
        
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(3f), Vector3.Zero, Vector3.One * 1f),
            MeshConstructor.LoadObjFromFileAssimp(@"station\stationV4_3.obj"),
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
            new Vector3(-0.6995512f, 0.76773214f, 0.68621445f),
            new Vector3(-1.1603967f, -0.18453844f, 2.7553217f),
            new Vector3(0.05f)
        ), "Objective Power"));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new PageSetterButton(Screens.Last().Value, 0, new Transform(
            new Vector3(-0.7672795f, 1.1283766f, 0.83934915f),
            new Vector3(-1.1603967f, -0.18453844f, 2.7553217f),
            new Vector3(0.05f)
        ), "Objective"));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new PageSetterButton(Screens.Last().Value, 1, new Transform(
            new Vector3(-0.7554139f, 1.0647875f, 0.81193936f),
            new Vector3(-1.1603967f, -0.18453844f, 2.7553217f),
            new Vector3(0.05f)
        ), "Log"));
        
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new NavigationScreen(new Vector2i(600, 600)));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new ScreenPowerButton(Screens.Last().Value, new Transform(
            new Vector3(-0.6736321f, 0.7660475f, -0.6361099f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), "Navigation Power"));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new PageSetterButton(Screens.Last().Value, 0, new Transform(
            new Vector3(-0.74636537f, 1.1241637f, -0.7801374f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), "Map"));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new PageSetterButton(Screens.Last().Value, 1, new Transform(
            new Vector3(-0.7306417f, 1.053911f, -0.7522063f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), "Aim"));
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new BatteryGauge(new Vector2i(600, 600)));
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new TurnGauge(new Vector2i(150, 150)));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new TurnButton(-1f, (TurnPage)Screens.Values.Last().Pages[0], new Transform(
            new Vector3(-1.0564964f, 0.688166f, -0.42037487f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), _timerManager, "Turn Left"));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new TurnButton(1f, (TurnPage)Screens.Values.Last().Pages[0], new Transform(
            new Vector3(-0.8259029f, 0.688166f, -0.5308491f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), _timerManager, "Turn Right"));
        
        Screens.Add(EngineObject.ObjectIdCounter + 1, new SpeedGauge(new Vector2i(150, 150)));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new SpeedButton(-5f, (SpeedPage)Screens.Values.Last().Pages[0], new Transform(
            new Vector3(-0.7538492f, 0.688166f, -0.56354666f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), "Less Speed"));
        Buttons.Add(EngineObject.ObjectIdCounter + 1, new SpeedButton(5f, (SpeedPage)Screens.Values.Last().Pages[0], new Transform(
            new Vector3(-0.4924112f, 0.688166f, -0.6850492f),
            new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
            new Vector3(0.05f)
        ), "More Speed"));
    }
    
}