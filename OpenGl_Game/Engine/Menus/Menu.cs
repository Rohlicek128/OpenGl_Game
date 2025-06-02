using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Menus;

public abstract class Menu
{
    private List<MenuPage> _pages;
    public List<MenuPage> Pages
    {
        get => _pages;
        set => _pages = value ?? throw new ArgumentNullException(nameof(value));
    }

    private int _pageIndex;
    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value;
    }
    
    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set => _isVisible = value;
    }

    protected Menu()
    {
        _pages = [];
        _pageIndex = 0;
        _isVisible = false;
    }

    public void RenderMenu(Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        if (!_isVisible) return;
        GL.Viewport(0, 0, viewport.X, viewport.Y);
        
        if (_pageIndex < 0 || _pageIndex >= _pages.Count)
        {
            fonts["Pixel"].DrawText("PAGE NOT FOUND", new Vector2(100f, viewport.Y / 2f - 30f), 0.25f, new Vector4(1f), viewport);
        }
        else _pages[_pageIndex].RenderPage(mouse, viewport, fonts, deltaTime);
    } 
}