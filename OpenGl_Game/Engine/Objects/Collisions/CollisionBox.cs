using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects.Collisions;

public class CollisionBox : CollisionShape
{
    public CollisionBox(Transform transform) : base(transform)
    {
    }

    public override bool CheckCollision(Vector3 point)
    {
        for (int i = 0; i < 3; i++)
        {
            if (point[i] <= Transform.Position[i] - Transform.Scale[i] / 1.5f ||
                point[i] >= Transform.Position[i] + Transform.Scale[i] / 1.5f)
            {
                return false;
            }
        }

        return true;
    }
}