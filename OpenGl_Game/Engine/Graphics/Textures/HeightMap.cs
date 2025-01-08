using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;
using StbImageSharp;
using Attribute = OpenGl_Game.Engine.Graphics.Buffers.Attribute;

namespace OpenGl_Game.Engine.Graphics.Textures;

public class HeightMap
{
    public EngineObject TerrainObject;
    
    public HeightMap(string path, Attribute[] attributes)
    {
        var image = ImageResult.FromStream(File.OpenRead(RenderEngine.DirectoryPath + @"Assets\" + path), ColorComponents.RedGreenBlue);
        GenMeshData(image, attributes, out var verts, out var inds);

        TerrainObject = new EngineObject(
            "Terrain",
            new Vector3(0f),
            verts,
            inds,
            new Material(new Vector3(1f))
        );
    }

    public void GenMeshData(ImageResult hm, Attribute[] attributes, out float[] vertices, out uint[] indices)
    {
        var yScale = 0.25f;
        var yShift = 16f;

        List<Vector3> verts = [];
        List<Vector2> texCoords = [];
        indices = new uint[hm.Width * hm.Width * 2];
        for (int x = 0; x < hm.Width; x++)
        {
            for (int y = 0; y < hm.Height; y++)
            {
                var hmY = hm.Data[x * hm.Width + y];
                
                verts.Add(new Vector3(
                    -hm.Height/2f + y,
                    hmY * yScale - yShift,
                    -hm.Width/2f + x
                ));
                
                texCoords.Add(new Vector2(x / (float)hm.Width, y / (float)hm.Height));

                for (int k = 0; k < 2; k++)
                {
                    indices[(x * hm.Width) + (y * k)] = (uint)(hm.Width * (y * k) + x);
                }
            }
        }
        
        var stride = attributes.Sum(a => a.Size);
        vertices = new float[verts.Count * stride];
        
        for (int v = 0; v < verts.Count; v++)
        {
            var t = (int)(Math.Floor(v / 3f) * 3);
            Vector3 normal;
            if (t + 2 >= verts.Count) normal = new Vector3(1f); 
            else normal = ObjFileLoader.CalculateNormal(verts[t], verts[t + 1], verts[t + 2]);
            
            var offset = 0;
            for (int i = 0; i < attributes.Length; i++)
            {
                for (int j = 0; j < attributes[i].Size; j++)
                {
                    var index = (v * stride) + offset + j;
                    if (attributes[i].Type == AttribType.Position) vertices[index] = verts[v][j];
                    if (attributes[i].Type == AttribType.TextureCoords) vertices[index] = texCoords[v][j];
                    if (attributes[i].Type == AttribType.Normal) vertices[index] = normal[j];
                }

                offset += attributes[i].Size;
            }
        }
    }
}