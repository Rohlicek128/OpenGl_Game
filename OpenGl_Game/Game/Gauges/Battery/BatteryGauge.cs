using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Gauges.Battery;

public class BatteryGauge : ScreenHandler
{
    public BatteryGauge(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Battery Gauge",
            new Transform(new Vector3(-1.2868996f, 0.77904314f, 0.32686314f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-40f), -MathF.PI/2f),
                new Vector3(0.125f, 0.05f, 0.375f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
                //{TextureTypes.Emissive, new Texture("white1x1.png", 4)}
            })
        );
        IsTurnOn = true;
        
        Pages.Add(new BatteryPage(screenResolution / EngineObject.Transform.Scale.Zx.Normalized(), EngineObject.Id));
    }
}

public class BatteryPage : ScreenPage
{
    private Vector2 _normRes;
    public BatteryPage(Vector2 screenResolution, int screenObjectId) : base(new Vector2i((int)screenResolution.X, (int)screenResolution.Y), screenObjectId)
    {
        _normRes = screenResolution;
        
        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector4(0.95f, 0.95f, 0.95f, 1f), 1.80f, 1.90f));
        UiGraphics.Elements.Add("Bar Bg", new UiRectangle(new Vector3(0.4f, 0f ,0f), new Vector4(0.1f, 0.05f, 0.08f, 1f), 0.8f, 1.75f));
        UiGraphics.Elements.Add("Bar", new UiRectangle(new Vector3(0.4f, 0f ,0f), new Vector4(1f, 0.1f, 0.19f, 1f), 0.8f, 1.75f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.1f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
        UiGraphics.Elements["Bar"].GetEngineObject().Transform.Scale.Y = UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Scale.Y * Station.BatteryPercentage;
        UiGraphics.Elements["Bar"].GetEngineObject().Transform.Position.Y =
            (UiGraphics.Elements["Bar"].GetEngineObject().Transform.Scale.Y - UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Scale.Y) / 2f;
            
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        fonts["Pixel"].DrawText("0 W", new Vector2(85f, 100f), 0.7f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText((int)(Station.BatteryMax / 2f) + " TWh", new Vector2(30f, _normRes.Y / 2f - 25f), 0.7f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText((int)Station.BatteryMax + " TWh", new Vector2(30f, _normRes.Y - 150f), 0.7f, new Vector4(0f, 0f, 0f, 1f), _normRes);
    }
}