using OpenGl_Game.Engine.Graphics.Textures;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using InternalFormat = OpenTK.Graphics.OpenGL.InternalFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;

namespace OpenGl_Game.Engine.Graphics.Buffers;

public class GBuffer : Framebuffer
{
    public Texture PositionTexture;
    public Texture NormalsTexture;
    public Texture ColorSpecTexture;
    public Renderbuffer Renderbuffer;
    public unsafe GBuffer(Vector2i viewport)
    {
        //Position
        PositionTexture = new Texture(0, viewport, null, InternalFormat.Rgba16f, PixelType.Float);
        AttachTexture(PositionTexture, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
        //Normals
        NormalsTexture = new Texture(1, viewport, null, InternalFormat.Rgba16f, PixelType.Float);
        AttachTexture(NormalsTexture, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2d);
        //Color + Spec
        ColorSpecTexture = new Texture(2, viewport, null, InternalFormat.Rgba);
        AttachTexture(ColorSpecTexture, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2d);

        Bind();
        GL.DrawBuffers(3, [DrawBufferMode.ColorAttachment0, DrawBufferMode.ColorAttachment1, DrawBufferMode.ColorAttachment2]);
        Unbind();
        
        Renderbuffer = new Renderbuffer(InternalFormat.DepthComponent, viewport);
        AttachRenderbuffer(FramebufferAttachment.DepthAttachment, Renderbuffer.Handle);
    }

    public void BindBufferTextures()
    {
        PositionTexture.ActiveAndBind();
        NormalsTexture.ActiveAndBind();
        ColorSpecTexture.ActiveAndBind();
    }
    
    public void Resize(Vector2i viewport)
    {
        PositionTexture.Resize(viewport);
        NormalsTexture.Resize(viewport);
        ColorSpecTexture.Resize(viewport);
        
        Renderbuffer.Delete();
        Renderbuffer = new Renderbuffer(InternalFormat.DepthComponent, viewport);
        AttachRenderbuffer(FramebufferAttachment.DepthAttachment, Renderbuffer.Handle);
    }
}