using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Engine.Objects;

public class Mouse
{
    public Vector2i ScreenPosition;
    public bool IsDown { get; set; }
    public MouseButton DownButton { get; set; }
    public int PressLenght { get; set; }

    public Mouse()
    {
        ScreenPosition = Vector2i.Zero;
    }
}