using OpenGl_Game.Buffers;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders;

public abstract class ShaderProgram
{
    public readonly int Handle;

    public VertexBuffer VertexBuffer;
    public IndexBuffer IndexBuffer;
    public VertexArrayBuffer ArrayBuffer;
    
    public List<EngineObject> Objects;
    public VertexAttribute[] Attributes;
    
    public readonly ShaderAttribute[] ShaderAttributes;
    public readonly ShaderUniform[] ShaderUniforms;

    public ShaderProgram(Shader[] shaders, List<EngineObject> objects, VertexAttribute[] attributes, BufferUsage hint = BufferUsage.StaticDraw, bool addTangent = false)
    {
        Objects = objects;
        Attributes = attributes;
        var meshData = new MeshData(VertexBuffer.CombineBufferData(Objects.Select(o => o.MeshData.Vertices).ToArray()),
            IndexBuffer.CombineIndexBuffers(Objects.Select(o => o.MeshData.Indices).ToArray()));
        if (addTangent) meshData = AddTangents(meshData, ref Attributes);
        
        VertexBuffer = new VertexBuffer(meshData.Vertices, Attributes, hint);
        IndexBuffer = new IndexBuffer(meshData.Indices);
        ArrayBuffer = new VertexArrayBuffer(VertexBuffer);
        
        Handle = GL.CreateProgram();

        foreach (var shader in shaders) GL.AttachShader(Handle, shader.Handle);
        GL.LinkProgram(Handle);

        foreach (var shader in shaders)
        {
            GL.DetachShader(Handle, shader.Handle);
            GL.DeleteShader(shader.Handle);
        }

        ShaderAttributes = CreateAttributeList();
        ShaderUniforms = CreateUniformList();
    }
    
    public ShaderProgram(Shader[] shaders, ShaderProgram otherProgram)
    {
        Objects = otherProgram.Objects;
        VertexBuffer = otherProgram.VertexBuffer;
        IndexBuffer = otherProgram.IndexBuffer;
        ArrayBuffer = otherProgram.ArrayBuffer;
        
        Handle = GL.CreateProgram();

        foreach (var shader in shaders) GL.AttachShader(Handle, shader.Handle);
        GL.LinkProgram(Handle);

        foreach (var shader in shaders)
        {
            GL.DetachShader(Handle, shader.Handle);
            GL.DeleteShader(shader.Handle);
        }

        ShaderAttributes = CreateAttributeList();
        ShaderUniforms = CreateUniformList();
    }
    
    public ShaderProgram(Shader[] shaders)
    {
        Handle = GL.CreateProgram();

        foreach (var shader in shaders) GL.AttachShader(Handle, shader.Handle);
        GL.LinkProgram(Handle);

        foreach (var shader in shaders)
        {
            GL.DetachShader(Handle, shader.Handle);
            GL.DeleteShader(shader.Handle);
        }

        ShaderAttributes = CreateAttributeList();
        ShaderUniforms = CreateUniformList();
    }

    public void AddEngineObject(EngineObject eo, VertexAttribute[] attributes)
    {
        eo.MeshData = AddTangents(eo.MeshData, ref attributes);
        Objects.Add(eo);
        
        VertexBuffer.Add(eo.MeshData.Vertices);
        IndexBuffer.Add(eo.MeshData.Indices);
    }
    
