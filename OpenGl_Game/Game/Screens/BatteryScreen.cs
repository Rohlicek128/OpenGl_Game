using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Screens;

public class BatteryScreen : ScreenHandler
{
    public BatteryScreen(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Battery Gauge",
            new Transform(new Vector3(-2.5346785f, -0.14945818f, 1f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
                new Vector3(0.2f, 0.05f, 0.6f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
                //{TextureTypes.Emissive, new Texture("white1x1.png", 4)}
            })
        );
        IsTurnOn = true;
        
        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector3(0.95f), 1.80f, 1.90f));
        UiGraphics.Elements.Add("Bar Bg", new UiRectangle(new Vector3(0.4f, 0f ,0f), new Vector3(0.1f, 0.05f, 0.08f), 0.8f, 1.75f));
        UiGraphics.Elements.Add("Bar", new UiRectangle(new Vector3(0.4f, 0f ,0f), new Vector3(1f, 0.1f, 0.19f), 0.8f, 1.75f));
        UiGraphics.InitProgram();
    }
    
    public override void RenderScreen(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts)
    {
        GL.Viewport(0, 0, ScreenResolution.X, ScreenResolution.Y);
        Framebuffer.Bind();

        if (IsTurnOn)
        {
            var bg = 0.1f;
            GL.ClearColor(bg, bg, bg, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            UiGraphics.Elements["Bar"].GetEngineObject().Transform.Scale.Y = UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Scale.Y * Station.BatteryPercentage;
            UiGraphics.Elements["Bar"].GetEngineObject().Transform.Position.Y =
                (UiGraphics.Elements["Bar"].GetEngineObject().Transform.Scale.Y - UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Scale.Y) / 2f;
            
            UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());

            var resNorm = ScreenResolution / EngineObject.Transform.Scale.Zx.Normalized();
            fonts["Pixel"].DrawText("0 W", new Vector2(150f, 100f), 1f, new Vector4(0f, 0f, 0f, 1f), resNorm);
            fonts["Pixel"].DrawText("50 PW", new Vector2(70f, resNorm.Y / 2f - 25f), 1f, new Vector4(0f, 0f, 0f, 1f), resNorm);
            fonts["Pixel"].DrawText("100 PW", new Vector2(55f, resNorm.Y - 150f), 1f, new Vector4(0f, 0f, 0f, 1f), resNorm);
        }
        else
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        
        Framebuffer.Unbind();
        GL.Viewport(0, 0, viewport.X, viewport.Y);
    }
}