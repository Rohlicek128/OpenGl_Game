using OpenTK.Mathematics;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenTK.Graphics.OpenGL;
using Attribute = OpenGl_Game.Engine.Graphics.Buffers.Attribute;

namespace OpenGl_Game.Engine.Objects;

public class ObjFileLoader
{
    public static EngineObject LoadFromFile(string path, Attribute[] vertexAttribs)
    {
        string? name = null;
        
        var vertices = new List<Vector3>();
        var texCoords = new List<Vector3>();
        var normals = new List<Vector3>();
        var indices = new List<List<Vector3i>>();
        
        using var sr = new StreamReader(File.OpenRead(RenderEngine.DirectoryPath + @"Assets\" + path));
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.Equals("")) continue;
            
            var elementsList = line.Split(" ").ToList();
            foreach (var element in elementsList.ToList().Where(element => element.Equals(""))) elementsList.Remove(element);
            var elements = elementsList.ToArray();

            if (elements[0].Equals("v"))
            {
                vertices.Add(new Vector3(
                    float.Parse(elements[1]),
                    float.Parse(elements[2]),
                    float.Parse(elements[3]))
                );
            }
            else if (elements[0].Equals("vt"))
            {
                texCoords.Add(new Vector3(
                    float.Parse(elements[1]),
                    elements.Length >= 3 && !elements[1].Equals("") ? float.Parse(elements[1]) : 0,
                    elements.Length >= 4 && !elements[2].Equals("") ? float.Parse(elements[2]) : 0)
                );
            }
            else if (elements[0].Equals("vn"))
            {
                normals.Add(new Vector3(
                    float.Parse(elements[1]),
                    float.Parse(elements[2]),
                    float.Parse(elements[3]))
                );
            }
            else if (elements[0].Equals("f"))
            {
                var index = new List<Vector3i>();
                indices.Add(index);
                
                for (var i = 1; i < elements.Length; i++)
                {
                    var faceSplit = elements[i].Split("/");
                    index.Add(new Vector3i(
                        int.Parse(faceSplit[0]) - 1,
                        faceSplit.Length >= 2 && !faceSplit[1].Equals("") ? int.Parse(faceSplit[1]) - 1 : -1,
                        faceSplit.Length >= 3 && !faceSplit[2].Equals("") ? int.Parse(faceSplit[2]) - 1 : -1
                    ));
                }

                if (index.Count == 4)
                {
                    var last = index.Last();
                    index.RemoveAt(index.Count - 1);

                    var another = new List<Vector3i>();
                    indices.Add(another);
                    
                    another.Add(index[0]);
                    another.Add(index.Last());
                    another.Add(last);
                }
            }
            else if (name == null && elements is ["#", "object", _, ..])
            {
                name = elements[2];
            }
        }

        /*var origin = new Vector3(0f);
        foreach (var vertex in vertices)
        {
            origin.X += vertex.X;
            origin.Y += vertex.Y;
            origin.Z += vertex.Z;
        }
        origin /= vertices.Count;*/
        var origin = new Vector3(
            -vertices.Average(v => v.X), 
            -vertices.Average(v => v.Y), 
            -vertices.Average(v => v.Z)
        );

        return new EngineObject(
            name ?? "N/A",
            new Transform(origin),
            new VerticesData(CombineVerticesData(vertexAttribs, vertices, texCoords, normals, indices), PrimitiveType.Triangles),
            FormatIndicesData(indices),
            new Material(new Vector3(1f))
        );
    }

    public static float[] CombineVerticesData(Attribute[] attributes, List<Vector3> vertices, List<Vector3> texCoords, List<Vector3> normals, List<List<Vector3i>> indices)
    {
        var stride = attributes.Sum(a => a.Size);
        var result = new float[vertices.Count * stride];
        
        for (int v = 0; v < vertices.Count; v++)
        {
            var offset = 0;
            for (int i = 0; i < attributes.Length; i++)
            {
                for (int j = 0; j < attributes[i].Size; j++)
                {
                    var index = (v * stride) + offset + j;
                    if (attributes[i].Type == AttribType.Position) result[index] = vertices[v][j];
                    if (attributes[i].Type == AttribType.TextureCoords) result[index] = texCoords[FindVertexInIndices(indices, v).Y][j];
                    //if (attributes[i].Type == AttribType.Normal) result[index] = CalculateNormal();
                }

                offset += attributes[i].Size;
            }
        }

        foreach (var face in indices)
        {
            var triangle = new Vector4[3];
            for (int i = 0; i < face.Count; i++)
            {
                triangle[i] = new Vector4(vertices[face[i].X], face[i].X);
            }

            var normal = CalculateNormal(triangle[0].Xyz, triangle[1].Xyz, triangle[2].Xyz);

            var offset = attributes.TakeWhile(attribute => attribute.Type != AttribType.Normal).Sum(attribute => attribute.Size);

            for (int i = 0; i < triangle.Length; i++)
            {
                result[((int)triangle[i].W * stride) + offset + 0] = normal.X;
                result[((int)triangle[i].W * stride) + offset + 1] = normal.Y;
                result[((int)triangle[i].W * stride) + offset + 2] = normal.Z;
            }
        }

        return result;
    }

