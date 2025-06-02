using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons.LaserParams;

public class RadiusButton : ButtonHandler
{
    public float Amount { get; set; }
    private TimerManager _timerManager;
    
    public RadiusButton(float amount, Transform transform, TimerManager timerManager, string name = "Radius Button")
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            name,
            transform,
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 0f, 0f))
        );
        Type = ButtonTypes.Press;

        Amount = amount;
        _timerManager = timerManager;
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        //SetButtonValue((bool)param[0]);

        if (!PrimeButton.IsPrimed && _timerManager.CheckTimer("radius " + Amount, (float)param[1]!, (bool)param[0]!))
        {
            Station.LaserRadius = MathF.Min(Station.MaxLaserRadius, MathF.Max(0.5f, Station.LaserRadius + Amount));
        }
    }
}