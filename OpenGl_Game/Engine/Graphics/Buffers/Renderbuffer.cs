using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Buffers;

public class Renderbuffer
{
    public readonly int Handle;

    public Renderbuffer(InternalFormat format, Vector2i viewport)
    {
        Handle = GL.GenRenderbuffer();
        Bind();
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, format, viewport.X, viewport.Y);
        Unbind();
    }

    public void Bind()
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Handle);
    }
    
    public void Unbind()
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    public void Delete()
    {
        GL.DeleteRenderbuffer(Handle);
    }
}