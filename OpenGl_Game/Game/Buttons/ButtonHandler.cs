using OpenGl_Game.Engine.Objects;

namespace OpenGl_Game.Game.Buttons;

public abstract class ButtonHandler
{
    private EngineObject _engineObject;
    public EngineObject EngineObject
    {
        get => _engineObject;
        set => _engineObject = value ?? throw new ArgumentNullException(nameof(value));
    }

    public delegate void EventHandler(object sender, params object?[] param);
    public event EventHandler OnEvent;

    public void AddEvent(EventHandler e)
    {
        if (OnEvent != null) return;
        OnEvent += e;
    }

    public void Activate(params object?[] param)
    {
        OnEvent(this, param);
    }
}