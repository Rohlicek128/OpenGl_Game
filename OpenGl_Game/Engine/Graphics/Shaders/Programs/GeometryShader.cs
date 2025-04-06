using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class GeometryShader : ShaderProgram
{
    
    public Matrix4 ProjectionMat { get; set; }
    public Matrix4 ViewMat { get; set; }
    public Matrix4 WorldMat { get; set; }
    
    public GeometryShader(List<EngineObject> objects, VertexAttribute[] attributes) : base
    (
        [
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"geometryShaders\geometryShader.frag", ShaderType.FragmentShader)
        ], objects, attributes, addTangent:true
    )
    {
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("world", WorldMat);
        SetUniform("view", ViewMat);
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        SetUniforms();
        
        DrawEachObject(ViewMat);
        
        UnbindAll();
    }

    public override void DeleteAll()
    {
        Delete();
    }
}