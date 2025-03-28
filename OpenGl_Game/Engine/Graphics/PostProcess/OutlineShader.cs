using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Engine.Graphics.PostProcess;

public class OutlineShader : PostProcessShader
{
    private ShaderProgram _silhouetteProgram;
    private ShaderProgram _outlineProgram;
    private Framebuffer _framebuffer;
    private Renderbuffer _renderbuffer;

    public ShaderProgram SilhouetteProgram
    {
        get => _silhouetteProgram;
        set => _silhouetteProgram = value ?? throw new ArgumentNullException(nameof(value));
    }

    public ShaderProgram OutlineProgram
    {
        get => _outlineProgram;
        set => _outlineProgram = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Framebuffer Framebuffer
    {
        get => _framebuffer;
        set => _framebuffer = value ?? throw new ArgumentNullException(nameof(value));
    }

    public unsafe OutlineShader(ShaderProgram geometry, Vector2i viewport)
    {
        _silhouetteProgram = new ShaderProgram([
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"outlineShaders\silhouette.frag", ShaderType.FragmentShader)
        ], geometry);
        
        _outlineProgram = new ShaderProgram([
            new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
            new Shader(@"outlineShaders\outline.frag", ShaderType.FragmentShader)
        ]);
        
        _framebuffer = new Framebuffer();
        _framebuffer.AttachTexture(new Texture(0, viewport, null), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
        
        _renderbuffer = new Renderbuffer(InternalFormat.DepthComponent, viewport);
        _framebuffer.AttachRenderbuffer((FramebufferAttachment)OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthAttachment, _renderbuffer.Handle);
    }

    public void Render(Matrix4 world, Matrix4 view, int selectedId)
    {
        _framebuffer.Bind();
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        _silhouetteProgram.DrawGeometryMesh(world, view, selectedId);
        
        _framebuffer.Unbind();
    }

    public void Resize(Vector2i viewport)
    {
        _framebuffer.AttachedTextures[0].Resize(viewport);
        
        _renderbuffer.Delete();
        _renderbuffer = new Renderbuffer(InternalFormat.DepthComponent, viewport);
        _framebuffer.AttachRenderbuffer(FramebufferAttachment.DepthAttachment, _renderbuffer.Handle);
    }

    public void UseProgram()
    {
        _outlineProgram.Use();
    }

    public void UnbindProgram()
    {
        _outlineProgram.Unbind();
    }

    public void SetUniforms()
    {
        _framebuffer.AttachedTextures[0].ActiveAndBind();
    }
}