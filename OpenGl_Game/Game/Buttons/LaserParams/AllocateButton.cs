using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons.LaserParams;

public class AllocateButton : ButtonHandler
{
    public static bool IsAllocating { get; set; }

    public AllocateButton(Transform transform, string name = "Allocate Button")
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
        if (param[0] == null || PrimeButton.IsPrimed || Station.AllocationPercentage >= 1f) return;
        SetButtonValue((bool)param[0]!);

        EngineObject.Material.Color = IsAllocating ? new Vector4(1f, 0f, 0f, 1f) : new Vector4(0f);
        EngineObject.Name = IsAllocating && Station.AllocationPercentage < 1f ? "Stop Allocating" : "Start Allocating";
        IsAllocating = ButtonValue >= 1f;
    }
}