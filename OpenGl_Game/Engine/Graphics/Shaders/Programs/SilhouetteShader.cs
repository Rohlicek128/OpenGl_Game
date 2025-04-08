using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class SilhouetteShader : ShaderProgram
{
    public SilhouetteShader(GeometryShader geometryShader) : base
    (
        [
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"outlineShaders\silhouette.frag", ShaderType.FragmentShader)
        ], geometryShader
    )
    {
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("world", (Matrix4)param[0]);
        SetUniform("view", (Matrix4)param[1]);
    }

    public override void Draw(params object[] param)
    {
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        BindAll();
        SetUniforms(param[0], param[1]);

        var selectedId = (int)param[2];
        var view = (Matrix4)param[1];
        var offset = 0;
        foreach (var engineObject in Objects)
        {
            if (engineObject.IsVisible)
            {
                SetUniform("isSelected", engineObject.IsSelectable && engineObject.Id == selectedId ? 1 : 0);
                engineObject.DrawObject(this, offset, view);
            }
            offset += engineObject.MeshData.Indices.Length * sizeof(uint);
        }
        
        UnbindAll();
    }

    public override void DeleteAll()
    {
        Delete();
    }
}