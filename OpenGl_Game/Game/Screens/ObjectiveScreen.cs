using System.Drawing;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Targets;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens;

public class ObjectiveScreen : ScreenHandler
{
    public CityTargets Cities { get; set; }
    public City CurrentCity { get; set; }
    
    public ObjectiveScreen(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Objective Screen",
            new Transform(new Vector3(-2.5346785f, -0.14945818f, -0.8248692f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
                new Vector3(0.6f, 0.05f, 0.6f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]},
                {TextureTypes.Emissive, new Texture("white1x1.png", 4)}
            })
        );
        Cities = new CityTargets();
        CurrentCity = Cities.CitiesWithPop(1_000_000)[Random.Shared.Next(Cities.CitiesWithPop(1_000_000).Count)];
        
        UiGraphics.Elements.Add("b1", new UiButton(new Vector3(0.8f, 0.8f, 0f), Vector3.UnitX, 0.25f, 0.1f));
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

            if (((UiButton)UiGraphics.Elements["b1"]).PointCollision(collision.LookingAtUv * 2f - Vector2.One))
            {
                ((UiButton)UiGraphics.Elements["b1"]).Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
                ((UiButton)UiGraphics.Elements["b1"]).EngineObject.Material.Color.X = 1f;
                
                if (mouse.IsDown && mouse.DownButton == MouseButton.Left) CurrentCity = Cities.CitiesWithPop(10_000_000)[Random.Shared.Next(Cities.CitiesWithPop(10_000_000).Count)];
            }
            else
            {
                ((UiButton)UiGraphics.Elements["b1"]).EngineObject.Material.Color.X = 0.25f;
            }
            
            UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
            
            fonts["Brigends"].DrawText("STATION 42", new Vector2(25f, ScreenResolution.Y - 60f), 0.75f, new Vector4(1f), ScreenResolution);
            fonts["Pixel"].DrawText("ORBITAL LASER", new Vector2(25f, ScreenResolution.Y - 80f), 0.35f, new Vector4(1f), ScreenResolution);
            //fonts["Pixel"].DrawText("SPEED: " + boost + "x", new Vector2(25f, ScreenResolution.Y - 150f), 0.8f, new Vector4(1f), ScreenResolution);
            fonts["Pixel"].DrawText("TARGET: " + CurrentCity.Name + ", " + CurrentCity.Country.Name + " [" + CurrentCity.Coordinates + "], (" +  CurrentCity.Population + ")", new Vector2(25f, ScreenResolution.Y - 110f), 0.30f, new Vector4(1f), ScreenResolution);
            
            if (collision.LookingAtObject.Id == EngineObject.Id)
            {
                fonts["Pixel"].DrawText("o", collision.LookingAtUv * ScreenResolution, 0.3f, new Vector4(1f), ScreenResolution);
                //UiGraphics.Elements[0].GetEngineObject().Transform.Position.X = collision.LookingAtUv.X * 2f - 1f;
                //UiGraphics.Elements[0].GetEngineObject().Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f;
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