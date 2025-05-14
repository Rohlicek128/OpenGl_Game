using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class SliderButton : ButtonHandler
{
    public SliderButton()
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            "Slider",
            new Transform(new Vector3(-4.9225183f, -0.3374207f, -1f), new Vector3(-MathF.PI/2f, -0.5290095f, -MathF.PI/2f),
                new Vector3(0.25f)),
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0.2f, 0.1f, 1f))
        );
        Type = ButtonTypes.Continuous;
    }
    
    private void SetButtonValue(float value)
    {
        ButtonValue += value / 15f;

        ButtonValue = MathF.Max(0f, MathF.Min(1f, ButtonValue));
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        var value = (float)param[0];
        SetButtonValue(value);

        EngineObject.Transform.Quaternion = Quaternion.FromAxisAngle(EngineObject.Transform.Rotation, value / 10f * MathF.PI * 2f) * EngineObject.Transform.Quaternion;
        //Console.WriteLine(ButtonValue);
    }
}