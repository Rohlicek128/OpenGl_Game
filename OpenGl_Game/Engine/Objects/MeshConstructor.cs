using Assimp;
using Assimp.Configs;
using Assimp.Unmanaged;
using OpenTK.Mathematics;
using OpenGl_Game.Engine.Graphics.Buffers;
using BoundingBox = OpenGl_Game.Engine.Objects.Collisions.BoundingBox;

namespace OpenGl_Game.Engine.Objects;

public class MeshConstructor
{
    public static EngineObject LoadFromFile(string path, VertexAttribute[] vertexAttribs)
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
            new MeshData(CombineVerticesData(vertexAttribs, vertices, texCoords, normals, indices), FormatIndicesData(indices)),
            new Material(new Vector3(1f))
        );
    }

    public static MeshData LoadObjFromFileAssimp(string path, bool removeFirst = false)
    {
        var importer = new AssimpContext();
        importer.SetConfig(new NormalSmoothingAngleConfig(66f));
        var scene = importer.ImportFile(RenderEngine.DirectoryPath + @"Assets\" + path, PostProcessPreset.TargetRealTimeMaximumQuality);

        float[] vertices = [];
        uint[] indices = [];
        uint indsMax = 0;

        var min = Vector3.Zero;
        var max = Vector3.Zero;

        if (removeFirst) scene.Meshes.RemoveAt(0);
        foreach (var mesh in scene.Meshes)
        {
            var verts = new float[mesh.Vertices.Count * 8];
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    verts[i * 8 + j] = mesh.Vertices[i][j];
                    if (min[j] > mesh.Vertices[i][j]) min[j] = mesh.Vertices[i][j];
                    if (max[j] < mesh.Vertices[i][j]) max[j] = mesh.Vertices[i][j];
                }

                if (mesh.TextureCoordinateChannelCount > 0)
                {
                    for (var j = 0; j < 2; j++) verts[i * 8 + j + 3] = mesh.TextureCoordinateChannels[0][i][j];
                }
                else
                {
                    for (var j = 0; j < 2; j++) verts[i * 8 + j + 3] = 0f;
                }
                for (var j = 0; j < 3; j++) verts[i * 8 + j + 5] = mesh.Normals[i][j];
            }

            vertices = vertices.Concat(verts).ToArray();

            var inds = mesh.GetUnsignedIndices();
            for (var i = 0; i < inds.Length; i++) inds[i] += indsMax;
            
            indices = indices.Concat(inds).ToArray();
            indsMax = indices.Max() + 1;
        }
        
        return new MeshData(vertices, indices, new BoundingBox(min, max, (max + min) / 2f));
    }

    public static float[] CombineVerticesData(VertexAttribute[] attributes, List<Vector3> vertices, List<Vector3> texCoords, List<Vector3> normals, List<List<Vector3i>> indices)
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
                    if (attributes[i].Type == VertexAttributeType.Position) result[index] = vertices[v][j];
                    if (attributes[i].Type == VertexAttributeType.TextureCoords) result[index] = texCoords[FindVertexInIndices(indices, v).Y][j];
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

            var offset = attributes.TakeWhile(attribute => attribute.Type != VertexAttributeType.Normal).Sum(attribute => attribute.Size);

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
    
    public static Vector3 CalculateNormal(Vector3[] t)
    {
        var u = t[1] - t[0];
        var v = t[2] - t[0];

        return new Vector3(
            u.Y * v.Z - u.Z * v.Y,
            u.Z * v.X - u.X * v.Z,
            u.X * v.Y - u.Y * v.X
        );
    }
    
    public static Vector3 CalculateTangent(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        var e1 = v2 - v1;
        var e2 = v3 - v1;
        var d1 = uv2 - uv1;
        var d2 = uv3 - uv1;

        var f = 1f / (d1.X * d2.Y - d2.X * d1.Y);

        return new Vector3(
            f * (d2.Y * e1.X - d1.Y * e2.X),
            f * (d2.Y * e1.Y - d1.Y * e2.Y),
            f * (d2.Y * e1.Z - d1.Y * e2.Z)
        );
    }

    public static Vector3 CubeToSphereVertex(Vector3 v)
    {
        var x2 = v.X * v.X;
        var y2 = v.Y * v.Y;
        var z2 = v.Z * v.Z;
        var x = v.X * MathF.Sqrt(1 - (y2 + z2) / 2 + (y2 * z2) / 3);
        var y = v.Y * MathF.Sqrt(1 - (z2 + x2) / 2 + (z2 * x2) / 3);
        var z = v.Z * MathF.Sqrt(1 - (x2 + y2) / 2 + (x2 * y2) / 3);
        return new Vector3(x, y, z);
    }

    public static MeshData CreateFace(Vector3 normal, int resolution, bool isSpherical = false)
    {
        var axisA = normal.Yzx;
        var axisB = Vector3.Cross(normal, axisA);

        var vertices = new float[(resolution) * resolution * 8];
        var indices = new uint[(resolution - 1) * (resolution - 1) * 6];
        var triIndex = 0;

        //var divider = false;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                var vertexIndex = x + y * resolution;
                var t = new Vector2(x, y) / (resolution - 1);
                var v = normal + axisA * (2 * t.X - 1) + axisB * (2 * t.Y - 1);
                if (isSpherical) v = CubeToSphereVertex(v);
                
                for (int i = 0; i < 3; i++) vertices[vertexIndex * 8 + i + 0] = v[i]; //Vertex
                //Tex Coords
                vertices[vertexIndex * 8 + 3] = x / (resolution - 1f);
                vertices[vertexIndex * 8 + 4] = y / (resolution - 1f);
                for (int i = 0; i < 3; i++) vertices[vertexIndex * 8 + i + 5] = normal[i]; //Normal

                if (x != resolution - 1 && y != resolution - 1)
                {
                    indices[triIndex + 0] = (uint)vertexIndex;
                    indices[triIndex + 1] = (uint)(vertexIndex + resolution + 1);
                    indices[triIndex + 2] = (uint)(vertexIndex + resolution);
                    indices[triIndex + 3] = (uint)(vertexIndex);
                    indices[triIndex + 4] = (uint)(vertexIndex + 1);
                    indices[triIndex + 5] = (uint)(vertexIndex + resolution + 1);
                    triIndex += 6;
                }
                /*if (!divider && y == (int)Math.Floor(resolution / 2f) - 1)
                {
                    x--;
                    divider = true;
                }*/
            }
        }

        return new MeshData(vertices, indices);
    }

    public static MeshData CombineMeshData(MeshData[] data)
    {
        var vertices = data[0].Vertices;
        for (var i = 1; i < data.Length; i++)
        {
            vertices = vertices.Concat(data[i].Vertices).ToArray();
        }
        
        var indices = data[0].Indices;
        for (var i = 1; i < data.Length; i++)
        {
            var offset = indices.Max() + 1;

            for (var j = 0; j < data[i].Indices.Length; j++)
            {
                if (data[i].Indices[j] == RenderEngine.PrimitiveIndex) continue;
                data[i].Indices[j] += offset;
                if (data[i].Indices[j] == 0) Console.WriteLine("ZERO FOUND");
            }
            indices = indices.Concat(data[i].Indices).ToArray();
        }

        return new MeshData(vertices, indices);
    }

    public static MeshData CreateCube()
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

        return new MeshData(vertices, indices);
    }
    
    public static MeshData CreatePlane(float texScaling = 1f)
    {
        float[] vertices =
        [
            1f, 0f,  1f,  0.0f, 0.0f,  0.0f,  1.0f,  0.0f,
            1f, 0f, -1f,  texScaling, 0.0f,  0.0f,  1.0f,  0.0f,
            -1f, 0f, -1f,  texScaling, texScaling,  0.0f,  1.0f,  0.0f,
            -1f, 0f,  1f,  0.0f, texScaling,  0.0f,  1.0f,  0.0f
        ];
        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0
        ];

        return new MeshData(vertices, indices);
    }
    
    public static MeshData CreateQuad(float mw = -1f, float mh = -1f, float w = 1f, float h = 1f, float texScaling = 1f)
    {
        float[] vertices =
        [
            w,  h,  0.0f, 0.0f,
            w, mh,  texScaling, 0.0f,
            mw, mh,  texScaling, texScaling,
            
            w,  h,  0.0f, 0.0f,
            mw, mh,  texScaling, texScaling,
            mw,  h,  0.0f, texScaling
        ];
        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0
        ];

        return new MeshData(vertices, indices);
    }

    public static float[] CreateRenderQuad()
    {
        return [
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f,  0.0f, 1.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 1.0f
        ];
    }

    public static EngineObject CreateScreenQuad()
    {
        var result = EngineObject.CreateEmpty();
        result.MeshData.Vertices = CreateRenderQuad();
        return result;
    }

}