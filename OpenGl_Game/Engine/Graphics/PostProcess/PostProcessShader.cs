using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Engine.Graphics.PostProcess;

public class PostProcessShader : ShaderProgram
{
    public Framebuffer Framebuffer;
    public Renderbuffer Renderbuffer;

    public List<ShaderProgram> Shaders;

    public int Banding;
    public float Grayscale;

    public unsafe PostProcessShader(Vector2i viewport) : base(
        [
            new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
            new Shader(@"PostProcessShaders\postProcessShader.frag", ShaderType.FragmentShader)
        ], [MeshConstructor.CreateScreenQuad()], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)]
    )
    {
        Banding = -1;
        Grayscale = 0.0f;
        Shaders = [];
        
        Framebuffer = new Framebuffer();
        Framebuffer.AttachTexture(new Texture(0, viewport, null), (FramebufferAttachment)OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, (TextureTarget)OpenTK.Graphics.OpenGL.TextureTarget.Texture2d);
        
        Renderbuffer = new Renderbuffer(InternalFormat.Depth24Stencil8, viewport);
        Framebuffer.AttachRenderbuffer((FramebufferAttachment)OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, Renderbuffer.Handle);
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("banding", Banding);
        SetUniform("grayscale", Grayscale);

        for (var i = 0; i < Framebuffer.AttachedTextures.Count; i++)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + (uint)i);
            Framebuffer.AttachedTextures[i].Bind();
        }
        
        if ((int)param[0] != -1)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, (int)param[0]);
        }
    }

    public override void Draw(params object[] param)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Disable(EnableCap.DepthTest);
        
        BindAll();
        SetUniforms(param[0]);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        GL.Enable(EnableCap.DepthTest);
    }

    public void DrawShaders()
    {
        GL.Disable(EnableCap.DepthTest);
        
        IndexBuffer.Bind();
        ArrayBuffer.Bind();
        foreach (var postProcess in Shaders)
        {
            postProcess.Use();
            
            postProcess.SetUniforms();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            postProcess.Unbind();
        }
        ArrayBuffer.Unbind();
        
        UnbindAll();
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

    public override void DeleteAll()
    {
        Delete();
        Framebuffer.Delete();
        Renderbuffer.Delete();
    }
}