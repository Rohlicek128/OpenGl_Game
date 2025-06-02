using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Objectives;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Engine.Menus.Cycle;

public class DayMenu : Menu
{
    public static float Opacity { get; set; }
    public static bool StayBlack { get; set; }
    
    public ObjectiveManager Objectives { get; set; }
    
    public DayMenu(GameWindow window, ObjectiveManager objectives)
    {
        Objectives = objectives;
        Pages.Add(new DayPage(window, this));
    }
}

public class DayPage : MenuPage
{
    private DayMenu _menu;
    private RenderEngine _engine;
    
    public DayPage(GameWindow window, DayMenu menu)
    {
        _engine = (RenderEngine)window;
        _menu = menu;
        
        UiGraphics.Elements.Add("bg", new UiRectangle(Vector3.Zero, new Vector4(0f, 0f, 0f, 1f), 2f, 2f));
        UiGraphics.Elements.Add("next", new UiButton(new Vector3(0.5f, 0f, 0f), new Vector4(1f), 0.35f, 0.25f));
        UiGraphics.InitProgram();
    }
    
    public override void RenderPage(Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        var screenPosNorm = mouse.ScreenPosition / viewport.ToVector2() * 2f - Vector2.One;

        var opacity = MathF.Abs(MathF.Sin(DayMenu.Opacity * MathF.PI / 2f));
        UiGraphics.Elements["bg"].GetEngineObject().Material.Color.W = opacity;
        
        //Next day
        var button = (UiButton)UiGraphics.Elements["next"];
        button.EngineObject.IsVisible = DayMenu.StayBlack && Math.Abs(DayMenu.Opacity - 1f) <= 0.01f;
        var selected = button.PointCollision(screenPosNorm);
        if (selected)
        {
            button.EngineObject.Material.Color = new Vector4(1f);
            if (ButtonHandler.TimerManager.CheckTimer("continue", deltaTime / 2f, mouse.IsDown && mouse.DownButton == MouseButton.Left))
            {
                _engine.Reset();
                DayMenu.StayBlack = false;
            }
        }
        else button.EngineObject.Material.Color = Light.NormalizeRgba(191, 25, 39, 100);

        button.EngineObject.Transform.Position = new Vector3(-0.25f, -0.225f, 0f);
        button.EngineObject.Transform.Scale = new Vector3(0.7f, 0.2f, 0f);
        
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        if (!DayMenu.StayBlack || Math.Abs(DayMenu.Opacity - 1f) > 0.01f)
        {
            DayMenu.Opacity -= deltaTime / 2f;
            return;
        }
        
        
        fonts["Pixel"].DrawText("DAY #" + _menu.Objectives.CurrentDay, new Vector2(viewport.X / 5f, viewport.Y / 2f - 45f), 1.5f, new Vector4(1f), viewport);
        
        fonts["Pixel"].DrawText("->", 
            ((button.UiShape.EngineObject.Transform.Position.Xy - button.UiShape.EngineObject.Transform.Scale.Xy / 2f) * 0.5f + new Vector2(0.5f)) * viewport + new Vector2(75f, 32f),
            0.85f, selected ? new Vector4(0f, 0f, 0f, 1f): new Vector4(1f), viewport);
    }
}