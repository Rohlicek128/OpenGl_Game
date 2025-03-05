using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Station
{
    public EngineObject StationObject;

    public Camera Camera;
    public List<EngineObject> ChildObjects;
    
    public Station(float earthRadius, Camera camera, List<EngineObject> children)
    {
        Camera = camera;
        ChildObjects = children;
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(0f)),
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f, 0.33f, 0.05f))
        );
        StationObject.IsShadowVisible = false;
        //StationObject.IsInverted = true;
        //StationObject.Transform.Position = new Vector3(earthRadius * 1.0639f, 0f, 0f);

        Camera.Transform.Position = StationObject.Transform.Position;
        foreach (var childObject in ChildObjects) childObject.Transform.Position = StationObject.Transform.Position;
    }

    public void MoveAltitude(KeyboardState keyboard, float deltaTime)
    {
        const float boost = 5f;
        //if (keyboard.IsKeyDown(Keys.Space)) StationObject.Transform.Position.X += boost * deltaTime;
        //if (keyboard.IsKeyDown(Keys.LeftControl)) StationObject.Transform.Position.X -= boost * deltaTime;
    }
}