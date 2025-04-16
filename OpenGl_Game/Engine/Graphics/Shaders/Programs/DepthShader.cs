using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class DepthShader : ShaderProgram
{
    public Texture DepthTexture { get; set; }
    private Framebuffer _framebuffer;
    
    public unsafe DepthShader(Vector2i viewport) : base([
        new Shader(@"depthShaders\depth.vert", ShaderType.VertexShader),
        new Shader(@"depthShaders\depth.frag", ShaderType.FragmentShader)
    ], [MeshConstructor.CreateScreenQuad()], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)])
    {
        DepthTexture = new Texture(0, viewport, null, InternalFormat.DepthComponent24);
        
        _framebuffer = new Framebuffer();
        _framebuffer.AttachTexture(DepthTexture, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2d);
    }

    public override void Draw(params object[] param)
    {
        var viewport = (Vector2i)param[0];
        DepthTexture.Bind();
        GL.CopyTexSubImage2D(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, 0, 0, 0, 0, 0, viewport.X, viewport.Y);
        DepthTexture.Unbind();
        
        //GL.Disable(EnableCap.DepthTest);
        //_framebuffer.Bind();
        //BindAll();
        //
        //DrawEachObject((Matrix4)param[0]);
        //
        //UnbindAll();
        //_framebuffer.Unbind();
        //GL.Enable(EnableCap.DepthTest);
    }

    public override void DeleteAll()
    {
        Delete();
    }
}