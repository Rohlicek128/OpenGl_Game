using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class LaserButton : ButtonHandler
{
    public LaserButton()
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            "Laser Button",
            new Transform(new Vector3(-5.079833f, 1.1671673f, 0.0257724f), new Vector3(0f, 0f, -0.8928769f),
                new Vector3(0.35f)),
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f, 1f, 0f))
        );
    }

    private void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;

        ((ShaderProgram)param[1]).Objects[0].IsVisible = (bool)param[0];
    }
}