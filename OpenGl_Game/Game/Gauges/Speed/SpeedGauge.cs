using System.Globalization;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Gauges.Turn;
using OpenGl_Game.Game.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Gauges.Speed;

public class SpeedGauge : ScreenHandler
{
    public SpeedGauge(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Speed Gauge",
            new Transform(new Vector3(-0.62280214f, 0.688166f, -0.62581855f), new Vector3(-1.9865768f, -0.19524574f, 0.4241766f),
                new Vector3(0.21f, 0.05f, 0.085f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
            })
        );
        IsTurnOn = true;
        
        Pages.Add(new SpeedPage(screenResolution / EngineObject.Transform.Scale.Zx.Normalized(), EngineObject.Id));
    }
}

public class SpeedPage : ScreenPage
{
    public float TargetSpeed { get; set; }
    public float ActualSpeed { get; set; }
    public float ChangeSpeed { get; set; }
    
    public float MaxSpeed { get; set; }
    public float MinSpeed { get; set; }
    private Vector2 _normRes;
    public SpeedPage(Vector2 screenResolution, int screenObjectId) : base(new Vector2i((int)screenResolution.X, (int)screenResolution.Y), screenObjectId)
    {
        _normRes = screenResolution;
        TargetSpeed = 50f;
        ActualSpeed = 180f;
        ChangeSpeed = 0.5f;

        MaxSpeed = 180f;
        MinSpeed = 25f;

        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector3(0.95f), 1.70f, 1.80f));
        UiGraphics.Elements.Add("Line", new UiRectangle(new Vector3(0f, 0.18f, 0f), new Vector3(0f), 0.03f, 0.9f));
        UiGraphics.InitProgram();
    }

    public override void RenderScreen(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.1f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        TargetSpeed = MathF.Max(MinSpeed, MathF.Min(MaxSpeed, TargetSpeed));
        ChangeSpeed = 0.35f;
        if (ActualSpeed < TargetSpeed) ActualSpeed += MathF.Max(1f, TargetSpeed - ActualSpeed) * deltaTime * ChangeSpeed;
        else if (ActualSpeed > TargetSpeed) ActualSpeed -= MathF.Max(1f, ActualSpeed - TargetSpeed) * deltaTime * ChangeSpeed;

        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());

        var red = MathF.Max(0f, (TargetSpeed - 100f) / (MaxSpeed - 100f));
        fonts["Pixel"].DrawText((int)Math.Floor(TargetSpeed) + "", new Vector2(_normRes.X / 2f - 160f, _normRes.Y / 2f - 10f), 1.2f, new Vector4(red, 0f, 0f, 1f), _normRes);
        red = MathF.Max(0f, (ActualSpeed - 100f) / (MaxSpeed - 100f));
        fonts["Pixel"].DrawText((int)Math.Floor(ActualSpeed) + "", new Vector2(_normRes.X / 2f + 28f, _normRes.Y / 2f - 10f), 1.2f, new Vector4(red, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText("km/s", new Vector2(_normRes.X / 2f - 75f, _normRes.Y / 2f - 65f), 0.8f, new Vector4(0f, 0f, 0f, 1f), _normRes);
    }
}