using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StbImageSharp;
using VertexAttribType = OpenGl_Game.Engine.Graphics.Buffers.VertexAttribType;

namespace OpenGl_Game.Engine.Graphics.Textures;

public class HeightMap
{
    public EngineObject TerrainObject;
    public float TerrainScale;
    
    public HeightMap(string path, VertexAttribute[] attributes)
    {
        TerrainScale = 2f;
        
        StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromStream(File.OpenRead(RenderEngine.DirectoryPath + @"Assets\" + path), ColorComponents.RedGreenBlue);
        GenMeshData(image, attributes, out var verts, out var inds);
        
        TerrainObject = new EngineObject(
            "Terrain",
            new Transform(new Vector3(0f), new Vector3(0f), new Vector3(TerrainScale)),
            new VerticesData(verts, PrimitiveType.TriangleStrip),
            inds,
            //new Material(new Vector3(74f / 255f, 149f / 255f, 207f / 255f))
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, new Texture(path, 0, TextureMagFilter.Linear, TextureMagFilter.Linear)},
                {TextureTypes.Specular, new Texture("black1x1.png", 1)}
            })
        );
    }

    public void GenMeshData(ImageResult hm, VertexAttribute[] attributes, out float[] vertices, out uint[] indices)
    {
        var yScale = 0.08f;
        var yShift = 0.5f;

        //Vertices
        List<Vector3> verts = [];
        List<Vector2> texCoords = [];
        
        for (int x = 0; x < hm.Width; x++)
        {
            for (int y = 0; y < hm.Height; y++)
            {
                var hmY = hm.Data[(x + hm.Width * y) * (int)hm.SourceComp];
                verts.Add(new Vector3(
                    -hm.Height/2f + y,
                    hmY * yScale - yShift,
                    //-0.5f,
                    -hm.Width/2f + x
                ));
                
                texCoords.Add(new Vector2(x / (float)hm.Width, y / (float)hm.Height) / TerrainScale);
            }
        }
        
        //Indices
        List<uint> inds = [];
        for (int y = 0; y < hm.Width - 1; y++)
        {
            for (int x = 0; x < hm.Height; x++)
            {
                if (x == hm.Height - 1) inds.Add(RenderEngine.PrimitiveIndex);
                else
                {
                    for (int k = 0; k < 2; k++)
                    {
                        inds.Add((uint)(x + hm.Height * (y + k)));
                    }
                }
            }
        }
        indices = inds.ToArray();
        
        var stride = attributes.Sum(a => a.Size);
        vertices = new float[verts.Count * stride];
        
        for (int v = 0; v < verts.Count; v++)
        {
            var t = (int)(Math.Floor(v / 3f) * 3);
            Vector3 normal;
            try
            {
                if (t + 2 >= verts.Count) normal = new Vector3(0f, -1f, 0f); 
                else normal = ObjFileLoader.CalculateNormal(verts[t], verts[t + hm.Height], verts[t + 1]);
            }
            catch (Exception e)
            {
                normal = new Vector3(0f, -1f, 0f);
            }
            
            var offset = 0;
            for (int i = 0; i < attributes.Length; i++)
            {
                for (int j = 0; j < attributes[i].Size; j++)
                {
                    var index = (v * stride) + offset + j;
                    if (attributes[i].Type == VertexAttribType.Position) vertices[index] = verts[v][j];
                    if (attributes[i].Type == VertexAttribType.TextureCoords) vertices[index] = texCoords[v][j];
                    if (attributes[i].Type == VertexAttribType.Normal) vertices[index] = normal[j];
                }

                offset += attributes[i].Size;
            }
        }
    }
}