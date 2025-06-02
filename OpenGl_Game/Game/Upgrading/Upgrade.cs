namespace OpenGl_Game.Game.Upgrading;

public class Upgrade
{
    public UpgradeCategories Category { get; set; }
    public string Name { get; set; }
    public string Units { get; set; }
    public float CurrentAmount;
    
    public List<UpgradeLevel> Levels { get; set; }
    public int CurrentLevel { get; set; }

    public Upgrade(UpgradeCategories category, string name, string units, List<UpgradeLevel> levels, int currentLevel = 0)
    {
        Category = category;
        Name = name;
        Units = units;
        Levels = levels;
        CurrentLevel = currentLevel;
    }

    public UpgradeLevel IncreaseLevel()
    {
        CurrentLevel = Math.Min(Levels.Count - 1, CurrentLevel + 1);
        return Levels[CurrentLevel];
    }
    
    public UpgradeLevel DecreaseLevel()
    {
        CurrentLevel = Math.Max(0, CurrentLevel - 1);
        return Levels[CurrentLevel];
    }
}