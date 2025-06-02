using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Menus.Cycle;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Objectives.Targets;
using OpenGl_Game.Game.Objectives;
using OpenGl_Game.Game.Objectives.Targets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Objective;

public class ObjectivePage : ScreenPage
{
    public ObjectiveManager Objectives { get; set; }
    
    private int _selectedObjective;
    
    public ObjectivePage(Vector2i screenResolution, int screenObjectId, ObjectiveManager objectives) : base(screenResolution, screenObjectId)
    {
        Objectives = objectives;
        _selectedObjective = -1;
        
        var top = 0.35f;
        var offset = 0.35f;
        UiGraphics.Elements.Add("sBg", new UiRectangle(new Vector3(0.345f, -0.175f, 0f), Light.NormalizeRgba(20, 32, 46, 25), 1.13f, 1.35f));
        UiGraphics.Elements.Add("o1", new UiButton(new Vector3(-0.6f, top, 0f), new Vector4(1f), 0.65f, 0.3f));
        UiGraphics.Elements.Add("o2", new UiButton(new Vector3(-0.6f, top - offset * 1f, 0f), new Vector4(1f), 0.65f, 0.3f));
        UiGraphics.Elements.Add("o3", new UiButton(new Vector3(-0.6f, top - offset * 2f, 0f), new Vector4(1f), 0.65f, 0.3f));
        UiGraphics.Elements.Add("o4", new UiButton(new Vector3(-0.6f, top - offset * 3f, 0f), new Vector4(1f), 0.65f, 0.3f));
        UiGraphics.Elements.Add("accept", new UiButton(new Vector3(0.345f, -0.70f, 0f), new Vector4(1f), 1f, 0.18f));
        
        UiGraphics.Elements.Add("end", new UiButton(new Vector3(0.66f, 0.62f, 0f), Vector4.One, 0.5f, 0.135f));
        
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        GL.ClearColor(ScreenHandler.LcdBlack.X, ScreenHandler.LcdBlack.Y, ScreenHandler.LcdBlack.Z, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var cursor = (UiRectangle)UiGraphics.Elements["cursor"];
        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            cursor.EngineObject.IsVisible = true;
            cursor.EngineObject.Transform.Position.X = collision.LookingAtUv.X * 2f - 1f + cursor.EngineObject.Transform.Scale.X / 2f;
            cursor.EngineObject.Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f - cursor.EngineObject.Transform.Scale.Y / 2f;
        }
        else cursor.EngineObject.IsVisible = false;
        
        var end = (UiButton)UiGraphics.Elements["end"];
        var endSelected = end.PointCollision(collision.LookingAtUv * 2f - Vector2.One) && collision.LookingAtObject.Id == ScreenObjectId;
        if (endSelected)
        {
            end.EngineObject.Material.Color = new Vector4(1f);
            if (ButtonHandler.TimerManager.CheckTimer("resume", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left && DayMenu.Opacity <= 0f) && Station.BatteryPercentage < 0.35f)
            {
                Objectives.CurrentDay++;
                if (Objectives.CurrentDay > 5) DayMenu.IsEnd = true;
                DayMenu.StayBlack = true;
                DayMenu.Opacity = 2f;
                return;
            }
        }
        else end.EngineObject.Material.Color = Light.NormalizeRgba(191, 25, 39, 100);
        end.EngineObject.Transform.Scale = new Vector3(0.7f, 0.135f, 0f);
        end.EngineObject.Transform.Position.X = 0.56f;
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        fonts["Pixel"].DrawText("END DAY#" + Objectives.CurrentDay, new Vector2(
                (end.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 75f,
                (end.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 10f), 0.40f,
            endSelected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
        
        fonts["Brigends"].DrawText("OBJECTIVE", new Vector2(25f, ScreenResolution.Y - 60f), 0.5f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("Pick an target to destroy.", new Vector2(30f, ScreenResolution.Y - 82f), 0.3f, new Vector4(1f), ScreenResolution);
        //fonts["Pixel"].DrawText("SELECTED: " + _selectedObjective, new Vector2(25f, ScreenResolution.Y - 80f), 0.35f, new Vector4(1f), ScreenResolution);
        //fonts["Pixel"].DrawText("OBJECTIVE: " + _objectives.CurrentObjective?.Header, new Vector2(25f, ScreenResolution.Y - 100f), 0.35f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("MONEY: $" + UpgradePage.Money.ToString("F2") + "M", new Vector2(25f, ScreenResolution.Y - 135f), 0.4f, new Vector4(1f), ScreenResolution);
        
        for (int i = 0; i < 4; i++)
        {
            var button = (UiButton)UiGraphics.Elements["o" + (i + 1)];
            var exists = Objectives.GetObjectives().Count > i;
            var accepted = exists && Objectives.GetObjectives()[i] == Objectives.CurrentObjective;
            var completed = exists && Objectives.GetObjectives()[i].IsCompleted;
            var selected = button.PointCollision(collision.LookingAtUv * 2f - Vector2.One) && collision.LookingAtObject.Id == ScreenObjectId;
            if (selected && exists)
            {
                button.EngineObject.Material.Color = new Vector4(1f);
            
                if (mouse.IsDown && mouse.DownButton == MouseButton.Left) _selectedObjective = i;
            }
            else
            {
                if (!exists) button.EngineObject.Material.Color = new Vector4(1f, 1f, 1f, 0.025f);
                else if (completed) button.EngineObject.Material.Color = Light.NormalizeRgba(20, 32, 46, 35);
                else if (accepted) button.EngineObject.Material.Color = Light.NormalizeRgba(255, 157, 28, 50f);
                else button.EngineObject.Material.Color = Light.NormalizeRgba(20, 32, 46, 75);
            }
            
            if (!exists) continue;
            selected = selected || i == _selectedObjective;
            if (selected) button.EngineObject.Material.Color = new Vector4(1f);

            var header = Objectives.GetObjectives()[i].Header; //"EXTRATERRESTRIAL HELP"
            fonts["Pixel"].DrawText(header, 
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 90f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y + 15f), MathF.Min(0.425f, 1f / (0.215f * header.Length)),
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(completed ? 0.5f : 1f), ScreenResolution);
            fonts["Pixel"].DrawText("> " + Objectives.GetObjectives()[i].Target, 
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 85f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 10f), 0.275f,
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(completed ? 0.5f : 1f), ScreenResolution);
            fonts["Pixel"].DrawText("$" + Objectives.GetObjectives()[i].Pay + "M",
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 15f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 35f), 0.325f,
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(completed ? 0.5f : 1f), ScreenResolution);
            if (completed)
            {
                fonts["Pixel"].DrawText("<-------",
                    new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 90f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 35f), 0.325f,
                    selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(completed ? 0.5f : 1f), ScreenResolution);
            }
        }

        //UiGraphics.Elements["sBg"].GetEngineObject().Transform.Position = new Vector3(0.345f, -0.175f, 0f);
        //UiGraphics.Elements["sBg"].GetEngineObject().Transform.Scale = new Vector3(1.13f, 1.35f, 0f);

        if (_selectedObjective > -1 && _selectedObjective < Objectives.GetObjectives().Count)
        {
            var left = 250f;
            var top = ScreenResolution.Y - 195f;
            var objective = Objectives.GetObjectives()[_selectedObjective];

            var header = objective.Header.ToUpper(); //"EXTRATERRESTRIAL HELP"
            fonts["Pixel"].DrawText(header, new Vector2(left, top), MathF.Min(0.6f, 1f / (0.13f * header.Length)), new Vector4(1f),
                ScreenResolution);
            top -= 24f;
            fonts["Pixel"].DrawText(objective.Description.Trim(), new Vector2(left, top), 0.25f, new Vector4(1f),
                ScreenResolution);
            top -= 45f;
            fonts["Pixel"].DrawText("TARGET: " + objective.Target, new Vector2(left, top), 0.325f, new Vector4(1f),
                ScreenResolution);
            top -= 28f;
            fonts["Pixel"].DrawText("COUNTRY: " + objective.Country,
                new Vector2(left, top), 0.325f, new Vector4(1f), ScreenResolution);
            top -= 38f;
            fonts["Pixel"].DrawText("LON: " + (objective.TargetLongitude == 0f ? "N/A" : objective.TargetLongitude),
                new Vector2(left, top), 0.325f, new Vector4(1f), ScreenResolution);
            top -= 28f;
            fonts["Pixel"].DrawText("LAT: " + (objective.TargetLatitude == 0f ? "N/A" : objective.TargetLatitude),
                new Vector2(left, top), 0.325f, new Vector4(1f), ScreenResolution);
            top -= 28f;
            fonts["Pixel"].DrawText("EST. SIZE: " + (objective.Size <= 0f ? "N/A" : objective.Size + " km"),
                new Vector2(left, top), 0.325f, new Vector4(1f), ScreenResolution);
            top -= 38f;
            fonts["Pixel"].DrawText("PAY: $" + objective.Pay + "M", new Vector2(left, top), 0.325f, new Vector4(1f),
                ScreenResolution);

            var accept = (UiButton)UiGraphics.Elements["accept"];
            if (!Objectives.GetObjectives()[_selectedObjective].IsCompleted)
            {
                accept.EngineObject.IsVisible = true;

                var selected = accept.PointCollision(collision.LookingAtUv * 2f - Vector2.One) && collision.LookingAtObject.Id == ScreenObjectId;
                var accepted = Objectives.GetObjectives()[_selectedObjective] == Objectives.CurrentObjective;
                if (selected)
                {
                    accept.EngineObject.Material.Color = new Vector4(1f);
                    if (mouse.IsDown && mouse.DownButton == MouseButton.Left)
                    {
                        Objectives.PickObjective(_selectedObjective);
                    }
                }
                else
                    accept.EngineObject.Material.Color =
                        accepted ? new Vector4(1f) : Light.NormalizeRgba(255, 157, 28, 50f);

                fonts["Pixel"].DrawText(accepted ? "ACCEPTED" : "ACCEPT", new Vector2(
                        (accept.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - (accepted ? 63f : 44f),
                        (accept.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 10f), 0.45f,
                    selected || accepted ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
            }
            else
            {
                accept.EngineObject.IsVisible = false;
                fonts["Pixel"].DrawText("COMPLETED", new Vector2(
                        (accept.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 79f,
                        (accept.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 10f), 0.45f,
                    new Vector4(1f), ScreenResolution);
            }
        }
        else UiGraphics.Elements["accept"].GetEngineObject().IsVisible = false;
    }
}