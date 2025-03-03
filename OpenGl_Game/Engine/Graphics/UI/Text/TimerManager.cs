namespace OpenGl_Game.Engine.Graphics.UI.Text;

public class TimerManager
{
    public Dictionary<string, ButtonTimer> Timers;
    public int PressTimeMsMs;

    public TimerManager(int pressTimeMs)
    {
        Timers = new Dictionary<string, ButtonTimer>();
        PressTimeMsMs = pressTimeMs;
    }

    public bool Add(string key)
    {
        return Timers.TryAdd(key, new ButtonTimer(PressTimeMsMs));
    }

    public bool CheckTimer(string key, float delta, bool isPressed)
    {
        if (!Timers.TryGetValue(key, out var timer)) return isPressed && Add(key);
        
        if (!timer.Check(delta)) return false;
        Timers.Remove(key);
        return isPressed && Add(key);
    }
}