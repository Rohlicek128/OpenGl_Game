using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class MapShader : ShaderProgram
{
    public MapShader(List<EngineObject> objects) : base
    (
        [
            new Shader(@"mapShaders\map.vert", ShaderType.VertexShader),
            new Shader(@"mapShaders\map.frag", ShaderType.FragmentShader)
        ], objects, [new VertexAttribute(VertexAttributeType.Position, 3)]
    )
    {
    }
    
    public override void SetUniforms(params object[] param)
    {
        SetUniform("world", (Matrix4)param[0]);
        SetUniform("viewPos", (Vector3)param[1]);
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        SetUniforms(param);
        
        DrawEachObject(Matrix4.Zero);
        
        UnbindAll();
    }

    public override void DeleteAll()
    {
        Delete();
    }
}