using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Station
{
    public EngineObject StationObject;

    public Camera Camera;

    public List<EngineObject> StationObjects;
    
    public Station(Camera camera)
    {
        Camera = camera;
        StationObjects = [];
        StationObject = new EngineObject(
            "Station",
            new Transform(new Vector3(3f)),
            MeshConstructor.LoadObjFromFileAssimp(@"station\station_v05.obj", true),
            new Material(new Vector3(1f))
        );
        StationObject.Material.Shininess = 1f;
        StationObject.Transform.Quaternion = Quaternion.FromEulerAngles(MathF.PI * Vector3.UnitY) * StationObject.Transform.Quaternion;
        StationObject.Transform.Position = -StationObject.MeshData.BoundingBox.Origin;
        StationObject.Transform.Position.X -= 6f;

        Camera.Transform.Position = StationObject.Transform.Position;
        
        StationObjects.Add(new EngineObject(
            "Laser Button", 
            new Transform(new Vector3(-5.079833f, 1.1671673f, 0.0257724f), new Vector3(0f, 0f, -0.8928769f), new Vector3(0.35f)), 
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f, 1f, 0f))
        ));
    }
    
}