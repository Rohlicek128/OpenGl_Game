using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Graphics.Buffers;

public struct VerticesData
{
    public float[] Data;
    public PrimitiveType Type;

    public VerticesData(float[] data, PrimitiveType type)
    {
        Data = data;
        Type = type;
    }
}