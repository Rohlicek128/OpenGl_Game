using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Screens;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Buttons;

public class PageSetterButton : ButtonHandler
{
    public ScreenHandler Screen { get; set; }
    public int PageIndex { get; set; }

    public PageSetterButton(ScreenHandler screen, int pageIndex, Transform transform, string name = "Page Setter")
    {
        AddEvent(MyEvent);
        EngineObject = new EngineObject(
            name,
            transform,
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 0f, 0f))
        );
        Type = ButtonTypes.Press;
        Screen = screen;
        PageIndex = pageIndex;
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        //SetButtonValue((bool)param[0]);

        Screen.PageIndex = PageIndex;
    }
}