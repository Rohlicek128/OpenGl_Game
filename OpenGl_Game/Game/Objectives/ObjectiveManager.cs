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
        LoadListObjectives();
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

    /// <summary>
    /// Based on inputted coords in check whether the objective is completed based on the distance
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="citySize"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Loads all objectives form a JSON Array and loads them into the game (\Game\Objectives\Missions\...)
    /// </summary>
    private void LoadListObjectives()
    {
        var files = Directory.GetFiles(RenderEngine.DirectoryPath + @"Game\Objectives\Missions");

        var objectives = new List<Objective>();
        foreach (var file in files)
        {
            using var sr = new StreamReader(file);
            objectives = objectives.Concat(JsonSerializer.Deserialize<List<Objective>>(sr.ReadToEnd())!).ToList();
            sr.Close();
        }

        foreach (var objective in objectives)
        {
            ObjectivesByOrder.TryGetValue(objective.Day, out var objs);
            if (objs != null) objs.Add(objective);
            else ObjectivesByOrder.Add(objective.Day, [objective]);
        }

        foreach (var orders in ObjectivesByOrder)
        {
            orders.Value.Sort((x, y) => y.Pay.CompareTo(x.Pay));
        }
    }
}