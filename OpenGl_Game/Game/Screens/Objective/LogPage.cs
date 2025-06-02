using System.Globalization;
using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Objectives;
using OpenGl_Game.Game.Objectives.Targets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Objective;

public class LogPage : ScreenPage
{
    private CityTargets _cities;
    private ObjectiveManager _objectives;
    public Vector2 CurrentCoords { get; set; }
    
    public int CurrentHitIndex { get; set; }
    public List<LaserHitInfo> Hits { get; set; }
    private bool _laserChange;
    
    public LogPage(Vector2i screenResolution, int screenObjectId, ObjectiveManager objectives) : base(screenResolution, screenObjectId)
    {
        _cities = CityTargets.Instance;
        _objectives = objectives;
        Hits = [];
        CurrentHitIndex = 0;
        
        UiGraphics.Elements.Add("prev", new UiButton(new Vector3(0.58f, 0.64f, 0f), new Vector4(1f, 0f, 0f, 1f), 0.1f, 0.15f));
        UiGraphics.Elements.Add("next", new UiButton(new Vector3(0.7f, 0.64f, 0f), new Vector4(1f, 0f, 0f, 1f), 0.1f, 0.15f));
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        GL.ClearColor(ScreenHandler.LcdBlack.X, ScreenHandler.LcdBlack.Y, ScreenHandler.LcdBlack.Z, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        var button = (UiButton)UiGraphics.Elements["prev"];
        if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
        {
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color.X = 1f;
                
            if (ButtonHandler.TimerManager.CheckTimer("next", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
                CurrentHitIndex = Math.Max(0, CurrentHitIndex - 1);
        }
        else button.EngineObject.Material.Color.X = 0.25f;
        
        button = (UiButton)UiGraphics.Elements["next"];
        if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
        {
            button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
            button.EngineObject.Material.Color.X = 1f;
                
            if (ButtonHandler.TimerManager.CheckTimer("next", deltaTime, mouse.IsDown && mouse.DownButton == MouseButton.Left))
                CurrentHitIndex = Math.Min(Hits.Count - 1, CurrentHitIndex + 1);;
        }
        else button.EngineObject.Material.Color.X = 0.25f;
        
        
        fonts["Brigends"].DrawText("LOG BOOK", new Vector2(25f, ScreenResolution.Y - 60f), 0.5f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("Your Orbital Laser strikes log info.", new Vector2(30f, ScreenResolution.Y - 82f), 0.3f, new Vector4(1f), ScreenResolution);
        
        var cursor = (UiRectangle)UiGraphics.Elements["cursor"];
        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            cursor.EngineObject.IsVisible = true;
            cursor.EngineObject.Transform.Position.X = collision.LookingAtUv.X * 2f - 1f + cursor.EngineObject.Transform.Scale.X / 2f;
            cursor.EngineObject.Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f - cursor.EngineObject.Transform.Scale.Y / 2f;
        }
        else cursor.EngineObject.IsVisible = false;

        CurrentHitIndex = Math.Max(0, Math.Min(Hits.Count - 1, CurrentHitIndex));
        
        fonts["Pixel"].DrawText("----- SHOT #" + (CurrentHitIndex + 1) + " -----", new Vector2(25f, ScreenResolution.Y - 120f), 0.5f, new Vector4(1f), ScreenResolution);
        if (Hits.Count > 0)
        {
            fonts["Pixel"].DrawText("HIT COUNTRIES: " + Hits[CurrentHitIndex].GetCountriesToString(), new Vector2(35f, ScreenResolution.Y - 150f), 0.275f, new Vector4(1f), ScreenResolution);
            fonts["Pixel"].DrawText("START: " + (Hits[CurrentHitIndex].StartCoords.ToString() ?? "---"), new Vector2(35f, ScreenResolution.Y - 170f), 0.275f, new Vector4(1f), ScreenResolution);
            fonts["Pixel"].DrawText("END: " + (Hits[CurrentHitIndex].EndCoords.ToString() ?? "---"), new Vector2(35f, ScreenResolution.Y - 190f), 0.275f, new Vector4(1f), ScreenResolution);
            var offset = 235f;
            for (int i = 0; i < Math.Min(10, Hits[CurrentHitIndex].HitCities.Count); i++)
            {
                var city = Hits[CurrentHitIndex].HitCities[i];
                fonts["Pixel"].DrawText((i + 1) + ". " + city.Name + " (" +  city.Population.ToString("N0") + ")", new Vector2(55f, ScreenResolution.Y - offset), 0.4f, new Vector4(1f), ScreenResolution);
                offset += 30f;
            }

            if (Hits[CurrentHitIndex].HitCities.Count > 10)
            {
                fonts["Pixel"].DrawText("...", new Vector2(55f, ScreenResolution.Y - offset), 0.4f, new Vector4(1f), ScreenResolution);
                offset += 30f;
            }
            var obj = Hits[CurrentHitIndex].HitObjective;
            if (obj != null)
            {
                offset += 10f;
                fonts["Pixel"].DrawText("[" +(obj.Day + 1) + "] " + obj.Header + " -> " + obj.Target, new Vector2(55f, ScreenResolution.Y - offset), 0.4f, new Vector4(1f, 0.902f, 0.118f, 1f), ScreenResolution);
                offset += 30f;
            }
            fonts["Pixel"].DrawText("ESTIMATED DEATHS: " + Hits[CurrentHitIndex].HitCities.Sum(c => c.Population).ToString("N0"), new Vector2(35f, ScreenResolution.Y - offset - 10f), 0.3f, new Vector4(1f), ScreenResolution);
        }
        
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
    }

    public void LogHits(bool isShooting)
    {
        if (isShooting)
        {
            if (!_laserChange)
            {
                Hits.Add(new LaserHitInfo(CurrentCoords.Yx));
                _laserChange = true;
                CurrentHitIndex = Hits.Count - 1;
            }
            
            var hit = _cities.FindCityOnCoords(CurrentCoords, Station.LaserRadius, 15_000, true);
            if (_objectives.CheckCompletion(CurrentCoords.Yx, CityTargets.PopToRadius(hit.Population)))
            {
                Hits.Last().HitObjective = _objectives.CurrentObjective;
                UpgradePage.Money += _objectives.CurrentObjective?.Pay ?? 0f;
                _objectives.CurrentObjective = null;
            }
            
            if (hit.Name != null! && CityTargets.PopToRadius(hit.Population) <= Station.LaserRadius && (Hits.Last().HitCities.Count == 0 || !Hits.Last().HitCities.Any(c => c.Name.Equals(hit.Name))))
            {
                hit.IsDestroyed = true;
                var findCityOnCoords = _cities.FindCityOnCoords(CurrentCoords, Station.LaserRadius, Settings.Instance.MapCitiesMinPop, true);
                findCityOnCoords.IsDestroyed = true;
                
                Hits.Last().HitCities.Add(hit);
                if (Hits.Last().HitCountries.Count == 0 || !Hits.Last().HitCountries.Any(c => c.Name.Equals(hit.Country.Name)))
                {
                    Hits.Last().HitCountries.Add(hit.Country);
                }
            }
        }
        else if (_laserChange)
        {
            Hits.Last().EndCoords = CurrentCoords.Yx;
            Hits.Last().HitCities.Sort((x, y) => y.Population.CompareTo(x.Population));
            _laserChange = false;
        }
    }
}