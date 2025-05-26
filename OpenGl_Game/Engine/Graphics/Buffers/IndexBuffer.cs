using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Graphics.Buffers;

public class IndexBuffer
{
    public int Handle;
    
    public uint[] Data;

    public int TriangleCount;
    public BufferUsage Hint;
    
    public int AddedLenght;
    public int FilledLenght;

    public IndexBuffer(uint[] data, bool isStatic = true)
    {
        Data = data;
        Hint = isStatic ? BufferUsage.StaticDraw : BufferUsage.StreamDraw;
        
        Handle = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, Hint);
        Unbind();

        TriangleCount = data.Length / 3;
    }
    
    public void ChangeData(uint[] data, int offset, bool atEnd = false)
    {
        if (atEnd)
        {
            var max = data.Max() + 1;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == RenderEngine.PrimitiveIndex) continue;
                data[i] += max;
            }
        }
        
        Bind();
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, offset * sizeof(float), data.Length * sizeof(float), data);
        Unbind();
        //data.CopyTo(Data, offset);
        
        FilledLenght += Math.Max(0, offset - Data.Length);
    }

    public void Enlarge(int lenght, uint[]? data = null, bool atEnd = false)
    {
        if (atEnd && data != null)
        {
            var max = data.Max() + 1;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == RenderEngine.PrimitiveIndex) continue;
                data[i] += max;
            }
        }
        
        Delete();
        Handle = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, (Data.Length + lenght) * sizeof(float), data != null ? Data.Concat(data).ToArray() : Data, Hint);
        Unbind();
        
        AddedLenght += lenght;
        FilledLenght += data?.Length ?? 0;
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    public void Delete()
    {
        Unbind();
        GL.DeleteBuffer(Handle);
    }
    
    public static uint[] CombineIndexBuffers(uint[][] bufferData)
    {
        var data = bufferData[0];
        for (var i = 1; i < bufferData.Length; i++)
        {
            var offset = data.Max() + 1;

            for (var j = 0; j < bufferData[i].Length; j++)
            {
                if (bufferData[i][j] == RenderEngine.PrimitiveIndex) continue;
                bufferData[i][j] += offset;
                if (bufferData[i][j] == 0) Console.WriteLine("ZERO FOUND");
            }
            data = data.Concat(bufferData[i]).ToArray();
        }
        
        return data;
    }
}