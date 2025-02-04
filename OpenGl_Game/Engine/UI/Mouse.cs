using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.UI;

public class Mouse
{
    public Vector2i ScreenPosition;
    public bool IsDown;

    public Mouse()
    {
        ScreenPosition = Vector2i.Zero;
    }
}