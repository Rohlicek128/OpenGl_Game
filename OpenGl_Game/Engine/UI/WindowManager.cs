using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using VertexAttribType = OpenGl_Game.Engine.Graphics.Buffers.VertexAttribType;

namespace OpenGl_Game.Engine.UI;

public class WindowManager
{
    public List<UiWindow> Windows;
    public ShaderProgram UiProgram;

    public WindowManager()
    {
        Windows = [];
        
        var ui = EngineObject.CreateEmpty();
        //ui.VerticesData.Data = new float[6 * 4];
        ui.MeshData = MeshConstructor.CreateQuad(1f);
        UiProgram = new ShaderProgram([
            new Shader(@"UiShaders\uiShader.vert", ShaderType.VertexShader),
            new Shader(@"UiShaders\uiShader.frag", ShaderType.FragmentShader)
        ], [ui], [new VertexAttribute(VertexAttribType.PosAndTex, 4)]);
        
        Windows.Add(new UiWindow(UiProgram, "Window #1", new Vector4(0.05f, 0.05f, 0.05f, 0.95f)));
    }

    public void DrawWindows(Vector2 viewport, Dictionary<string, FontMap> fonts)
    {
        UiProgram.Use();
        UiProgram.ArrayBuffer.Bind();
        
        UiProgram.SetUniform("viewport", viewport);

        foreach (var window in Windows.Where(window => window.IsVisible))
        {
            window.DrawWindow(UiProgram, viewport, fonts);
        }
        
        UiProgram.ArrayBuffer.Unbind();
        UiProgram.Unbind();
    }

    public void Delete()
    {
        UiProgram.Delete();
    }
}