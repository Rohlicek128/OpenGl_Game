namespace OpenGl_Game.Engine.Graphics.UI.Text;

public class ButtonTimer
{
    public float MaxTimeMs;
    public float TimeMs;

    public ButtonTimer(int maxTimeMs)
    {
        MaxTimeMs = maxTimeMs / 1000f;
        TimeMs = 0f;
    }

    public bool Check(float delta)
    {
        TimeMs += delta;
        return TimeMs >= MaxTimeMs;
    }
}