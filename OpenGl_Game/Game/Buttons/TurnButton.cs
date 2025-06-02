using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Gauges;
using OpenGl_Game.Game.Gauges.Turn;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class TurnButton : ButtonHandler
{
    public float TurnAmount { get; set; }
    private TurnPage _turnPage;
    private TimerManager _timerManager;
    
    public TurnButton(float turnAmount, TurnPage turnPage, Transform transform, TimerManager timerManager, string name = "Turn Button")
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            name,
            transform,
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 0f, 0f))
        );
        Type = ButtonTypes.Press;

        TurnAmount = turnAmount;
        _turnPage = turnPage;
        _timerManager = timerManager;
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        //SetButtonValue((bool)param[0]);

        if (_timerManager.CheckTimer("turn " + TurnAmount, (float)param[1], (bool)param[0]))
        {
            _turnPage.TurnDegrees = MathF.Min(TurnGauge.MaxTurn, MathF.Max(-TurnGauge.MaxTurn, _turnPage.TurnDegrees + TurnAmount));
        }
    }
}