using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using TextureMagFilter = OpenTK.Graphics.OpenGL.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL.TextureMinFilter;

namespace OpenGl_Game.Game.Screens;

public abstract class ScreenHandler
{
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
        _isTurnOn = true;
        
        _framebuffer = new Framebuffer();
        _framebuffer.AttachTexture(new Texture(0, _screenResolution, null, minFilter:(TextureMinFilter)OpenTK.Graphics.OpenGL.Compatibility.TextureMinFilter.Nearest, magFilter:(TextureMagFilter)OpenTK.Graphics.OpenGL.Compatibility.TextureMagFilter.Nearest), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
    }

    public virtual void RenderScreen(CollisionShader collision, ShaderProgram program, Matrix4 world, Matrix4 view, Vector2i viewport, FontMap font, float boost)
    {
        
    } 
}