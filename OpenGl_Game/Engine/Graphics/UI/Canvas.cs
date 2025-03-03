using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.UI;

public class Canvas
{
    private ShaderProgram _program;

    public Canvas()
    {
        var reticule = EngineObject.CreateEmpty();
        reticule.MeshData = MeshConstructor.CreateQuad();
        reticule.Transform.Scale *= 0.005f;
        
        _program = new ShaderProgram([
            new Shader(@"UiShaders\uiShader.vert", ShaderType.VertexShader),
            new Shader(@"UiShaders\uiShader.frag", ShaderType.FragmentShader)
        ], [reticule], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)]);
    }

    public void DrawCanvas(Vector2i viewport)
    {
        GL.Disable(EnableCap.CullFace);
        
        _program.Use();
        _program.ArrayBuffer.Bind();
        _program.IndexBuffer.Bind();
        
        _program.SetUniform("viewport", viewport);
        
        var offset = 0;
        foreach (var engineObject in _program.Objects.Where(o => o.IsVisible))
        {
            _program.SetUniform("color", new Vector4(engineObject.Material.Color, 0.75f));
            _program.SetUniform("radius", new Vector3(0.5f, 0.5f, 1f));
            _program.SetUniform("model", Matrix4.CreateScale(engineObject.Transform.Scale) * Matrix4.CreateTranslation(engineObject.Transform.Position));
            
            //if (engineObject.IsVisible) engineObject.Draw(offset);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            offset += engineObject.MeshData.Indices.Length * sizeof(uint);
        }
        
        _program.Unbind();
        _program.ArrayBuffer.Unbind();
        _program.IndexBuffer.Unbind();
        
        GL.Enable(EnableCap.CullFace);
    }
}