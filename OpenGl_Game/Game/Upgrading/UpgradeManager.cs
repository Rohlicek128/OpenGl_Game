using System.Text.Json;
using OpenGl_Game.Engine;
using OpenGl_Game.Game.Gauges.Battery;
using OpenGl_Game.Game.Gauges.Speed;
using OpenGl_Game.Game.Gauges.Turn;

namespace OpenGl_Game.Game.Upgrading;

public class UpgradeManager
{
    public List<Upgrade> Upgrades { get; set; }

    public UpgradeManager()
    {
        Upgrades = LoadUpgrades(RenderEngine.DirectoryPath + @"Game\Upgrading\upgrades.json") ?? [];
    }

    public void SetStartUpgrades(int startLevel = 0)
    {
        foreach (var upgrade in Upgrades)
        {
            if (startLevel != 0) upgrade.CurrentLevel = startLevel;
            var amount = upgrade.Levels[upgrade.CurrentLevel].Amount;
            switch (upgrade.Category)
            {
                case UpgradeCategories.Turn:
                    upgrade.CurrentAmount = amount;
                    TurnGauge.MaxTurn = amount;
                    break;
                case UpgradeCategories.MaxSpeed:
                    upgrade.CurrentAmount = amount;
                    SpeedGauge.MaxSpeed = amount;
                    break;
                case UpgradeCategories.MaxBattery:
                    upgrade.CurrentAmount = amount;
                    Station.BatteryMax = amount;
                    break;
                case UpgradeCategories.AllocationSpeed:
                    upgrade.CurrentAmount = amount;
                    AllocationGauge.AllocationSpeed = amount;
                    break;
                case UpgradeCategories.MaxLaserSize:
                    upgrade.CurrentAmount = amount;
                    Station.MaxLaserRadius = amount;
                    break;
            }
        }
    }

    public List<Upgrade>? LoadUpgrades(string path)
    {
        using var sr = new StreamReader(path);
        return JsonSerializer.Deserialize<List<Upgrade>>(sr.ReadToEnd());
    }

    public void SaveUpgrades(string path)
    {
        using var sw = new StreamWriter(path);
        sw.WriteLine(JsonSerializer.Serialize(Upgrades));
        sw.Flush();
        sw.Close();
    }
}