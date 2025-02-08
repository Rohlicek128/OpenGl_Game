using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;
using VertexAttribType = OpenGl_Game.Engine.Graphics.Buffers.VertexAttribType;

namespace OpenGl_Game.Engine.Graphics.PostProcess;

public class PostProcess
{
    public ShaderProgram Program;
    public Framebuffer Framebuffer;
    public Renderbuffer Renderbuffer;

    public int Banding;
    public float Grayscale;

    public unsafe PostProcess(Shader[] shaders, Vector2i viewport)
    {
        Banding = 10;
        Grayscale = 0.5f;
        var screenQuad = EngineObject.CreateEmpty();
        screenQuad.VerticesData.Data =
        [
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f,  0.0f, 1.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 1.0f
        ];
        
        Program = new ShaderProgram(shaders, [screenQuad], [new VertexAttribute(VertexAttribType.PosAndTex, 4)]);
        
        Framebuffer = new Framebuffer();
        Framebuffer.AttachTexture(new Texture(0, viewport, null), (FramebufferAttachment)OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, (TextureTarget)OpenTK.Graphics.OpenGL.TextureTarget.Texture2d);
        
        Renderbuffer = new Renderbuffer(InternalFormat.Depth24Stencil8, viewport);
        Framebuffer.AttachRenderbuffer((FramebufferAttachment)OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, Renderbuffer.Handle);
    }

    public void UseProgram()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        Program.Use();
        Program.ArrayBuffer.Bind();
        GL.Disable(EnableCap.DepthTest);
        
        GL.ActiveTexture(TextureUnit.Texture0);
        Framebuffer.AttachedTextures[0].Bind();
    }

    public virtual void DrawPostProcess()
    {
        UseProgram();
        Program.SetUniform("banding", Banding);
        Program.SetUniform("grayscale", Grayscale);
        Draw();
    }

    public void Draw()
    {
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        Program.ArrayBuffer.Unbind();
        GL.Enable(EnableCap.DepthTest);
    }

    public unsafe void Resize(Vector2i viewport)
    {
        Framebuffer.AttachedTextures[0].Bind();
        GL.TexImage2D((OpenTK.Graphics.OpenGL.TextureTarget)TextureTarget.Texture2d, 0, InternalFormat.Rgba, 
            viewport.X, viewport.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        Framebuffer.AttachedTextures[0].Unbind();
        
        Renderbuffer.Delete();
        Renderbuffer = new Renderbuffer(InternalFormat.Depth24Stencil8, viewport);
        Framebuffer.AttachRenderbuffer(FramebufferAttachment.DepthStencilAttachment, Renderbuffer.Handle);
    }

    public void Bind()
    {
        Framebuffer.Bind();
    }

    public void Unbind()
    {
        Framebuffer.Unbind();
    }

    public void Delete()
    {
        Program.Delete();
        Framebuffer.Delete();
        Renderbuffer.Delete();
    }
}