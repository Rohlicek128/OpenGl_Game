using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class PrimeButton : ButtonHandler
{
    public static bool IsPrimed { get; set; }
    
    public PrimeButton(Transform transform, string name = "Prime Button")
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            name,
            transform,
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 0f, 0f))
        );
        Type = ButtonTypes.Press;
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
        SetButtonValue((bool)param[0]!);
        if (Station.AllocationPercentage <= 0f) ButtonValue = 0f;

        IsPrimed = ButtonValue >= 1f;
        EngineObject.Material.Color = IsPrimed ? new Vector4(1f, 0.902f, 0.118f, 1f) : new Vector4(0f);
    }
}