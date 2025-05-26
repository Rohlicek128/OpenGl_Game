using OpenGl_Game.Engine.Graphics.Buffers;
using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Buffers;

public class VertexArrayBuffer
{
    public readonly int Handle;

    public VertexArrayBuffer(VertexBuffer vertexBuffer)
    {
        Handle = GL.GenVertexArray();
        
        Bind();
        vertexBuffer.Bind();
        
        var offset = 0;
        for (uint i = 0; i < vertexBuffer.Attributes.Length; i++)
        {
            var attribSize = vertexBuffer.Attributes[i].Size;
            
            GL.VertexAttribPointer(i, attribSize, VertexAttribPointerType.Float, false, vertexBuffer.Stride * sizeof(float), offset);
            GL.EnableVertexAttribArray(i);
            
            offset += attribSize * sizeof(float);
        }
        vertexBuffer.Unbind();
        Unbind();
    }

    public void Bind()
    {
        GL.BindVertexArray(Handle);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void Delete()
    {
        Unbind();
        GL.DeleteBuffer(Handle);
    }
}