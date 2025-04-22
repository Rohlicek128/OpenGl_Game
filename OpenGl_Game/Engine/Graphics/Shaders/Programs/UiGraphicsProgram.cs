using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class UiGraphicsProgram : ShaderProgram
{
    public UiGraphicsProgram(List<EngineObject> objects) : base([
        new Shader(@"uiGraphicsShaders\uiGraphics.vert", ShaderType.VertexShader),
        new Shader(@"uiGraphicsShaders\uiGraphics.frag", ShaderType.FragmentShader)
    ], objects, [new VertexAttribute(VertexAttributeType.PosAndTex, 4)])
    {
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("viewport", (Vector2)param[0]);
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        SetUniforms(param[0]);
        
        DrawEachObject(Matrix4.Zero);
        
        UnbindAll();
    }

    public override void DeleteAll()
    {
        Delete();
    }
}