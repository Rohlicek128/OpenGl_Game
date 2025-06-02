using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Gauges.Battery;

public class LaserParamsGauge : ScreenHandler
{
    public LaserParamsGauge(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Laser Params Gauge",
            new Transform(new Vector3(-1.3824716f, 0.92495096f, 0.07097028f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-40f), -MathF.PI/2f),
                new Vector3(0.3f, 0.05f, 0.08f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
            })
        );
        IsTurnOn = true;
        
        Pages.Add(new LaserParamsPage(screenResolution / EngineObject.Transform.Scale.Zx.Normalized(), EngineObject.Id));
    }
}

public class LaserParamsPage : ScreenPage
{
    private Vector2 _normRes;
    public LaserParamsPage(Vector2 screenResolution, int screenObjectId) : base(new Vector2i((int)screenResolution.X, (int)screenResolution.Y), screenObjectId)
    {
        _normRes = screenResolution;
        
        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector4(0.95f, 0.95f, 0.95f, 1f), 1.90f, 1.80f));
        UiGraphics.Elements.Add("Line", new UiRectangle(new Vector3(0f, 0.18f, 0f), new Vector4(0f, 0f, 0f, 1f), 0.03f, 0.9f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.1f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        UiGraphics.Elements["Line"].GetEngineObject().Transform.Position = new Vector3(-0.375f, 0f, 0f);
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        fonts["Pixel"].DrawText((int)Station.AllocatedMax + "", new Vector2(125f, 165f), 3.5f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText("TWh", new Vector2(125f, 60f), 1.5f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        
        fonts["Pixel"].DrawText((MathF.Round(Station.LaserRadius * 10f) / 10f).ToString("F1"), new Vector2(555f, 165f), 3.5f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText("km", new Vector2(600f, 60f), 1.5f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        
        fonts["Pixel"].DrawText("=>", new Vector2(950f, 200f), 2f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText(MathF.Round(MathF.Pow(Station.LaserRadius, 0.5f) / 3f * 35f) + "", new Vector2(1150f, 165f), 3.5f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText("TWh", new Vector2(1150f, 60f), 1.5f, new Vector4(0f, 0f, 0f, 1f), _normRes);
    }
}