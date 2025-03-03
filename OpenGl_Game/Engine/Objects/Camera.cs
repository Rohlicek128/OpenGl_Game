using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Engine.Objects;

public class Camera
{
    public Transform Transform;
    public Vector3 Target;
    public Vector3 Direction;

    public Quaternion Quaternion;

    public Vector3 Up;
    public Vector3 Front;
    public Vector3 Right;

    public float Speed;
    public bool SpeedBoost = false;
    public float BaseSpeed = 2f;
    public float BoostSpeed = 5f;
    
    public float OriginalSensitivity;
    public float Sensitivity;
    public float Fov;
    public float NearPlane, FarPlane;

    public float Pitch;
    public float Yaw;
    public Vector2 Mouse;
    private Vector2 _lastMouse;

    public Camera(Vector3 startingPos, float speed, float sensitivity, float fov)
    {
        Transform = new Transform(startingPos);
        Target = Vector3.Zero;
        Direction = Vector3.Normalize(Transform.Position - Target);
        
        Quaternion = Quaternion.Identity;

        Up = Vector3.UnitY;
        Front = new Vector3(0f, 0f, -1f);
        Right = Vector3.Normalize(Vector3.Cross(Up, Direction));

        Speed = speed;
        OriginalSensitivity = sensitivity;
        Fov = fov;
        NearPlane = 0.025f;
        FarPlane = 1000000f;

        _lastMouse = new Vector2(0f, 0f);
        UpdateSensitivityByFov();
    }

    public void UpdateSensitivityByAspect(Vector2i viewport)
    {
        Sensitivity = OriginalSensitivity / ((float)viewport.X / viewport.Y);
    }
    
    public void UpdateSensitivityByFov()
    {
        Sensitivity = (Fov / OriginalSensitivity) / 10000f;
    }

    public Matrix4 GetViewMatrix4()
    {
        return Matrix4.LookAt(Transform.Position, Transform.Position + Front, Up);
        //return Matrix4.LookAt(Transform.Position, Transform.Position + Quaternion.ToEulerAngles(), Up);
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

        //Quaternion = Quaternion.FromEulerAngles(Yaw, Pitch, 0f) * Quaternion;
        
        Right = Vector3.Normalize(Vector3.Cross(Up, Front));
    }

    public void Move(KeyboardState keyboard, float deltaTime)
    {
        var speedTime = Speed * deltaTime * (SpeedBoost ? BoostSpeed : BaseSpeed);
        
        if (keyboard.IsKeyDown(Keys.W)) Transform.Position += Vector3.Normalize(Vector3.Cross(Right, Up)) * speedTime;
        if (keyboard.IsKeyDown(Keys.S)) Transform.Position -= Vector3.Normalize(Vector3.Cross(Right, Up)) * speedTime;
        if (keyboard.IsKeyDown(Keys.A)) Transform.Position -= Vector3.Normalize(Vector3.Cross(Front, Up)) * speedTime;
        if (keyboard.IsKeyDown(Keys.D)) Transform.Position += Vector3.Normalize(Vector3.Cross(Front, Up)) * speedTime;
        if (keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.E)) Transform.Position += Up * speedTime;
        if (keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.Q)) Transform.Position -= Up * speedTime;
    }

}