using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;

namespace OpenGl_Game.Game.Buttons;

public abstract class ButtonHandler
{
    private static TimerManager _timerManager;
    public static TimerManager TimerManager
    {
        get
        {
            if (_timerManager == null) _timerManager = new TimerManager(150);
            return _timerManager;
        }
    }
    
    private EngineObject _engineObject;
    public EngineObject EngineObject
    {
        get => _engineObject;
        set => _engineObject = value ?? throw new ArgumentNullException(nameof(value));
    }

    private float _buttonValue;
    public float ButtonValue
    {
        get => _buttonValue;
        set => _buttonValue = value;
    }

    private ButtonTypes _type;
    public ButtonTypes Type
    {
        get => _type;
        set => _type = value;
    }

    public delegate void EventHandler(object sender, params object?[] param);
    public event EventHandler OnEvent;

    public void AddEvent(EventHandler e)
    {
        if (OnEvent != null) return;
        OnEvent += e;
    }

    private protected virtual void MyEvent(object sender, params object?[] param)
    {
        
    }

    public void Activate(params object?[] param)
    {
        OnEvent(this, param);
    }
}