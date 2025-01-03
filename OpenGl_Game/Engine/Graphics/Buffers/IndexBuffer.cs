using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Buffers;

public class IndexBuffer
{
    public readonly int Handle;
    
    public readonly uint[] Data;

    public int TriangleCount;

    public IndexBuffer(uint[] data, bool isStatic = true)
    {
        Data = data;
        var hint = isStatic ? BufferUsage.StaticDraw : BufferUsage.StreamDraw;
        
        Handle = GL.GenBuffer();
        Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, hint);
        Unbind();

        TriangleCount = data.Length / 3;
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
            
            for (var j = 0; j < bufferData[i].Length; j++) bufferData[i][j] += offset;
            data = data.Concat(bufferData[i]).ToArray();
        }

        return data;
    }
}