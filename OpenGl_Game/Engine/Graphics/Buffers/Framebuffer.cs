using OpenGl_Game.Engine.Graphics.Textures;
using OpenTK.Graphics.OpenGL.Compatibility;
using FramebufferTarget = OpenTK.Graphics.OpenGL.Compatibility.FramebufferTarget;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using GL = OpenTK.Graphics.OpenGL.Compatibility.GL;
using RenderbufferTarget = OpenTK.Graphics.OpenGL.Compatibility.RenderbufferTarget;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Engine.Graphics.Buffers;

public class Framebuffer
{
    public readonly int Handle;
    public List<Texture> AttachedTextures;

    public Framebuffer()
    {
        AttachedTextures = [];
        Handle = GL.GenFramebuffer();
    }

    public void AttachTexture(Texture texture, FramebufferAttachment type, TextureTarget target)
    {
        Bind();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, type, target, texture.Handle, 0);
        Unbind();
        AttachedTextures.Add(texture);
    }
    
    public void AttachTexture(int textureHandle, FramebufferAttachment type, TextureTarget target)
    {
        Bind();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, type, target, textureHandle, 0);
        Unbind();
    }
    
    public void AttachRenderbuffer(FramebufferAttachment type, int rbHandle)
    {
        Bind();
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, type, RenderbufferTarget.Renderbuffer, rbHandle);
        Unbind();
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
    }

    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteFramebuffer(Handle);
    }
    
}