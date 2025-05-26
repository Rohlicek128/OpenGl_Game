using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public struct Transform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Quaternion Quaternion;
    public Vector3 Scale;
    public Vector3 Origin;

    public Transform(Vector3 position)
    {
        Position = position;
        Rotation = new Vector3(0f);
        Scale = new Vector3(1f);
        Quaternion = Quaternion.Identity;
        Origin = Vector3.Zero;
    }
    
    public Transform(Vector3 position, Vector3 rotation)
    {
        Position = position;
        Rotation = rotation;
        Scale = new Vector3(1f);;
        Quaternion = Quaternion.FromEulerAngles(rotation);
        Origin = Vector3.Zero;
    }

    public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Quaternion = Quaternion.FromEulerAngles(rotation);
        Origin = Vector3.Zero;
    }
    
    public Transform(Vector3 position, Vector3 rotation, Vector3 scale, Vector3 origin)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
        Quaternion = Quaternion.FromEulerAngles(rotation);
        Origin = origin;
    }

    public void SetRotationByPosition(Vector3 origin)
    {
        Rotation = -Position / Position.Length;

        //Rotation.Z = MathF.Atan((-Position.Y - origin.Y) / (Position.X - origin.X));
        //Rotation.Y = MathF.Atan((-Position.Z - origin.Z) / (Position.X - origin.X));
        //Rotation.X = MathF.Atan((Position.Z - origin.Z) / (Position.Z - origin.Z));
    }

    public static Quaternion QuaternionLookAt(Vector3 source, Vector3 dest)
    {
        var forword = Vector3.Normalize(dest - source);

        var dot = Vector3.Dot(Vector3.UnitZ, forword);

        if (MathF.Abs(dot + 1f) < 0.000001f)
        {
            return new Quaternion(Vector3.UnitY, MathF.PI);
        }

        if (MathF.Abs(dot - 1f) < 0.000001f)
        {
            return Quaternion.Identity;
        }

        var rotAngle = MathF.Acos(dot);
        var rotAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, forword));
        return Quaternion.FromAxisAngle(rotAxis, rotAngle);
    }
}