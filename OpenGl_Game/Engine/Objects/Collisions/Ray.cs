using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects.Collisions;

public struct Ray
{
    public Vector3 Origin;
    public Vector3 Direction;

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = direction;
    }
}