using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.Objects.Collisions;
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

    public BurnEffect BurnEffect;
    
    public Earth(Transform transform, int resolution, float scale, float midLevel = 1f)
    {
        if (Settings.Instance.TextureQuality == 1) // High
        {
            ColorMap = new Texture("Earth\\earth_color_may.png", 0, TextureMinFilter.Linear, TextureMagFilter.Linear);
            NormalMap = new Texture("Earth\\earth_normal_high2.png", 2, TextureMinFilter.Linear, TextureMagFilter.Linear);
        }
        if (Settings.Instance.TextureQuality == 0) // Low
        {
            ColorMap = new Texture("Earth\\earth_color_may_low.png", 0, TextureMinFilter.Linear, TextureMagFilter.Linear);
            NormalMap = new Texture("Earth\\earth_normal.png", 2, TextureMinFilter.Linear, TextureMagFilter.Linear);
        }
        RoughnessMap = new Texture("Earth\\earth_specular.png", 1, TextureMinFilter.Linear, TextureMagFilter.Linear);
        HeightMap = new Texture("Earth\\earth_height_water.png", 0, TextureMinFilter.Linear, TextureMagFilter.Linear);
        CitiesMap = new Texture("Earth\\earth_cities.png", 3, TextureMinFilter.Linear, TextureMagFilter.Linear);
        Scale = scale;
        MidLevel = midLevel;
        
        BurnEffect = new BurnEffect(ColorMap!, new Vector2i(ColorMap!.Image.Width, ColorMap.Image.Height));
        
        EarthObject = new EngineObject(
            "Earth",
            transform,
            GenerateFaces(resolution + 1),
            //new Material(new Vector3(1f, 0f, 1f))
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, BurnEffect.Framebuffer.AttachedTextures[0]},
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

    public void Delete()
    {
        EarthObject.Textures.DeleteAll();
    }

    /// <summary>
    /// Rotates the earth forward with any attached objects
    /// </summary>
    /// <param name="keyboard"></param>
    /// <param name="deltaTime"></param>
    /// <param name="boost"></param>
    /// <param name="engineObjects"></param>
    /// <param name="debug">Allows additional controls</param>
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
            
            if (keyboard.IsKeyDown(Keys.Space)) EarthObject.Transform.Position[EarthAxis] -= deltaTime * 50f;
            if (keyboard.IsKeyDown(Keys.LeftControl)) EarthObject.Transform.Position[EarthAxis] += deltaTime * 50f;
        }
        
        CollisionSphere.Transform.Position = EarthObject.Transform.Position;
    }
    
    public void RotateEarth(float deltaTime, float amount, List<EngineObject> engineObjects)
    {
        if (amount == 0f) return;
        engineObjects.Add(EarthObject);
        
        foreach (var o in engineObjects) o.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitY * deltaTime * amount) * o.Transform.Quaternion;
    }

    /// <summary>
    /// Constructs each face of a cube and inflates them to a sphere and then applies a Cosine distribution
    /// </summary>
    /// <param name="resolution"></param>
    /// <returns></returns>
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

    public void SetRandomCoords(float landThreshold = 0f)
    {
        if (landThreshold > 0f) Console.WriteLine("start finding spawn");
        Vector2 coords;
        do
        {
            coords = new Vector2(Random.Shared.NextSingle() * 360f - 180f, Random.Shared.NextSingle() * 180f - 90f);
            EarthObject.Transform.Quaternion = GpsCoordsToQuaternion(coords, Random.Shared.NextSingle() * 360f);
        }
        while (landThreshold > 0f && RoughnessMap.SampleTexture(new Vector2i((int)((coords.X + 180f) / 360f * RoughnessMap.Image.Width), (int)((coords.Y + 90f) / 180f * RoughnessMap.Image.Width))).X / 255f > landThreshold);
        if (landThreshold > 0f) Console.WriteLine("end finding spawn");
    }

    public float GetHeightOnEarth(Vector2 coords)
    {
        return HeightMap.SampleTexture(new Vector2i(
            (int)((coords.Y + 180f) / 360f * HeightMap.Image.Width),
            (int)((coords.X + 90) / 180f * HeightMap.Image.Height)
        )).X / 255f;
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

    /// <summary>
    /// From quaternion rotation gets GPS coordinates (1/2 instance of AI use)
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static Vector2 QuaternionToGpsCoords(Quaternion q)
    {
        var rv = Vector3.Transform(Vector3.UnitY, q.Inverted());
        
        var t = MathF.Asin(rv.Y);
        var p = MathF.Atan2(rv.X, rv.Z);

        return new Vector2(MathHelper.RadiansToDegrees(t), MathHelper.RadiansToDegrees(p));
    }
    
    /// <summary>
    /// From GPS coordinates creates quaternion rotation (2/2 instance of AI use)
    /// </summary>
    /// <param name="gps"></param>
    /// <param name="rotationDeg"></param>
    /// <returns></returns>
    public static Quaternion GpsCoordsToQuaternion(Vector2 gps, float rotationDeg = 0f)
    {
        var tilt = MathHelper.DegreesToRadians(gps.Y);
        var pan = MathHelper.DegreesToRadians(gps.X + 180f);

        var dir = new Vector3(
            MathF.Sin(pan) * MathF.Cos(tilt),
            MathF.Sin(tilt),
            MathF.Cos(pan) * MathF.Cos(tilt)
        );

        dir = dir.Normalized();

        if (dir == Vector3.UnitY) return Quaternion.Identity;

        if (dir == -Vector3.UnitY) return Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI);

        var axis = Vector3.Cross(Vector3.UnitY, dir).Normalized();
        var angle = MathF.Acos(Vector3.Dot(Vector3.UnitY, dir));

        return Quaternion.FromEulerAngles(Vector3.UnitY * MathHelper.DegreesToRadians(rotationDeg)) * Quaternion.FromAxisAngle(axis, angle);
    }
    
}