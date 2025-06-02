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
            new Transform(new Vector3(-1.2517788f, 0.76516974f, 0.3373942f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-40f), -MathF.PI),
                new Vector3(0.04f, 0.02f, 0.02f), new Vector3(-0.04f, 0f, 0f)),
            MeshConstructor.LoadObjFromFileAssimp(@"Station\laserCylinder.obj"),
            new Material(new Vector3(0.2f, 0.1f, 1f))
        );
        Type = ButtonTypes.Continuous;
    }
    
    private void SetButtonValue(float value)
    {
        ButtonValue += value / 10f;

        ButtonValue = MathF.Round(MathF.Max(0f, MathF.Min(1f, ButtonValue)) * 10f) / 10f;
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        var value = (float)param[0];
        SetButtonValue(value);
        
        EngineObject.Transform.Quaternion = Quaternion.FromEulerAngles(
            new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-40f - (ButtonValue * 2f - 1f) * 50f), -MathF.PI));
        Console.WriteLine(ButtonValue);
    }
}