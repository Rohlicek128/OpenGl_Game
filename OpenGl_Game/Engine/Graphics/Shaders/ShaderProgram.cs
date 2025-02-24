using OpenGl_Game.Buffers;
using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VertexAttribType = OpenGl_Game.Engine.Graphics.Buffers.VertexAttribType;

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

    public ShaderProgram(Shader[] shaders, List<EngineObject> objects, VertexAttribute[] attributes, BufferUsage hint = BufferUsage.StaticDraw, bool addTangent = false)
    {
        Objects = objects;
        var meshData = new MeshData(VertexBuffer.CombineBufferData(Objects.Select(o => o.MeshData.Vertices).ToArray()),
            IndexBuffer.CombineIndexBuffers(Objects.Select(o => o.MeshData.Indices).ToArray()));
        if (addTangent) meshData = AddTangents(meshData, ref attributes);
        
        VertexBuffer = new VertexBuffer(meshData.Vertices, attributes, hint);
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

        Attributes = CreateAttributeList();
        Uniforms = CreateUniformList();
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

        Attributes = CreateAttributeList();
        Uniforms = CreateUniformList();
    }
    
    public MeshData AddTangents(MeshData meshData, ref VertexAttribute[] attributes)
    {
        //Attributes
        var attribs = new VertexAttribute[attributes.Length + 1];
        for (int i = 0; i < attributes.Length; i++) attribs[i] = attributes[i];
        attribs[attributes.Length] = new VertexAttribute(VertexAttribType.Tangent, 3);
        attributes = attribs;
        
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

    public void DrawMesh(Matrix4 worldMat, Matrix4 proj, Matrix4 view)
    {
        Use();
        ArrayBuffer.Bind();
        //IndexBuffer.Bind();
        
        /*SetUniform("skybox", 10);
        GL.ActiveTexture(TextureUnit.Texture10);
        GL.BindTexture(TextureTarget.TextureCubeMap, skyboxHandle);*/
        
        SetUniform("world", worldMat);
        SetUniform("projection", proj);
        SetUniform("view", view);
        
        var offset = 0;
        foreach (var engineObject in Objects)
        {
            if (engineObject.IsVisible) engineObject.DrawObject(this, offset, view);
            offset += engineObject.MeshData.Indices.Length * sizeof(uint);
        }
        
        ArrayBuffer.Unbind();
        //IndexBuffer.Unbind();
    }
    
    public void DrawMeshLighting(Dictionary<LightTypes, List<Light>> lights, Camera camera, ShadowMap shadowMap, Ssao ssao)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        Use();
        ArrayBuffer.Bind();
        IndexBuffer.Bind();
        GL.Disable(EnableCap.DepthTest);
        
        SetUniform("gPosition", 0);
        SetUniform("gNormal", 1);
        SetUniform("gAlbedoSpec", 2);
        
        SetUniform("shadowMap", 3);
        GL.ActiveTexture(TextureUnit.Texture3);
        GL.BindTexture(TextureTarget.Texture2d, shadowMap.TextureHandle);

        SetUniform("ssaoMap", 4);
        GL.ActiveTexture(TextureUnit.Texture4);
        GL.BindTexture(TextureTarget.Texture2d, ssao.BlurFramebuffer.AttachedTextures[0].Handle);
        
        SetUniform("lightSpace", shadowMap.LightSpace);
        SetUniform("viewPos", camera.Transform.Position);
        
        foreach (var lightList in lights)
        {
            if (lightList.Key == LightTypes.Directional) lightList.Value[0].SetUniformsForDirectional(this);
            else if (lightList.Key == LightTypes.Point)
            {
                for (int i = 0; i < lightList.Value.Count; i++) lightList.Value[i].SetUniformsForPoint(this, i);
            }
        }
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        ArrayBuffer.Unbind();
        IndexBuffer.Unbind();
        GL.Enable(EnableCap.DepthTest);
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