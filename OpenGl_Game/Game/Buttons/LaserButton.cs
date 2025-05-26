using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class LaserButton : ButtonHandler
{
    public LaserShader LaserShader { get; set; }
    public static bool IsShooting { get; set; }
    
    public LaserButton()
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            "Laser Button",
            new Transform(new Vector3(-0.9776639f, 0.6197455f, -0.34537253f), new Vector3(0f, 0f, 0f),
                new Vector3(0.075f)),
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f, 0f, 0f))
        );
        Type = ButtonTypes.Press;
    }

    private void SetButtonValue(bool down)
    {
        ButtonValue = down ? 1f : 0f;
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null || param[1] == null || Station.BatteryPercentage <= 0f) return;
        SetButtonValue((bool)param[0]);

        LaserShader.Objects[0].IsVisible = (int)ButtonValue == 1;
        IsShooting = LaserShader.Objects[0].IsVisible;
    }
}