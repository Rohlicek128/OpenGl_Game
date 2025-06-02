using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Gauges.Turn;

public class TurnGauge : ScreenHandler
{
    public static float MaxTurn { get; set; }
    public TurnGauge(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Turn Gauge",
            new Transform(new Vector3(-0.94140166f, 0.688166f, -0.4747811f), new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
                new Vector3(0.175f, 0.05f, 0.085f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
            })
        );
        IsTurnOn = true;
        
        Pages.Add(new TurnPage(screenResolution / EngineObject.Transform.Scale.Zx.Normalized(), EngineObject.Id));
    }
}

public class TurnPage : ScreenPage
{
    public float TurnDegrees { get; set; }
    private Vector2 _normRes;
    public TurnPage(Vector2 screenResolution, int screenObjectId) : base(
        new Vector2i((int)screenResolution.X, (int)screenResolution.Y), screenObjectId)
    {
        _normRes = screenResolution;

        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector4(0.95f, 0.95f, 0.95f, 1f), 1.80f, 1.80f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.1f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        fonts["Pixel"].DrawText((TurnDegrees < 0 ? "<" : "") + MathF.Abs(MathF.Round(TurnDegrees * 10f) / 10f) + (TurnDegrees > 0 ? ">" : ""),
            new Vector2(_normRes.X / 2f - (TurnDegrees < 0 ? 100f : 40f), _normRes.Y / 2f - 51f), 2.25f, new Vector4(0f, 0f, 0f, 1f), _normRes);
    }
}