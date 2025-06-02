using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Gauges.Battery;
using OpenGl_Game.Game.Gauges.Speed;
using OpenGl_Game.Game.Gauges.Turn;
using OpenGl_Game.Game.Upgrading;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Objective;

public class UpgradePage : ScreenPage
{
    public static float Money { get; set; }

    private UpgradeManager _upgrades;
    
    public UpgradePage(Vector2i screenResolution, int screenObjectId) : base(screenResolution, screenObjectId)
    {
        _upgrades = new UpgradeManager();
        _upgrades.SetStartUpgrades();

        var top = 0.35f;
        var offset = 0.225f;
        for (int i = 0; i < 5; i++)
        {
            UiGraphics.Elements.Add("u" + (i + 1), new UiButton(new Vector3(0f, top - offset * i, 0f), new Vector4(1f), 1.8f, 0.2f));
        }
        
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport,
        Dictionary<string, FontMap> fonts, float deltaTime)
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
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        fonts["Brigends"].DrawText("UPGRADES", new Vector2(25f, ScreenResolution.Y - 60f), 0.5f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("Choose a component of the OLS to upgrade.", new Vector2(30f, ScreenResolution.Y - 82f), 0.3f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("MONEY: $" + Money.ToString("F2") + "M", new Vector2(40f, ScreenResolution.Y - 145f), 0.6f, new Vector4(1f), ScreenResolution);
        
        
        for (int i = 0; i < Math.Min(6, _upgrades.Upgrades.Count); i++)
        {
            var button = (UiButton)UiGraphics.Elements["u" + (i + 1)];
            var max = _upgrades.Upgrades[i].CurrentLevel >= 5;
            var selected = button.PointCollision(collision.LookingAtUv * 2f - Vector2.One) && collision.LookingAtObject.Id == ScreenObjectId;
            if (selected && !max) // && !max
            {
                button.EngineObject.Material.Color = new Vector4(1f);
                var price = _upgrades.Upgrades[i].Levels[_upgrades.Upgrades[i].CurrentLevel + 1].Price;
                if (Money - price >= 0f && ButtonHandler.TimerManager.CheckTimer("upgrade", deltaTime / 2f, mouse.IsDown && mouse.DownButton == MouseButton.Left))
                {
                    _upgrades.Upgrades[i].IncreaseLevel();
                    var amount = _upgrades.Upgrades[i].Levels[_upgrades.Upgrades[i].CurrentLevel].Amount;
                    switch (_upgrades.Upgrades[i].Category)
                    {
                        case UpgradeCategories.Turn:
                            TurnGauge.MaxTurn += amount;
                            _upgrades.Upgrades[i].CurrentAmount = TurnGauge.MaxTurn;
                            Money -= price;
                            break;
                        case UpgradeCategories.MaxSpeed:
                            SpeedGauge.MaxSpeed += amount;
                            _upgrades.Upgrades[i].CurrentAmount = SpeedGauge.MaxSpeed;
                            Money -= price;
                            break;
                        case UpgradeCategories.MaxBattery:
                            Station.BatteryPercentage *= Station.BatteryMax / (Station.BatteryMax + amount);
                            Station.BatteryMax += amount;
                            _upgrades.Upgrades[i].CurrentAmount = Station.BatteryMax;
                            Money -= price;
                            break;
                        case UpgradeCategories.AllocationSpeed:
                            AllocationGauge.AllocationSpeed -= amount;
                            _upgrades.Upgrades[i].CurrentAmount = AllocationGauge.AllocationSpeed;
                            Money -= price;
                            break;
                        case UpgradeCategories.MaxLaserSize:
                            Station.MaxLaserRadius += amount;
                            _upgrades.Upgrades[i].CurrentAmount = Station.MaxLaserRadius;
                            Money -= price;
                            break;
                    }
                }
            }
            else
            {
                button.EngineObject.Material.Color = max
                    ? Light.NormalizeRgba(191, 25, 39, 100f)
                    : Light.NormalizeRgba(20, 32, 46, 16.6f * (_upgrades.Upgrades[i].CurrentLevel + 1));
            }
            //button.EngineObject.Transform.Scale = new Vector3(1.8f, 0.2f, 0f);
            //button.EngineObject.Transform.Position.X = 0f;

            selected = selected && !max;
            var upgrade = _upgrades.Upgrades[i];
            fonts["Pixel"].DrawText(upgrade.Name.ToUpper(), 
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 255f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 0f), 0.4f,
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
            fonts["Pixel"].DrawText(upgrade.CurrentAmount + " " + upgrade.Units, 
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X - 245f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 20f), 0.25f,
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f, 1f, 1f, 0.3f), ScreenResolution);
            
            if (upgrade.CurrentLevel < upgrade.Levels.Count - 1)
            {
                fonts["Pixel"].DrawText("+" + upgrade.Levels[upgrade.CurrentLevel + 1].Amount,
                    new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 20f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 4f), 0.4f,
                    selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
                fonts["Pixel"].DrawText(upgrade.Units,
                    new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 21, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 18f), 0.2f,
                    selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f, 1f, 1f, 0.3f), ScreenResolution);
                
                fonts["Pixel"].DrawText("$" + upgrade.Levels[upgrade.CurrentLevel + 1].Price.ToString("F1") + "M", 
                    new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 115f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 8f), 0.325f,
                    selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
            }
            else
            {
                fonts["Pixel"].DrawText("MAX LEVEL", 
                    new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 10f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 8f), 0.325f,
                    selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
            }
            
            fonts["Pixel"].DrawText(upgrade.CurrentLevel + "", 
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 195f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 16f), 0.75f,
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f), ScreenResolution);
            fonts["Pixel"].DrawText("lvl", 
                new Vector2((button.EngineObject.Transform.Position.X * 0.5f + 0.5f) * ScreenResolution.X + 230f, (button.EngineObject.Transform.Position.Y * 0.5f + 0.5f) * ScreenResolution.Y - 16f), 0.3f,
                selected ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f, 1f, 1f, 0.3f), ScreenResolution);
        }
    }
}