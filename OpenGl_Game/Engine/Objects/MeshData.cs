using OpenGl_Game.Engine.Objects.Collisions;
using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Objects;

public struct MeshData
{
    public float[] Vertices;
    public uint[] Indices;
    public PrimitiveType PrimitiveType;

    public BoundingBox BoundingBox;

    public MeshData(float[] vertices, uint[] indices, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        Vertices = vertices;
        Indices = indices;
        PrimitiveType = primitiveType;
    }

    public MeshData(float[] vertices, uint[] indices, BoundingBox boundingBox, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        Vertices = vertices;
        Indices = indices;
        PrimitiveType = primitiveType;
        BoundingBox = boundingBox;
    }
}