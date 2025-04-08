using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class LaserShader : ShaderProgram
{
    public LaserShader(EngineObject laser, VertexAttribute[] vertexAttributes) : base(
        [
            new Shader(@"laserShaders\laser.vert", ShaderType.VertexShader),
            new Shader(@"LightShaders\lightShader.frag", ShaderType.FragmentShader)
        ], [laser], vertexAttributes
    )
    {
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("laser", Random.Shared.NextSingle() * 0.5f + 0.5f);
        SetUniform("world", (Matrix4)param[0]);
        SetUniform("view", (Matrix4)param[1]);
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        SetUniforms(param[0], param[1]);
        
        DrawEachObject((Matrix4)param[1]);
        
        UnbindAll();
    }
    
    public override void DeleteAll()
    {
        Delete();
    }
}