    public MeshData AddTangents(MeshData meshData, ref VertexAttribute[] attributes)
    {
        //Attributes
        if (attributes.All(a => a.Type != VertexAttributeType.Tangent))
        {
            var attribs = new VertexAttribute[attributes.Length + 1];
            for (int i = 0; i < attributes.Length; i++) attribs[i] = attributes[i];
            attribs[attributes.Length] = new VertexAttribute(VertexAttributeType.Tangent, 3);
            attributes = attribs;
        }
        
        //Vertices
        var verts = new float[(meshData.Vertices.Length / 8) * 11];
        
        var triangle = new Vector3[3];
        var uv = new Vector2[3];
        for (int i = 0; i < meshData.Indices.Length / 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                triangle[j] = new Vector3(
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 0],
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 1],
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 2]
                );
                uv[j] = new Vector2(
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 3],
                    meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + 4]
                );
            }

            var tangent = MeshConstructor.CalculateTangent(triangle[0], triangle[1], triangle[2], uv[0], uv[1], uv[2]);
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    verts[meshData.Indices[i * 3 + j] * 11 + k] = meshData.Vertices[meshData.Indices[i * 3 + j] * 8 + k];
                }
                verts[meshData.Indices[i * 3 + j] * 11 + 8] = tangent.X;
                verts[meshData.Indices[i * 3 + j] * 11 + 9] = tangent.Y;
                verts[meshData.Indices[i * 3 + j] * 11 + 10] = tangent.Z;
            }
        }

        return new MeshData(verts, meshData.Indices);
    }

    public virtual void SetUniforms(params object[] param)
    {
        
    }

    public virtual void Draw(params object[] param)
    {
        
    }

    protected void DrawEachObject(Matrix4 view)
    {
        var offset = 0;
        foreach (var engineObject in Objects)
        {
            if (engineObject.IsVisible)
            {
                engineObject.DrawObject(this, offset, view);
            }
            offset += engineObject.MeshData.Indices.Length * sizeof(uint);
        }
    }

    public virtual void DeleteAll()
    {
        
    }

    public void BindAll()
    {
        Use();
        ArrayBuffer.Bind();
        IndexBuffer.Bind();
    }

    public void UnbindAll()
    {
        IndexBuffer.Unbind();
        ArrayBuffer.Unbind();
        Unbind();
    }
    
    private ShaderAttribute[] CreateAttributeList()
    {
        GL.GetProgrami(Handle, ProgramProperty.ActiveAttributes, out var attributeCount);

        var attributes = new ShaderAttribute[attributeCount];

        for (uint i = 0; i < attributeCount; i++)
        {
            GL.GetActiveAttrib(Handle, i, 256, out _, out _, out var type, out var name);
            var location = GL.GetAttribLocation(Handle, name);
            attributes[i] = new ShaderAttribute(name, location, type);
        }

        return attributes;
    }

    private ShaderUniform[] CreateUniformList()
    {
        GL.GetProgrami(Handle, ProgramProperty.ActiveUniforms, out var uniformCount);

        var uniforms = new ShaderUniform[uniformCount];

        for (uint i = 0; i < uniformCount; i++)
        {
            GL.GetActiveUniform(Handle, i, 256, out _, out _, out var type, out var name);
            var location = GL.GetUniformLocation(Handle, name);
            uniforms[i] = new ShaderUniform(name, location, type);
        }

        return uniforms;
    }

    public int GetUniformLocation(string name)
    {
        foreach (var uniform in ShaderUniforms)
        {
            if (name.Equals(uniform.Name)) return uniform.Location;
        }

        return -1;
    }
    
    public void SetUniform(string name, float value)
    {
        var loc = GetUniformLocation(name);
        if (loc == -1) return;
        GL.Uniform1f(loc, value);
    }
    
    public void SetUniform(string name, int value)
    {
        var loc = GetUniformLocation(name);
        if (loc == -1) return;
        GL.Uniform1i(loc, value);
    }
    
    public void SetUniform(string name, Vector2 value)
    {
        var loc = GetUniformLocation(name);
        if (loc == -1) return;
        GL.Uniform2f(loc, 1, ref value);
    }
    
    public void SetUniform(string name, Vector3 value)
    {
        var loc = GetUniformLocation(name);
        if (loc == -1) return;
        GL.Uniform3f(loc, 1, ref value);
    }
    
    public void SetUniform(string name, Vector4 value)
    {
        var loc = GetUniformLocation(name);
        if (loc == -1) return;
        GL.Uniform4f(GetUniformLocation(name), 1, ref value);
    }

    public void SetUniform(string name, Matrix4 value, bool transpose = true)
    {
        var loc = GetUniformLocation(name);
        if (loc == -1) return;
        GL.UniformMatrix4f(GetUniformLocation(name), 1, transpose, ref value);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void Unbind()
    {
        GL.UseProgram(0);
    }

    public void Delete()
    {
        VertexBuffer.Delete();
        IndexBuffer.Delete();
        ArrayBuffer.Delete();
        
        Unbind();
        GL.DeleteProgram(Handle);
    }
}

public struct ShaderAttribute
{
    public string Name;
    public int Location;
    public AttributeType Type;

    public ShaderAttribute(string name, int location, AttributeType type)
    {
        Name = name;
        Location = location;
        Type = type;
    }
}

public struct ShaderUniform
{
    public string Name;
    public int Location;
    public UniformType Type;

    public ShaderUniform(string name, int location, UniformType type)
    {
        Name = name;
        Location = location;
        Type = type;
    }
}