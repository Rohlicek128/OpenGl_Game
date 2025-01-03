namespace OpenGl_Game.Engine.Graphics.Buffers;

public struct Attribute
{
    public AttribType Type;
    public int Size;

    public Attribute(AttribType type, int size)
    {
        Type = type;
        Size = size;
    }
}