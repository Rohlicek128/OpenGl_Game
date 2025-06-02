using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Menus;

public class MenuPage
{
    private UiGraphics _uiGraphics;
    public UiGraphics UiGraphics
    {
        get => _uiGraphics;
        set => _uiGraphics = value ?? throw new ArgumentNullException(nameof(value));
    }

    protected MenuPage()
    {
        _uiGraphics = new UiGraphics();
    }

    public virtual void RenderPage(Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        
    }
}