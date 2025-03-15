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
    
    public Station(Camera camera, List<EngineObject> children)
    {
        Camera = camera;
        ChildObjects = children;
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(3f)),
            MeshConstructor.LoadObjFromFileAssimp(@"station\station_v05.obj"),
            new Material(new Vector3(1f))
        );
        StationObject.Material.Shininess = 1f;
        StationObject.Transform.Quaternion = Quaternion.FromEulerAngles(MathF.PI * Vector3.UnitY) * StationObject.Transform.Quaternion;
        StationObject.Transform.Position = -StationObject.MeshData.BoundingBox.Origin;
        StationObject.Transform.Position.X -= 6f;

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