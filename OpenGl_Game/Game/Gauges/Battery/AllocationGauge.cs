using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons.LaserParams;
using OpenGl_Game.Game.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Gauges.Battery;

public class AllocationGauge : ScreenHandler
{
    public static float AllocationSpeed { get; set; }
    public AllocationGauge(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Allocation Gauge",
            new Transform(new Vector3(-1.5095744f, 1.0695179f, -0.03465618f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-40f), -MathF.PI/2f),
                new Vector3(0.525f, 0.05f, 0.15f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]}
            })
        );
        IsTurnOn = true;
        
        Pages.Add(new AllocationPage(screenResolution / EngineObject.Transform.Scale.Zx.Normalized(), EngineObject.Id));
    }

    public static void AllocateBattery(float deltaTime, float speed = 0f, AllocateButton? button = null)
    {
        var ratio = Station.AllocatedMax / Station.BatteryMax;
        Station.BatteryPercentage -= deltaTime * (speed != 0f ? speed : 1f / (AllocationSpeed * 40f));
        Station.AllocationPercentage += deltaTime * (speed != 0f ? speed : 1f / (AllocationSpeed * 40f)) * (1 / ratio);

        if (Station.AllocationPercentage >= 1f || Station.BatteryPercentage <= 0f)
        {
            AllocateButton.IsAllocating = false;
            if (button != null)
            {
                button.EngineObject.Material.Color = new Vector4(0f, 0f, 0f, 1f);
                button.EngineObject.Name = "Start Allocating";
                button.ButtonValue = 0f;
            }
        }
    }
}

public class AllocationPage : ScreenPage
{
    private Vector2 _normRes;
    public AllocationPage(Vector2 screenResolution, int screenObjectId) : base(new Vector2i((int)screenResolution.X, (int)screenResolution.Y), screenObjectId)
    {
        _normRes = screenResolution;
        
        UiGraphics.Elements.Add("Bg", new UiRectangle(new Vector3(0f), new Vector4(0.95f, 0.95f, 0.95f, 1f), 1.90f, 1.80f));
        
        UiGraphics.Elements.Add("Bar Bg", new UiRectangle(new Vector3(0f, 0.2f ,0f), new Vector4(0.1f, 0.05f, 0.08f, 1f), 1.8f, 1f));
        UiGraphics.Elements.Add("Bar", new UiRectangle(new Vector3(0f, 0.2f ,0f), new Vector4(1f, 0.1f, 0.19f, 1f), 1.8f, 1f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.1f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Position.Y = 0.2f;
        UiGraphics.Elements["Bar"].GetEngineObject().Transform.Position.Y = 0.2f;
        
        UiGraphics.Elements["Bar"].GetEngineObject().Transform.Scale.X = UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Scale.X * Station.AllocationPercentage;
        UiGraphics.Elements["Bar"].GetEngineObject().Transform.Position.X =
            (UiGraphics.Elements["Bar"].GetEngineObject().Transform.Scale.X - UiGraphics.Elements["Bar Bg"].GetEngineObject().Transform.Scale.X) / 2f;
            
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());

        if (Station.AllocationPercentage < 0f) Station.AllocationPercentage = 0f;
        fonts["Pixel"].DrawText("ALLOCATED BATTERY: " + MathF.Round(Station.AllocationPercentage * Station.AllocatedMax * 100f) / 100f + "/" + Station.AllocatedMax + " TWh",
            new Vector2(100f, 57f), 1f, new Vector4(0f, 0f, 0f, 1f), _normRes);
    }
}