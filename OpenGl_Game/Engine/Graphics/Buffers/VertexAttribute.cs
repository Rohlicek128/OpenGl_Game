namespace OpenGl_Game.Engine.Graphics.Buffers;

public struct VertexAttribute
{
    public VertexAttributeType Type;
    public int Size;

    public VertexAttribute(VertexAttributeType type, int size)
    {
        Type = type;
        Size = size;
    }
}