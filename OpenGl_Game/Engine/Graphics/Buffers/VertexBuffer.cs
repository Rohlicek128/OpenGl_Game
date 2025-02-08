using OpenGl_Game.Engine.Graphics.Buffers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Buffer = OpenTK.Graphics.OpenGL.Buffer;

namespace OpenGl_Game.Buffers;

public class VertexBuffer
{
    public readonly int Handle;
    
    public readonly float[] Data;
    public readonly VertexAttribute[] Attributes;
    public readonly int Stride;

    public VertexBuffer(float[] data, VertexAttribute[] attributes, BufferUsage hint)
    {
        Data = data;
        Attributes = attributes;
        Stride = Attributes.Sum(attrib => attrib.Size);
        
        Handle = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, hint);
        Unbind();
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Delete()
    {
        Unbind();
        GL.DeleteBuffer(Handle);
    }
    
    public static float[] CombineBufferData(float[][] bufferData)
    {
        var data = bufferData[0];
        for (var i = 1; i < bufferData.Length; i++)
        {
            data = data.Concat(bufferData[i]).ToArray();
        }

        return data;
    }

}