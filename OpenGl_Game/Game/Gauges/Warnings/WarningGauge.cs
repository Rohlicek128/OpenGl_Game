using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Gauges.Battery;
using OpenGl_Game.Game.Gauges.Speed;
using OpenGl_Game.Game.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Gauges.Warnings;

public class WarningGauge : ScreenHandler
{
    public WarningGauge(Vector2i screenResolution, SpeedPage speedPage) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Warning Gauge",
            new Transform(new Vector3(-1.1236829f, 0.60921943f, -0.13007098f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-40f), -MathF.PI/2f),
                new Vector3(0.23f, 0.05f, 0.04f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
            })
        );
        IsTurnOn = true;
        
        Pages.Add(new WarningPage(screenResolution / EngineObject.Transform.Scale.Zx.Normalized(), EngineObject.Id, EngineObject.Transform, speedPage));
    }
}

public class WarningPage : ScreenPage
{
    public static bool Warnings { get; set; }
    public Light[] _warningLights { get; set; }
    private SpeedPage _speedPage;
    public float LimitSpeed { get; set; }
    
    private Vector2 _normRes;
    public WarningPage(Vector2 screenResolution, int screenObjectId, Transform transform, SpeedPage speedPage) : base(new Vector2i((int)screenResolution.X, (int)screenResolution.Y), screenObjectId)
    {
        _warningLights = [
            new Light(
                "Overspeed light", 
                new Transform(
                    transform.Position - new Vector3(-0.020669818f, -0.020395279f, -0.07196577f),
                    transform.Rotation,
                    new Vector3(0.015f, 0.005f, 0.01f)),
                MeshConstructor.CreateCube(),
                new Material(new Vector3(1f, 0.902f, 0.118f)),
                new Vector3(0.25f, 1f, 1f),
                new Vector3(1.0f, 10f, 50f),
                LightTypes.Point
            ),
            new Light(
                "Low power light", 
                new Transform(
                    transform.Position - new Vector3(-0.020669818f, -0.020395279f, 0.0028055012f),
                    transform.Rotation,
                    new Vector3(0.015f, 0.005f, 0.01f)),
                MeshConstructor.CreateCube(),
                new Material(new Vector3(1f, 0.902f, 0.118f)),
                new Vector3(0.25f, 1f, 1f),
                new Vector3(1.0f, 10f, 50f),
                LightTypes.Point
            )
        ];
        _normRes = screenResolution;
        _speedPage = speedPage;
        LimitSpeed = 30f;
        
        //Console.WriteLine(transform.Position - new Vector3(-1.103013f, 0.6296147f, -0.13287649f));
        
        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector4(0.95f, 0.95f, 0.95f, 1f), 1.90f, 1.80f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.1f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        _warningLights[0].IsLighting = MathF.Floor(_speedPage.ActualSpeed) > LimitSpeed && PrimeButton.IsPrimed;
        if (_warningLights[0].IsLighting) _warningLights[0].Material.Color = new Vector4(1f, 0.902f, 0.118f, 1f);
        else _warningLights[0].Material.Color = new Vector4(0.45f);
        
        _warningLights[1].IsLighting = Station.AllocationPercentage * Station.AllocatedMax < (MathF.Pow(Station.LaserRadius, 0.5f) / 3f * 35f) * 0.7f && PrimeButton.IsPrimed && !LaserButton.IsShooting;
        if (_warningLights[1].IsLighting) _warningLights[1].Material.Color = new Vector4(1f, 0.902f, 0.118f, 1f);
        else _warningLights[1].Material.Color = new Vector4(0.45f);
        
        fonts["Pixel"].DrawText("OVERSPEED", new Vector2(_normRes.X / 6f - 130f, 35f), 1.15f, new Vector4(0f, 0f, 0f, 1f), _normRes);
        fonts["Pixel"].DrawText("LOW POWER", new Vector2(_normRes.X / 2f - 165f, 35f), 1.15f, new Vector4(0f, 0f, 0f, 1f), _normRes);

        if (!LaserButton.IsShooting) Warnings = _warningLights.Any(l => l.IsLighting);
    }
}