
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Menus.Main;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Engine.Menus.Pause;

public class PauseMenu : Menu
{
    public PauseMenu(GameWindow window, MainMenu mainMenu)
    {
        Pages.Add(new MainPausePage(window, mainMenu, this));
    }
}

public class MainPausePage : MenuPage
{
    private MainMenu _mainMenu;
    private PauseMenu _pauseMenu;
    private GameWindow _window;
    public MainPausePage(GameWindow window, MainMenu mainMenu, PauseMenu pauseMenu)
    {
        _window = window;
        _pauseMenu = pauseMenu;
        _mainMenu = mainMenu;
        
        UiGraphics.Elements.Add("backbg", new UiButton(new Vector3(0f, 0f, 0f), new Vector4(0f, 0f, 0f, 0.5f), 2f, 2f));
        UiGraphics.Elements.Add("bg", new UiButton(new Vector3(-2f / 3f, 0f, 0f), new Vector4(Light.NormalizeRgb(9, 11, 15), 0.9f), 0.75f, 2f));
        
        UiGraphics.Elements.Add("quitD", new UiButton(new Vector3(0f, 0.16f, 0f), new Vector4(Light.NormalizeRgb(19, 22, 28), 0.8f), 0.4f, 0.125f));
        UiGraphics.Elements.Add("quitM", new UiButton(new Vector3(0f, 0.33f, 0f), new Vector4(Light.NormalizeRgb(19, 22, 28), 0.8f), 0.4f, 0.125f));
        UiGraphics.Elements.Add("resume", new UiButton(new Vector3(0f, 0.5f, 0f), new Vector4(Light.NormalizeRgb(19, 22, 28), 0.8f), 0.4f, 0.125f));
        UiGraphics.InitProgram();
    }
    
    public override void RenderPage(Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var screenPosNorm = mouse.ScreenPosition / viewport.ToVector2() * 2f - Vector2.One;
        var moveEffect = screenPosNorm / 100f;
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        //RESUME
        var button = (UiButton)UiGraphics.Elements["resume"];
        if (button.PointCollision(screenPosNorm))
        {
            fonts["Brigends"].DrawText("RESUME", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.25f, new Vector4(0f, 0f, 0f, 1f), viewport);
            
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color = new Vector4(1f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.6f;
            button.UiShape.EngineObject.Transform.Position.X = -0.65f + 0.1f + moveEffect.X;
            if (ButtonHandler.TimerManager.CheckTimer("resume", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
            {
                _pauseMenu.IsVisible = false;
            }
        }
        else
        {
            button.EngineObject.Material.Color = new Vector4(1f, 1f, 1f, 0.5f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.4f;
            button.UiShape.EngineObject.Transform.Position.X = -0.65f + 0f + moveEffect.X;
            fonts["Brigends"].DrawText("RESUME", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.25f, new Vector4(1f), viewport);
        }
        
        
        //QUIT to Menu
        button = (UiButton)UiGraphics.Elements["quitM"];
        if (button.PointCollision(screenPosNorm))
        {
            fonts["Brigends"].DrawText("QUIT TO MENU", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.25f, new Vector4(0f, 0f, 0f, 1f), viewport);
            
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color = new Vector4(1f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.6f;
            button.UiShape.EngineObject.Transform.Position.X = -0.65f + 0.1f + moveEffect.X;
            if (ButtonHandler.TimerManager.CheckTimer("quit", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
            {
                _pauseMenu.IsVisible = false;
                _mainMenu.IsVisible = true;
            }
        }
        else
        {
            button.EngineObject.Material.Color = new Vector4(1f, 1f, 1f, 0.5f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.4f;
            button.UiShape.EngineObject.Transform.Position.X = -0.65f + 0f + moveEffect.X;
            fonts["Brigends"].DrawText("QUIT TO MENU", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.25f, new Vector4(1f), viewport);
        }
        
        //QUIT To Desktop
        button = (UiButton)UiGraphics.Elements["quitD"];
        if (button.PointCollision(screenPosNorm))
        {
            fonts["Brigends"].DrawText("QUIT TO DESKTOP", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.25f, new Vector4(0f, 0f, 0f, 1f), viewport);
            
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color = new Vector4(1f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.6f;
            button.UiShape.EngineObject.Transform.Position.X = -0.65f + 0.1f + moveEffect.X;
            if (ButtonHandler.TimerManager.CheckTimer("quit", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
            {
                _window.Close();
            }
        }
        else
        {
            button.EngineObject.Material.Color = new Vector4(1f, 1f, 1f, 0.5f);
            button.UiShape.EngineObject.Transform.Scale.X = 0.4f;
            button.UiShape.EngineObject.Transform.Position.X = -0.65f + 0f + moveEffect.X;
            fonts["Brigends"].DrawText("QUIT TO DESKTOP", 
                ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(13f),
                0.25f, new Vector4(1f), viewport);
        }

        UiGraphics.Elements["bg"].GetEngineObject().Transform.Position.X = -2f / 3f + moveEffect.X;
        //UiGraphics.Elements["backbg"].GetEngineObject().Material.Color = new Vector4(0f, 0f, 0f, 0.5f);
        
        fonts["Brigends"].DrawText("PAUSE ", new Vector2(110f + moveEffect.X * viewport.X, viewport.Y - 120f), 1f, new Vector4(1f), viewport);
        fonts["Brigends"].DrawText("MENU", new Vector2(110f + moveEffect.X * viewport.X, viewport.Y - 195f), 1f, new Vector4(1f), viewport);
        fonts["Pixel"].DrawText("mouse: " + mouse.ScreenPosition, new Vector2(80f + moveEffect.X * viewport.X, 50f), 0.35f, new Vector4(0.5f), viewport);
    }
}