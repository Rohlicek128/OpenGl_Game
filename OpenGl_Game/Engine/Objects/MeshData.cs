using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Objects;

public class MeshData
{
    public float[] Vertices;
    public uint[] Indices;
    public PrimitiveType PrimitiveType;

    public MeshData(float[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        Vertices = vertices;
        Indices = indices;
        PrimitiveType = primitiveType;
    }
}