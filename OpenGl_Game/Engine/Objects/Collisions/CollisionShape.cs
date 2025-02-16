using OpenGl_Game.Engine.Graphics.Buffers;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects.Collisions;

public abstract class CollisionShape
{
    public Transform Transform;

    protected CollisionShape(Transform transform)
    {
        Transform = transform;
    }

    public abstract bool CheckCollision(Vector3 point);
}