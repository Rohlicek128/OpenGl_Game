using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Targets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Objective;

public class ObjectivePage : ScreenPage
{
    private CityTargets _cities;
    public City CurrentTarget { get; set; }
    
    public ObjectivePage(Vector2i screenResolution, int screenObjectId) : base(screenResolution, screenObjectId)
    {
        _cities = CityTargets.Instance;
        CurrentTarget = _cities.CitiesWithPop(1_000_000)[Random.Shared.Next(_cities.CitiesWithPop(1_000_000).Count)];
        
        UiGraphics.Elements.Add("b1", new UiButton(new Vector3(0.8f, 0.8f, 0f), Vector3.UnitX, 0.25f, 0.1f));
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public override void RenderScreen(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var bg = 0.04f;
        GL.ClearColor(bg, bg, bg, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (((UiButton)UiGraphics.Elements["b1"]).PointCollision(collision.LookingAtUv * 2f - Vector2.One))
        {
            ((UiButton)UiGraphics.Elements["b1"]).Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            ((UiButton)UiGraphics.Elements["b1"]).EngineObject.Material.Color.X = 1f;
            
            if (mouse.IsDown && mouse.DownButton == MouseButton.Left) CurrentTarget = _cities.CitiesWithPop(10_000_000)[Random.Shared.Next(_cities.CitiesWithPop(10_000_000).Count)];
        }
        else
        {
            ((UiButton)UiGraphics.Elements["b1"]).EngineObject.Material.Color.X = 0.25f;
        }
        
        fonts["Brigends"].DrawText("OBJECTIVE", new Vector2(25f, ScreenResolution.Y - 60f), 0.75f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("ORBITAL LASER", new Vector2(25f, ScreenResolution.Y - 80f), 0.35f, new Vector4(1f), ScreenResolution);
        //fonts["Pixel"].DrawText("SPEED: " + boost + "x", new Vector2(25f, ScreenResolution.Y - 150f), 0.8f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("TARGET: " + CurrentTarget.Name + ", " + CurrentTarget.Country.Name + " [" + CurrentTarget.Coordinates + "], (" +  CurrentTarget.Population + ")", new Vector2(25f, ScreenResolution.Y - 110f), 0.30f, new Vector4(1f), ScreenResolution);
        
        var cursor = (UiRectangle)UiGraphics.Elements["cursor"];
        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            cursor.EngineObject.IsVisible = true;
            cursor.EngineObject.Transform.Position.X = collision.LookingAtUv.X * 2f - 1f + cursor.EngineObject.Transform.Scale.X / 2f;
            cursor.EngineObject.Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f - cursor.EngineObject.Transform.Scale.Y / 2f;
        }
        else cursor.EngineObject.IsVisible = false;
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
    }
}