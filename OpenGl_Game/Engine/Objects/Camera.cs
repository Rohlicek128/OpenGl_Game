using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public class Camera
{
    public Transform Transform;
    public Vector3 Target;
    public Vector3 Direction;

    public Vector3 Up;
    public Vector3 Front;
    public Vector3 Right;

    public float Speed;
    public bool SpeedBoost = false;
    public float OriginalSensitivity;
    public float Sensitivity;
    public float Fov;

    public float Pitch;
    public float Yaw;
    public Vector2 Mouse;
    private Vector2 _lastMouse;

    public Camera(Vector3 startingPos, float speed, float sensitivity, float fov)
    {
        Transform = new Transform(startingPos);
        Target = Vector3.Zero;
        Direction = Vector3.Normalize(Transform.Position - Target);

        Up = Vector3.UnitY;
        Front = new Vector3(0f, 0f, -1f);
        Right = Vector3.Normalize(Vector3.Cross(Up, Direction));

        Speed = speed;
        OriginalSensitivity = sensitivity;
        Fov = fov;

        _lastMouse = new Vector2(0f, 0f);
    }

    public void UpdateSensitivityByAspect(Vector2 viewport)
    {
        Sensitivity = OriginalSensitivity / (viewport.X / viewport.Y);
    }
    
    public void UpdateSensitivityByFov()
    {
        Sensitivity = (Fov / OriginalSensitivity) / 10000f;
    }

    public Matrix4 GetViewMatrix4()
    {
        return Matrix4.LookAt(Transform.Position, Transform.Position + Front, Up);
    }

    public void UpdateCameraFront()
    {
        var deltaX = Mouse.X - _lastMouse.X;
        var deltaY = Mouse.Y - _lastMouse.Y;
        _lastMouse.X = Mouse.X;
        _lastMouse.Y = Mouse.Y;

        Yaw += deltaX * Sensitivity;
        Pitch += deltaY * Sensitivity;
        if (Pitch > 89f) Pitch = 89f;
        else if (Pitch < -89f) Pitch = -89f;

        Front.X = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(Yaw));
        Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
        Front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(Yaw));
        Front = Vector3.Normalize(Front);
        
        Right = Vector3.Normalize(Vector3.Cross(Up, Front));
    }

}