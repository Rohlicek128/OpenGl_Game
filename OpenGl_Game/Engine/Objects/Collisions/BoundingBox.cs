using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects.Collisions;

public class BoundingBox
{
    private Vector3 _minimum;
    private Vector3 _maximum;
    private Vector3 _origin;

    public Vector3 Minimum
    {
        get => _minimum;
        set => _minimum = value;
    }

    public Vector3 Maximum
    {
        get => _maximum;
        set => _maximum = value;
    }

    public Vector3 Origin
    {
        get => _origin;
        set => _origin = value;
    }

    public BoundingBox(Vector3 minimum, Vector3 maximum, Vector3 origin)
    {
        _minimum = minimum;
        _maximum = maximum;
        _origin = origin;
    }
}