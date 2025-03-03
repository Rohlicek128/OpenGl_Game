using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game;

public class Earth
{
    public EngineObject EarthObject;
    
    public Texture ColorMap;
    public Texture RoughnessMap;
    public Texture NormalMap;
    public Texture CitiesMap;
    
    public Texture HeightMap;
    public float Scale;
    public float MidLevel;
    
    public Earth(Transform transform, int resolution, float scale, float midLevel = 1f)
    {
        ColorMap = new Texture("Earth\\earth_color_mar.png", 0, TextureMagFilter.Linear, TextureMagFilter.Linear);
        RoughnessMap = new Texture("Earth\\earth_specular.png", 1, TextureMagFilter.Linear, TextureMagFilter.Linear);
        NormalMap = new Texture("Earth\\earth_normal_high2.png", 2, TextureMagFilter.Linear, TextureMagFilter.Linear);
        HeightMap = new Texture("Earth\\earth_height_water.png", 0, TextureMagFilter.Linear, TextureMagFilter.Linear);
        CitiesMap = new Texture("Earth\\earth_cities.png", 3, TextureMagFilter.Linear, TextureMagFilter.Linear);
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
                {TextureTypes.Overlay, CitiesMap}
            })
        );
    }

    public void MoveEarth(KeyboardState keyboard, float deltaTime, float boost)
    {
        var speed = deltaTime * boost / 250f;
        
        if (keyboard.IsKeyDown(Keys.W)) EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(-Vector3.UnitZ * speed) * EarthObject.Transform.Quaternion;
        if (keyboard.IsKeyDown(Keys.S)) EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitZ * speed) * EarthObject.Transform.Quaternion;
        if (keyboard.IsKeyDown(Keys.A)) EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitY * speed) * EarthObject.Transform.Quaternion;
        if (keyboard.IsKeyDown(Keys.D)) EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(-Vector3.UnitY * speed) * EarthObject.Transform.Quaternion;
        if (keyboard.IsKeyDown(Keys.Q)) EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(-Vector3.UnitX * deltaTime * 0.5f) * EarthObject.Transform.Quaternion;
        if (keyboard.IsKeyDown(Keys.E)) EarthObject.Transform.Quaternion = Quaternion.FromEulerAngles(Vector3.UnitX * deltaTime * 0.5f) * EarthObject.Transform.Quaternion;
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
    
}