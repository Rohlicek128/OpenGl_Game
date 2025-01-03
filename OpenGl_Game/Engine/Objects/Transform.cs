using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public struct Transform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public Transform(Vector3 position)
    {
        Position = position;
        Rotation = new Vector3(0f);
        Scale = new Vector3(1f);
    }
    
    public Transform(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = rotation;
        Scale = new Vector3(1f);;
    }

    public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}