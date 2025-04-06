using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.PostProcess;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class OutlineShader : ShaderProgram
{
    private ShaderProgram _silhouetteProgram;
    private Framebuffer _framebuffer;
    private Renderbuffer _renderbuffer;

    public ShaderProgram SilhouetteProgram
    {
        get => _silhouetteProgram;
        set => _silhouetteProgram = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Framebuffer Framebuffer
    {
        get => _framebuffer;
        set => _framebuffer = value ?? throw new ArgumentNullException(nameof(value));
    }

    public unsafe OutlineShader(GeometryShader geometryShader, Vector2i viewport) : base(
        [
            new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
            new Shader(@"outlineShaders\outline.frag", ShaderType.FragmentShader)
        ]
    )
    {
        _silhouetteProgram = new SilhouetteShader(geometryShader);
        
        _framebuffer = new Framebuffer();
        _framebuffer.AttachTexture(new Texture(0, viewport, null), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
        
        _renderbuffer = new Renderbuffer(InternalFormat.DepthComponent, viewport);
        _framebuffer.AttachRenderbuffer((FramebufferAttachment)OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthAttachment, _renderbuffer.Handle);
    }

    public override void SetUniforms(params object[] param)
    {
        _framebuffer.AttachedTextures[0].ActiveAndBind();
        SetUniform("silhouetteTexture", 0);
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        SetUniforms();
        
        UnbindAll();
    }

    public override void DeleteAll()
    {
        Delete();
        _silhouetteProgram.DeleteAll();
        _framebuffer.Delete();
        _renderbuffer.Delete();
    }

    public void RenderSilhouette(Matrix4 world, Matrix4 view, int selectedId)
    {
        _framebuffer.Bind();
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        _silhouetteProgram.Draw(world, view, selectedId);
        
        _framebuffer.Unbind();
    }

    public void Resize(Vector2i viewport)
    {
        _framebuffer.AttachedTextures[0].Resize(viewport);
        
        _renderbuffer.Delete();
        _renderbuffer = new Renderbuffer(InternalFormat.DepthComponent, viewport);
        _framebuffer.AttachRenderbuffer(FramebufferAttachment.DepthAttachment, _renderbuffer.Handle);
    }
}