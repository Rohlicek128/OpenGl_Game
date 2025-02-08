namespace OpenGl_Game.Engine.Graphics.Buffers;

public struct VertexAttribute
{
    public VertexAttribType Type;
    public int Size;

    public VertexAttribute(VertexAttribType type, int size)
    {
        Type = type;
        Size = size;
    }
}