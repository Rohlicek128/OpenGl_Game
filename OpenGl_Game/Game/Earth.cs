using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.Objects.Collisions;
using OpenGl_Game.Game.Targets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Earth
{
    public EngineObject EarthObject;
    public CollisionSphere CollisionSphere;
    public const int EarthAxis = 1; 
    public const int EarthOrientation = -1;
    public const float Circumference = 40_075f;
    
    public Texture ColorMap;
    public Texture RoughnessMap;
    public Texture NormalMap;
    public Texture CitiesMap;
    
    public Texture HeightMap;
    public float Scale;
    public float MidLevel;
    
    public Earth(Transform transform, int resolution, float scale, float midLevel = 1f)
    {
        ColorMap = new Texture("Earth\\earth_color_mar.png", 0, TextureMinFilter.Linear, TextureMagFilter.Linear);
        RoughnessMap = new Texture("Earth\\earth_specular.png", 1, TextureMinFilter.Linear, TextureMagFilter.Linear);
        NormalMap = new Texture("Earth\\earth_normal_high2.png", 2, TextureMinFilter.Linear, TextureMagFilter.Linear);
        HeightMap = new Texture("Earth\\earth_height_water.png", 0, TextureMinFilter.Linear, TextureMagFilter.Linear);
        CitiesMap = new Texture("Earth\\earth_cities.png", 3, TextureMinFilter.Linear, TextureMagFilter.Linear);
        Scale = scale;
        MidLevel = midLevel;
        
        EarthObject = new EngineObject(
            "Earth",
            transform,
            GenerateFaces(resolution + 1),
            //new Material(new Vector3(1f, 0f, 1f))
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, ColorMap},
                {TextureTypes.Specular, RoughnessMap},
                {TextureTypes.Normal, NormalMap},
                {TextureTypes.Overlay, CitiesMap},
                {TextureTypes.Emissive, CitiesMap}
            })
        );
        EarthObject.IsSelectable = false;
        EarthObject.IsShadowVisible = false;
        EarthObject.VisibleForId = 1;

        CollisionSphere = new CollisionSphere(transform, transform.Scale.X);
    }

    public void MoveEarth(KeyboardState keyboard, float deltaTime, float boost, List<EngineObject> engineObjects, bool debug = false)
    {
        engineObjects.Add(EarthObject);
        var speed = deltaTime * boost;
        
        if (keyboard.IsKeyDown(Keys.W) || true) foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(-Vector3.UnitZ * speed) * o.Transform.Quaternion;

        if (debug)
        {
            if (keyboard.IsKeyDown(Keys.S)) foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitZ * speed) * o.Transform.Quaternion;
            if (keyboard.IsKeyDown(Keys.A)) foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(-Vector3.UnitX * speed) * o.Transform.Quaternion;
            if (keyboard.IsKeyDown(Keys.D)) foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitX * speed) * o.Transform.Quaternion;
        
            if (keyboard.IsKeyDown(Keys.Q)) foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(-Vector3.UnitY * deltaTime * 0.5f) * o.Transform.Quaternion;
            if (keyboard.IsKeyDown(Keys.E)) foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitY * deltaTime * 0.5f) * o.Transform.Quaternion;
        }
        
        if (keyboard.IsKeyDown(Keys.Space)) EarthObject.Transform.Position[EarthAxis] -= deltaTime * 50f;
        if (keyboard.IsKeyDown(Keys.LeftControl)) EarthObject.Transform.Position[EarthAxis] += deltaTime * 50f;
        
        CollisionSphere.Transform.Position = EarthObject.Transform.Position;
    }

    public void RotateEarth(float deltaTime, float amount, List<EngineObject> engineObjects)
    {
        if (amount == 0f) return;
        engineObjects.Add(EarthObject);
        
        foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitY * deltaTime * amount) * o.Transform.Quaternion;
    }

    public MeshData GenerateFaces(int resolution)
    {
        Vector3[] faceNormals =
        [
            Vector3.UnitY,
            -Vector3.UnitY,
            -Vector3.UnitX,
            Vector3.UnitX,
            -Vector3.UnitZ,
            Vector3.UnitZ
        ];
        var faces = new MeshData[faceNormals.Length];
        for (int i = 0; i < faceNormals.Length; i++)
        {
            faces[i] = MeshConstructor.CreateFace(faceNormals[i], resolution, true);
        }

        var meshData = MeshConstructor.CombineMeshData(faces);

        //var sb = new StringBuilder();
        //Tex Coords + HM
        var highestPoint = 8849;
        var deepestPoint = 10909;
        for (int i = 0; i < meshData.Vertices.Length / 8; i++)
        {
            var gps = SphereToGpsCoords(new Vector3(
                meshData.Vertices[i * 8 + 0],
                meshData.Vertices[i * 8 + 1],
                meshData.Vertices[i * 8 + 2]
            ));
            gps.X += 180f;
            gps.X /= 360f;
            gps.Y += 90f;
            gps.Y /= 180f;

            meshData.Vertices[i * 8 + 3] = gps.X;
            meshData.Vertices[i * 8 + 4] = gps.Y;
            
            //HM
            var height = HeightMap.SampleTexture(new Vector2i((int)(gps.X * HeightMap.Image.Width), (int)(gps.Y * HeightMap.Image.Height))).X / 255f;
            var realHeight = height * (highestPoint + deepestPoint) - deepestPoint;
            if (realHeight > 0f)
            {
                height -= deepestPoint / (float)(highestPoint + deepestPoint);
                height *= deepestPoint / (float)(highestPoint + deepestPoint);
            }
            else height = 0f;
            
            height *= Scale;
            height += MidLevel;
            meshData.Vertices[i * 8 + 0] *= height;
            meshData.Vertices[i * 8 + 1] *= height;
            meshData.Vertices[i * 8 + 2] *= height;

            //sb.Append("[" + Math.Floor(gps.X * 100f) / 100f + ", " + Math.Floor(gps.Y * 100f) / 100f + "], ");
            //if ((i + 1) % resolution == 0) sb.Append("\n");
        }
        //Console.WriteLine(sb.ToString());
        
        var triangle = new Vector3[3];
        for (int i = 0; i < meshData.Indices.Length / 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                triangle[j] = new Vector3(
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 0],
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 1],
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 2]);
            }

            var norm = MeshConstructor.CalculateNormal(triangle);
            for (int j = 0; j < 3; j++)
            {
                meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 5] = norm.X;
                meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 6] = norm.Y;
                meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 7] = norm.Z;
            }
        }
        
        return meshData;
    }

    public void SetEarthToCoords(Vector2 coords)
    {
        coords *= MathF.PI / 180f;
        EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(0f, coords.Y, coords.X).Inverted();
    }

    public float GetHeightOnEarth(Vector2 coords)
    {
        coords.X += 180f;
        coords.X /= 360f;
        coords.Y += 90f;
        coords.Y /= 180f;
        
        return HeightMap.SampleTexture(new Vector2i((int)(coords.Y * HeightMap.Image.Width), (int)(coords.X * HeightMap.Image.Height))).X / 255f;
    }

    public static Vector3 GpsToSphereCoords(Vector2 gps)
    {
        gps *= MathF.PI / 180f;
        
        var x = MathF.Cos(gps.Y) * MathF.Sin(gps.X);
        var y = MathF.Sin(gps.Y);
        var z = MathF.Cos(gps.Y) * MathF.Cos(gps.X);
        return new Vector3(x, -y, z);
    }
    
    public static Vector2 SphereToGpsCoords(Vector3 s)
    {
        var lat = MathF.Asin(s.Y);
        var lon = MathF.Atan2(s.X, s.Z);
        return new Vector2(lon, lat) * (180f / MathF.PI);
    }

    public static Vector2 QuaternionToGpsCoords(Quaternion q)
    {
        var rv = Vector3.Transform(Vector3.UnitY, q.Inverted());

        //var r = rv.Length;
        var t = MathF.Asin(rv.Y);
        var p = MathF.Atan2(rv.X, rv.Z);

        return new Vector2(MathHelper.RadiansToDegrees(t), MathHelper.RadiansToDegrees(p));
    }
    
    public static Quaternion LookRotation(Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        var right = Vector3.Normalize(Vector3.Cross(up, forward));
        var newUp = Vector3.Cross(forward, right);
        
        var rotationMatrix = new Matrix3(
            right.X, newUp.X, forward.X,
            right.Y, newUp.Y, forward.Y,
            right.Z, newUp.Z, forward.Z
        );

        return Quaternion.FromMatrix(rotationMatrix);
    }
    
}