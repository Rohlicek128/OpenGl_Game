using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Game.Screens.Navigation;

public class AimPage : ScreenPage
{
    public Framebuffer SceneFramebuffer { get; set; }
    public Camera AimCamera { get; set; }

    public unsafe AimPage(Vector2i screenResolution, int screenObjectId) : base(screenResolution, screenObjectId)
    {
        SceneFramebuffer = new Framebuffer();
        SceneFramebuffer.AttachTexture(new Texture(0, screenResolution, null, minFilter: TextureMinFilter.Nearest, magFilter: TextureMagFilter.Nearest), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
        
        AimCamera = new Camera(new Vector3(-2.75f, -4f, 0f), 0f, 0f, 70f, 0.1f, 100_000f);
        AimCamera.SetPitchYaw(-90f, 180f);
        
        UiGraphics.Elements.Add("image", new UiRectangle(new Vector3(0f), SceneFramebuffer.AttachedTextures[0], 1.85f, 1.85f));
        UiGraphics.Elements.Add("bUp", new UiButton(new Vector3(0.73f, -0.65f, 0f), Vector3.UnitX, 0.25f, 0.1f));
        UiGraphics.Elements.Add("bDown", new UiButton(new Vector3(0.73f, -0.8f, 0f), Vector3.UnitX, 0.25f, 0.1f));
        
        UiGraphics.Elements.Add("reticuleX", new UiRectangle(new Vector3(0f), new Vector3(0.5f), 0.05f, 0.005f));
        UiGraphics.Elements.Add("reticuleY", new UiRectangle(new Vector3(0f), new Vector3(0.5f), 0.005f, 0.4f));
        
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public override void RenderScreen(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.04f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var button = (UiButton)UiGraphics.Elements["bUp"];
        if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
        {
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color.X = 1f;
                
            if (mouse.IsDown && mouse.DownButton == MouseButton.Left) AimCamera.SetPitchYaw(MathF.Min(-25f, AimCamera.Pitch + 35f * deltaTime), 180f);
        }
        else button.EngineObject.Material.Color.X = 0.25f;
            
        button = (UiButton)UiGraphics.Elements["bDown"];
        if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
        {
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color.X = 1f;
                
            if (mouse.IsDown && mouse.DownButton == MouseButton.Left) AimCamera.SetPitchYaw(AimCamera.Pitch - 35f * deltaTime, 180f);
        }
        else button.EngineObject.Material.Color.X = 0.25f;

        var cursor = (UiRectangle)UiGraphics.Elements["cursor"];
        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            cursor.EngineObject.IsVisible = true;
            cursor.EngineObject.Transform.Position.X = collision.LookingAtUv.X * 2f - 1f + cursor.EngineObject.Transform.Scale.X / 2f;
            cursor.EngineObject.Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f - cursor.EngineObject.Transform.Scale.Y / 2f;
        }
        else cursor.EngineObject.IsVisible = false;

        UiGraphics.Elements["reticuleX"].GetEngineObject().Transform.Position.Y = -MathF.Cos(MathHelper.DegreesToRadians(AimCamera.Pitch)) * 1.5f;
        UiGraphics.Elements["reticuleY"].GetEngineObject().Transform.Position.Y = -MathF.Cos(MathHelper.DegreesToRadians(AimCamera.Pitch)) * 1.5f + 0.15f;
            
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
            
        fonts["Pixel"].DrawText(MathF.Round(-AimCamera.Pitch) + "deg", new Vector2(25f, ScreenResolution.Y - 45f), 0.4f, new Vector4(1f), ScreenResolution);
        
        if (Station.BatteryPercentage <= 0f) fonts["Pixel"].DrawText("NO POWER", new Vector2(ScreenResolution.X / 2f - 116f, ScreenResolution.Y / 2f - 15f), 0.7f, new Vector4(1f, 0f, 0f, 0.4f), ScreenResolution);
    }
}