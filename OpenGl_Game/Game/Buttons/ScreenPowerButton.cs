using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Screens;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class ScreenPowerButton : ButtonHandler
{
    private ScreenHandler _screen;
    public ScreenHandler Screen
    {
        get => _screen;
        set => _screen = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ScreenPowerButton(ScreenHandler screen)
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            "OS Power Button",
            new Transform(new Vector3(-4.9225183f, -0.3374207f, 0.9792786f), new Vector3(-MathF.PI/2f, -0.5290095f, -MathF.PI/2f),
                new Vector3(0.08f)),
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 0f, 0f))
        );
        _screen = screen;
    }

    private void SetButtonValue(bool down)
    {
        if (down)
        {
            if (ButtonValue >= 1f) ButtonValue = 0.6f;
            else if (ButtonValue <= 0f) ButtonValue = 0.4f;
        }
        else
        {
            if (Math.Abs(ButtonValue - 0.6f) < 0.001f) ButtonValue = 0f;
            else if (Math.Abs(ButtonValue - 0.4f) < 0.001f) ButtonValue = 1f;
        }
        
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        SetButtonValue((bool)param[0]);

        if (_screen.IsTurnOn && ButtonValue <= 0f) _screen.IsTurnOn = false;
        else if (!_screen.IsTurnOn && ButtonValue >= 1f) _screen.IsTurnOn = true;
    }
}