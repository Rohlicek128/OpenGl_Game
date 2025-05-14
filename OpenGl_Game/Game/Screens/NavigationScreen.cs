using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Targets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Game.Screens;

public class NavigationScreen : ScreenHandler
{
    public Framebuffer SceneFramebuffer { get; set; }
    public Camera AimCamera { get; set; }
    
    public unsafe NavigationScreen(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Navigation Screen",
            new Transform(new Vector3(-2.5346785f, -0.14945818f, -1.8248692f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
                new Vector3(0.6f, 0.05f, 0.6f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]},
                {TextureTypes.Emissive, new Texture("white1x1.png", 4)}
            })
        );
        SceneFramebuffer = new Framebuffer();
        SceneFramebuffer.AttachTexture(new Texture(0, ScreenResolution, null, minFilter: TextureMinFilter.Nearest, magFilter: TextureMagFilter.Nearest), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
        
        AimCamera = new Camera(new Vector3(-2.75f, -4f, 0f), 0f, 0f, 75f, 0.1f, 100_000f);
        AimCamera.Front = new Vector3(-0.2f, -1f, 0f);
        
        UiGraphics.Elements.Add("image", new UiRectangle(new Vector3(0f), SceneFramebuffer.AttachedTextures[0], 1.85f, 1.85f));
        UiGraphics.Elements.Add("bUp", new UiButton(new Vector3(0.73f, -0.65f, 0f), Vector3.UnitX, 0.25f, 0.1f));
        UiGraphics.Elements.Add("bDown", new UiButton(new Vector3(0.73f, -0.8f, 0f), Vector3.UnitX, 0.25f, 0.1f));
        UiGraphics.InitProgram();
    }

    public override void RenderScreen(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts)
    {
        GL.Viewport(0, 0, ScreenResolution.X, ScreenResolution.Y);
        Framebuffer.Bind();
        
        if (IsTurnOn)
        {
            var bg = 0.04f;
            GL.ClearColor(bg, bg, bg, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var button = (UiButton)UiGraphics.Elements["bUp"];
            if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
            {
                button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
                button.EngineObject.Material.Color.X = 1f;
                
                if (mouse.IsDown && mouse.DownButton == MouseButton.Left) AimCamera.SetPitchYaw(MathF.Min(-25f, AimCamera.Pitch + 0.25f), 180f);
            }
            else button.EngineObject.Material.Color.X = 0.25f;
            
            button = (UiButton)UiGraphics.Elements["bDown"];
            if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
            {
                button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
                button.EngineObject.Material.Color.X = 1f;
                
                if (mouse.IsDown && mouse.DownButton == MouseButton.Left) AimCamera.SetPitchYaw(AimCamera.Pitch - 0.25f, 180f);
            }
            else button.EngineObject.Material.Color.X = 0.25f;
            
            
            UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
            
            fonts["Pixel"].DrawText(-AimCamera.Pitch + "deg", new Vector2(25f, ScreenResolution.Y - 45f), 0.4f, new Vector4(1f), ScreenResolution);
            
            if (collision.LookingAtObject.Id == EngineObject.Id)
            {
                fonts["Pixel"].DrawText("o", collision.LookingAtUv * ScreenResolution, 0.3f, new Vector4(1f), ScreenResolution);
            }
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