using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.UI.Obsolete;

public class WindowManager
{
    public List<UiWindow> Windows;
    public ShaderProgram UiProgram;

    public WindowManager()
    {
        Windows = [];
        
        var ui = EngineObject.CreateEmpty();
        ui.MeshData = MeshConstructor.CreateQuad();
        //UiProgram = new ShaderProgram([
        //    new Shader(@"UiShaders\uiShader.vert", ShaderType.VertexShader),
        //    new Shader(@"UiShaders\uiShader.frag", ShaderType.FragmentShader)
        //], [ui], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)]);
        
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