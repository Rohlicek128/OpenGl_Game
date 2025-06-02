using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Objective;

public class ManualPage : ScreenPage
{
    
    public ManualPage(Vector2i screenResolution, int screenObjectId) : base(screenResolution, screenObjectId)
    {
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        GL.ClearColor(ScreenHandler.LcdBlack.X, ScreenHandler.LcdBlack.Y, ScreenHandler.LcdBlack.Z, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        fonts["Brigends"].DrawText("MANUAL", new Vector2(25f, ScreenResolution.Y - 60f), 0.5f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("TARGET", new Vector2(25f, ScreenResolution.Y - 110f), 0.30f, new Vector4(1f), ScreenResolution);
        
        var cursor = (UiRectangle)UiGraphics.Elements["cursor"];
        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            cursor.EngineObject.IsVisible = true;
            cursor.EngineObject.Transform.Position.X = collision.LookingAtUv.X * 2f - 1f + cursor.EngineObject.Transform.Scale.X / 2f;
            cursor.EngineObject.Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f - cursor.EngineObject.Transform.Scale.Y / 2f;
        }
        else cursor.EngineObject.IsVisible = false;
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
    }
}