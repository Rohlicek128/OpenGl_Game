using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using TextureMagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;

namespace OpenGl_Game.Game.Screens;

public abstract class ScreenHandler
{
    public static Vector3 LcdBlack = new(0.02f);
    
    private List<ScreenPage> _pages;
    public List<ScreenPage> Pages
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

    private EngineObject _engineObject;
    public EngineObject EngineObject
    {
        get => _engineObject;
        set => _engineObject = value ?? throw new ArgumentNullException(nameof(value));
    }

    private Framebuffer _framebuffer;
    public Framebuffer Framebuffer
    {
        get => _framebuffer;
        set => _framebuffer = value ?? throw new ArgumentNullException(nameof(value));
    }

    private Vector2i _screenResolution;
    public Vector2i ScreenResolution
    {
        get => _screenResolution;
        set => _screenResolution = value;
    }

    private bool _isTurnOn;
    public bool IsTurnOn
    {
        get => _isTurnOn;
        set => _isTurnOn = value;
    }

    protected unsafe ScreenHandler(Vector2i screenResolution)
    {
        _screenResolution = screenResolution;
        _isTurnOn = false;
        _pages = [];
        _pageIndex = 0;
        
        _framebuffer = new Framebuffer();
        _framebuffer.AttachTexture(new Texture(0, _screenResolution, null, minFilter:(TextureMinFilter)OpenTK.Graphics.OpenGL.Compatibility.TextureMinFilter.Nearest, magFilter:(TextureMagFilter)OpenTK.Graphics.OpenGL.Compatibility.TextureMagFilter.Nearest), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
    }

    /// <summary>
    /// If the screen is turned on, renders currently selected page
    /// </summary>
    /// <param name="collision"></param>
    /// <param name="mouse"></param>
    /// <param name="viewport"></param>
    /// <param name="fonts"></param>
    /// <param name="deltaTime"></param>
    public void RenderScreen(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        GL.Viewport(0, 0, ScreenResolution.X, ScreenResolution.Y);
        Framebuffer.Bind();
        
        if (IsTurnOn)
        {
            if (_pageIndex < 0 || _pageIndex >= _pages.Count)
            {
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                fonts["Pixel"].DrawText("PAGE NOT FOUND", new Vector2(100f, _screenResolution.Y / 2f - 30f), 0.25f, new Vector4(1f), _screenResolution);
            }
            else _pages[_pageIndex].RenderPage(collision, mouse, viewport, fonts, deltaTime);
        }
        else
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        
        Framebuffer.Unbind();
        GL.Viewport(0, 0, viewport.X, viewport.Y);
    }
}