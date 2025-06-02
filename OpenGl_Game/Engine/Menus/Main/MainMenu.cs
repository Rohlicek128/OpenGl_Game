using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Engine.Menus.Main;

public class MainMenu : Menu
{
    public Camera Camera { get; set; }
    
    public MainMenu(GameWindow window)
    {
        Camera = new Camera(new Vector3(-1094f, 140f, 59f), 0f, 0f, 75f, 0.1f, 1_000_000f)
        {
            Front = new Vector3(-0.186f, -0.826f, -0.534f),
            Pitch = -55.484f,
            Yaw = 250.916f
        };
        Pages.Add(new MainPage(window, this, Camera));
    }
}

public class MainPage : MenuPage
{
    private MainMenu _menu;
    private GameWindow _window;

    private Vector2 _origoPitchYaw;
    private Camera _camera;
    
    public MainPage(GameWindow window, MainMenu menu, Camera camera)
    {
        _window = window;
        _menu = menu;
        _camera = camera;
        _origoPitchYaw = new Vector2(_camera.Yaw, _camera.Pitch);
        
        UiGraphics.Elements.Add("quit", new UiButton(new Vector3(0f, -0.5f, 0f), new Vector4(Light.NormalizeRgb(19, 22, 28), 0.8f), 0.4f, 0.15f));
        UiGraphics.Elements.Add("play", new UiButton(new Vector3(0f, -0.3f, 0f), new Vector4(Light.NormalizeRgb(19, 22, 28), 0.8f), 0.4f, 0.15f));
        UiGraphics.InitProgram();
    }
    
    public override void RenderPage(Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var screenPosNorm = mouse.ScreenPosition / viewport.ToVector2() * 2f - Vector2.One;
        var moveEffect = screenPosNorm / 100f;
        _camera.SetPitchYaw(_origoPitchYaw.Y + screenPosNorm.Y, _origoPitchYaw.X + screenPosNorm.X);
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        //PLAY
        var button = (UiButton)UiGraphics.Elements["play"];
        if (button.PointCollision(screenPosNorm))
        {
            fonts["Brigends"].DrawText("PLAY", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.5f, new Vector4(0f, 0f, 0f, 1f), viewport);
            
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color = new Vector4(1f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.6f;
            button.UiShape.EngineObject.Transform.Position.X = -0.6f + 0.08f + moveEffect.X;
            if (ButtonHandler.TimerManager.CheckTimer("play", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
            {
                _menu.IsVisible = false;
            }
        }
        else
        {
            button.EngineObject.Material.Color = new Vector4(1f, 1f, 1f, 0.5f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.4f;
            button.UiShape.EngineObject.Transform.Position.X = -0.6f + 0f + moveEffect.X;
            fonts["Brigends"].DrawText("PLAY", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.5f, new Vector4(1f), viewport);
        }
        button.UiShape.EngineObject.Transform.Position.Y = 0.5f;
        
        //QUIT
        button = (UiButton)UiGraphics.Elements["quit"];
        if (button.PointCollision(screenPosNorm))
        {
            fonts["Brigends"].DrawText("QUIT", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.5f, new Vector4(0f, 0f, 0f, 1f), viewport);
            
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color = new Vector4(1f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.6f;
            button.UiShape.EngineObject.Transform.Position.X = -0.6f + 0.08f + moveEffect.X;
            if (ButtonHandler.TimerManager.CheckTimer("quit", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
            {
                _window.Close();
            }
        }
        else
        {
            button.EngineObject.Material.Color = new Vector4(1f, 1f, 1f, 0.5f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.4f;
            button.UiShape.EngineObject.Transform.Position.X = -0.6f + 0f + moveEffect.X;
            fonts["Brigends"].DrawText("QUIT", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.5f, new Vector4(1f), viewport);
        }
        button.UiShape.EngineObject.Transform.Position.Y = 0.3f;
        
        fonts["Brigends"].DrawText("FROM ORBIT", new Vector2(viewport.X * (0.08f + moveEffect.X), viewport.Y * 0.835f), 1f, new Vector4(1f), viewport);
        fonts["ATName"].DrawText("By Adam Svec", new Vector2(800f + moveEffect.X * viewport.X, viewport.Y * 0.8f), 0.5f, new Vector4(1f), viewport);
    }
}