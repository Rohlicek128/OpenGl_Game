using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Gauges.Battery;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons.LaserParams;

public class AllocationMaxButton : ButtonHandler
{
    public float Amount { get; set; }
    private TimerManager _timerManager;
    
    public AllocationMaxButton(float amount, Transform transform, TimerManager timerManager, string name = "Allocation Button")
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

        if (_timerManager.CheckTimer("allocation " + Amount, (float)param[1]!, (bool)param[0]!) && Station.AllocationPercentage <= 1f + Amount / Station.AllocatedMax)
        {
            Station.AllocationPercentage *= Station.AllocatedMax / MathF.Min(Station.BatteryMax, MathF.Max(6f, Station.AllocatedMax + Amount));
            
            Station.AllocatedMax = MathF.Min(Station.BatteryMax, MathF.Max(6f, Station.AllocatedMax + Amount));
        }
    }
}