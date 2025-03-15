using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects.Collisions;

public class CollisionSphere : CollisionShape
{
    private float _radius;

    public float Radius
    {
        get => _radius;
        set => _radius = value;
    }

    public CollisionSphere(Transform transform, float radius) : base(transform)
    {
        _radius = radius;
    }

    public override bool CheckCollision(Vector3 point)
    {
        throw new NotImplementedException();
    }

    public override RayInfo CheckCollision(Ray ray)
    {
        var m = ray.Origin - Transform.Position;
        var b = Vector3.Dot(m, ray.Direction);
        var c = Vector3.Dot(m, m) - _radius * _radius;

        if (c > 0f && b > 0f) return new RayInfo(false);
        var discr = b * b - c;

        if (discr < 0f) return new RayInfo(false);

        var t = -b - MathF.Sqrt(discr);
        
        var q = ray.Origin + t * ray.Direction;

        return new RayInfo(true, q, t);
    }
}