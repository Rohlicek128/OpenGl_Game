using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.UI;

public class Canvas : ShaderProgram
{
    public Canvas(EngineObject reticule) : base(
        [
            new Shader(@"UiShaders\uiShader.vert", ShaderType.VertexShader),
            new Shader(@"UiShaders\uiShader.frag", ShaderType.FragmentShader)
        ], [reticule], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)]
    )
    {
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("viewport", (Vector2i)param[0]);
    }

    public override void Draw(params object[] param)
    {
        GL.Disable(EnableCap.CullFace);
        
        BindAll();
        SetUniforms(param[0]);
        
        var offset = 0;
        foreach (var engineObject in Objects.Where(o => o.IsVisible))
        {
            SetUniform("color", engineObject.Material.Color);
            SetUniform("radius", new Vector3(0.5f, 0.5f, 1f));
            SetUniform("model", Matrix4.CreateScale(engineObject.Transform.Scale) * Matrix4.CreateTranslation(engineObject.Transform.Position));
            
            //if (engineObject.IsVisible) engineObject.Draw(offset);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            offset += engineObject.MeshData.Indices.Length * sizeof(uint);
        }
        
        UnbindAll();
        
        GL.Enable(EnableCap.CullFace);
    }

    public override void DeleteAll()
    {
        Delete();
    }
}