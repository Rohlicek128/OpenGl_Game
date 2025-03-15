using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects.Collisions;

public struct RayInfo
{
    public bool HasHit;
    public Vector3 HitPos;
    public float Distance;

    public RayInfo(bool hasHit)
    {
        HasHit = hasHit;
    }

    public RayInfo(bool hasHit, Vector3 hitPos, float distance)
    {
        HasHit = hasHit;
        HitPos = hitPos;
        Distance = distance;
    }
}