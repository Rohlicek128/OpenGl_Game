using OpenGl_Game.Buffers;
using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Attribute = OpenGl_Game.Engine.Graphics.Buffers.Attribute;

namespace OpenGl_Game.Shaders;

public class ShaderProgram
{
    public readonly int Handle;

    public VertexBuffer VertexBuffer;
    public IndexBuffer IndexBuffer;
    public VertexArrayBuffer ArrayBuffer;
    
    public List<EngineObject> Objects;
    
    public readonly ShaderAttribute[] Attributes;
    public readonly ShaderUniform[] Uniforms;

    public ShaderProgram(Shader[] shaders, List<EngineObject> objects, Attribute[] attributes, BufferUsage hint = BufferUsage.StaticDraw)
    {
        Objects = objects;
        VertexBuffer = new VertexBuffer(VertexBuffer.CombineBufferData(Objects.Select(o => o.VerticesData).ToArray()), attributes, hint);
        IndexBuffer = new IndexBuffer(IndexBuffer.CombineIndexBuffers(Objects.Select(o => o.IndicesData).ToArray()));
        ArrayBuffer = new VertexArrayBuffer(VertexBuffer);
        
        Handle = GL.CreateProgram();

        foreach (var shader in shaders) GL.AttachShader(Handle, shader.Handle);
        GL.LinkProgram(Handle);

        foreach (var shader in shaders)
        {
            GL.DetachShader(Handle, shader.Handle);
            GL.DeleteShader(shader.Handle);
        }

        Attributes = CreateAttributeList();
        Uniforms = CreateUniformList();
    }

    public void DrawMesh(Matrix4 worldMat, Dictionary<LightTypes, List<Light>> lights, Camera camera, int skyboxHandle)
    {
        Use();
        ArrayBuffer.Bind();
        IndexBuffer.Bind();
        
        SetUniform("skybox", 5);
        GL.ActiveTexture(TextureUnit.Texture5);
        GL.BindTexture(TextureTarget.TextureCubeMap, skyboxHandle);
        
        foreach (var lightList in lights)
        {
            if (lightList.Key == LightTypes.Directional) lightList.Value[0].SetUniformsForDirectional(this);
            else if (lightList.Key == LightTypes.Point)
            {
                for (int i = 0; i < lightList.Value.Count; i++) lightList.Value[i].SetUniformsForPoint(this, i);
            }
        }
        
        var offset = 0;
        foreach (var engineObject in Objects)
        {
            if (engineObject.IsVisible) engineObject.DrawObject(this, worldMat, camera, offset);
            offset += engineObject.IndicesData.Length * sizeof(uint);
        }
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
        foreach (var uniform in Uniforms)
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