    public static uint[] FormatIndicesData(List<List<Vector3i>> indices)
    {
        var stride = 3;
        var result = new uint[indices.Count * stride];

        for (int i = 0; i < indices.Count; i++)
        {
            for (int j = 0; j < indices[i].Count; j++)
            {
                result[(i * stride) + j] = (uint)indices[i][j].X;
            }
        }

        return result;
    }

    private static Vector3i FindVertexInIndices(List<List<Vector3i>> indices, int vertIndex)
    {
        foreach (var face in indices)
        {
            foreach (var vertex in face)
            {
                if (vertex.X == vertIndex) return vertex;
            }
        }

        throw new ArgumentException("No such Vertex (" + vertIndex + ") in Indices list!");
    }

    public static Vector3 CalculateNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var u = v2 - v1;
        var v = v3 - v1;

        return new Vector3(
            u.Y * v.Z - u.Z * v.Y,
            u.Z * v.X - u.X * v.Z,
            u.X * v.Y - u.Y * v.X
        );
    }

    public static float[] CreateCubeVertices()
    {
        float[] vertices =
        [
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.0f,  0.0f, -1.0f, // A 0
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f,  0.0f, -1.0f, // B 1
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f, 0.0f,  0.0f, -1.0f, // C 2
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f, 0.0f,  0.0f, -1.0f, // D 3
            
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,  0.0f,  0.0f,  1.0f, // E 4
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,  0.0f,  0.0f,  1.0f, // F 5
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,  0.0f,  0.0f,  1.0f, // G 6
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,  0.0f,  0.0f,  1.0f, // H 7
 
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f, -1.0f,  0.0f,  0.0f, // D 8
            -0.5f, -0.5f, -0.5f,  1.0f, 0.0f, -1.0f,  0.0f,  0.0f, // A 9
            -0.5f, -0.5f,  0.5f,  1.0f, 1.0f, -1.0f,  0.0f,  0.0f, // E 10
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f, -1.0f,  0.0f,  0.0f, // H 11
            
            0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 1.0f,  0.0f,  0.0f,  // B 12
            0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 1.0f,  0.0f,  0.0f,  // C 13
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 1.0f,  0.0f,  0.0f,  // G 14
            0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 1.0f,  0.0f,  0.0f,  // F 15
 
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, 0.0f, -1.0f,  0.0f, // A 16
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f, 0.0f, -1.0f,  0.0f,  // B 17
            0.5f, -0.5f,  0.5f,  1.0f, 1.0f, 0.0f, -1.0f,  0.0f,  // F 18
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f, 0.0f, -1.0f,  0.0f, // E 19
            
            0.5f,  0.5f, -0.5f,   0.0f, 0.0f, 0.0f,  1.0f,  0.0f, // C 20
            -0.5f,  0.5f, -0.5f,  1.0f, 0.0f, 0.0f,  1.0f,  0.0f, // D 21
            -0.5f,  0.5f,  0.5f,  1.0f, 1.0f, 0.0f,  1.0f,  0.0f, // H 22
            0.5f,  0.5f,  0.5f,   0.0f, 1.0f, 0.0f,  1.0f,  0.0f  // G 23
        ];

        return vertices;
    }

    public static uint[] CreateCubeIndices()
    {
        uint[] indices =
        [
            // front and back
            0, 3, 2,
            2, 1, 0,
            4, 5, 6,
            6, 7 ,4,
            // left and right
            11, 8, 9,
            9, 10, 11,
            12, 13, 14,
            14, 15, 12,
            // bottom and top
            16, 17, 18,
            18, 19, 16,
            20, 21, 22,
            22, 23, 20
        ];

        return indices;
    }
    
    public static float[] CreatePlaneVertices(float texScaling)
    {
        float[] vertices =
        [
            1f, 0f,  1f,  0.0f, 0.0f,  0.0f,  1.0f,  0.0f,
            1f, 0f, -1f,  texScaling, 0.0f,  0.0f,  1.0f,  0.0f,
            -1f, 0f, -1f,  texScaling, texScaling,  0.0f,  1.0f,  0.0f,
            -1f, 0f,  1f,  0.0f, texScaling,  0.0f,  1.0f,  0.0f
        ];

        return vertices;
    }

    public static uint[] CreatePlaneIndices()
    {
        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0
        ];

        return indices;
    }
    
    public static float[] CreateQuadVertices(float texScaling)
    {
        /*float[] vertices =
        [
            1f,  1f,  0.0f, 0.0f,
            1f, -1f,  texScaling, 0.0f,
            -1f, -1f,  texScaling, texScaling,
            -1f,  1f,  0.0f, texScaling
        ];*/
        
        float[] vertices =
        [
            1f,  1f,  0.0f, 0.0f,
            1f, -1f,  texScaling, 0.0f,
            -1f, -1f,  texScaling, texScaling,
            
            1f,  1f,  0.0f, 0.0f,
            -1f, -1f,  texScaling, texScaling,
            -1f,  1f,  0.0f, texScaling
        ];

        return vertices;
    }

}