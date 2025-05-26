using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Graphics.Buffers;

public class VertexBuffer
{
    public int Handle;
    
    public float[] Data;
    public readonly VertexAttribute[] Attributes;
    public readonly int Stride;
    public BufferUsage Hint;

    public int AddedLenght;
    public int FilledLenght;

    public VertexBuffer(float[] data, VertexAttribute[] attributes, BufferUsage hint)
    {
        Data = data;
        Attributes = attributes;
        Stride = Attributes.Sum(attrib => attrib.Size);
        Hint = hint;
        
        Handle = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, hint);
        Unbind();
    }

    public void ChangeData(float[] data, int offset)
    {
        Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, offset * sizeof(float), data.Length * sizeof(float), data);
        Unbind();
        //data.CopyTo(Data, offset);

        FilledLenght += Math.Max(0, offset - Data.Length);
    }

    public void Enlarge(int lenght, float[]? data = null)
    {
        Delete();
        Handle = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, (Data.Length + lenght) * sizeof(float), data != null ? Data.Concat(data).ToArray() : Data, Hint);
        Unbind();

        AddedLenght += lenght;
        FilledLenght += data?.Length ?? 0;
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