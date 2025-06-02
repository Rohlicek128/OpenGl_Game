using System.Text.Json;
using OpenGl_Game.Engine;
using OpenGl_Game.Game.Buttons;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Objectives;

public class ObjectiveManager
{
    public Dictionary<int, List<Objective>> ObjectivesByOrder { get; set; }
    public int CurrentDay { get; set; }
    public Objective? CurrentObjective { get; set; }

    public ObjectiveManager()
    {
        ObjectivesByOrder = [];
        LoadObjectives();
    }

    public List<Objective> GetObjectives()
    {
        ObjectivesByOrder.TryGetValue(CurrentDay, out var list);
        if (list == null)
        {
            list = [];
            ObjectivesByOrder.Add(CurrentDay, list);
        }
        return list;
    }

    public bool CheckCompletion(Vector2 coords, float citySize)
    {
        if (!LaserButton.IsShooting || CurrentObjective == null) return false;

        var distance = (coords - new Vector2(CurrentObjective.TargetLongitude, CurrentObjective.TargetLatitude)).Length;
        if (distance / 360f * Earth.Circumference <= (CurrentObjective.Size <= 0f ? citySize * 5f : CurrentObjective.Size * 5f))
        {
            CurrentObjective.IsCompleted = true;
            return true;
        }

        return false;
    }

    public bool PickObjective(int index)
    {
        try
        {
            CurrentObjective = ObjectivesByOrder[CurrentDay][index];
        }
        catch (Exception)
        {
            return false;
        }
        return CurrentObjective != null;
    }

    private void LoadObjectives()
    {
        var files = Directory.GetFiles(RenderEngine.DirectoryPath + @"Game\Objectives\Missions");

        foreach (var file in files)
        {
            using var sr = new StreamReader(file);
            var objective = JsonSerializer.Deserialize<Objective>(sr.ReadToEnd())!;

            ObjectivesByOrder.TryGetValue(objective.Day, out var objectives);
            if (objectives != null) objectives.Add(objective);
            else ObjectivesByOrder.Add(objective.Day, [objective]);
            
            sr.Close();
        }

        foreach (var orders in ObjectivesByOrder)
        {
            orders.Value.Sort((x, y) => y.Pay.CompareTo(x.Pay));
        }
    }
}