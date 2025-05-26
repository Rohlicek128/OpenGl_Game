using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Gauges.Speed;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class SpeedButton : ButtonHandler
{
    public float SpeedAmount { get; set; }
    private SpeedPage _speedPage;
    
    public SpeedButton(float speedAmount, SpeedPage speedPage, Transform transform, string name = "Speed Button")
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            name,
            transform,
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 0f, 0f))
        );
        Type = ButtonTypes.Hold;

        SpeedAmount = speedAmount;
        _speedPage = speedPage;
    }
    
    private void SetButtonValue(bool down)
    {
        ButtonValue = down ? 1f : 0f;
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        SetButtonValue((bool)param[0]);

        _speedPage.TargetSpeed += SpeedAmount * (float)param[1]! * ButtonValue * 10f;
    }
}