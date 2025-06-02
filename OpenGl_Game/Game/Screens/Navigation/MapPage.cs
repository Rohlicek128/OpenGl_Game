using System.Text.Json;
using System.Text.Json.Serialization;
using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Objectives.Targets;
using OpenGl_Game.Game.Objectives.Targets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Navigation;

public class MapPage : ScreenPage
{
    public MapShader MapShader;
    private EngineObject _mapCountries;
    private EngineObject _mapCities;
    public EngineObject MapStation;
    public EngineObject MapTarget;
    public EngineObject MapHitmarks;
    
    private Matrix4 _mapProjection;
    private Matrix4 _mapView;
    private List<MapCountryShape> _countryShapes;

    public float Zoom;

    private int _earthHeldLenght;
    public Vector2 EarthAngle;
    private Vector2 _currentPos;
    private Vector2 _lastPos;

    private CityTargets _cities;
    
    public MapPage(Vector2i screenResolution, int screenObjectId) : base(screenResolution, screenObjectId)
    {
        _countryShapes = [];
        _cities = CityTargets.Instance;
        LoadMap(RenderEngine.DirectoryPath + @"Assets\Earth\borders.csv");

        Reset();
        
        MapStation = CreateStation();
        MapTarget = CreateTarget();
        MapHitmarks = CreateHitmark();
        _mapCities = CreateEarthCities(_cities.CitiesWithPop(Settings.Instance.MapCitiesMinPop));
        _mapCountries = CreateEarthMap(_countryShapes);
        MapShader = new MapShader([_mapCountries, _mapCities, MapStation, MapTarget]);
        
        UiGraphics.Elements.Add("bUp", new UiButton(new Vector3(0.8f, -0.65f, 0f), new Vector4(1f, 0f, 1f, 1f), 0.1f, 0.1f));
        UiGraphics.Elements.Add("bDown", new UiButton(new Vector3(0.8f, -0.8f, 0f), new Vector4(1f, 0f, 1f, 1f), 0.1f, 0.1f));
        
        UiGraphics.Elements.Add("reticuleX", new UiRectangle(new Vector3(0f), new Vector4(1f), 0.1f, 0.0075f));
        UiGraphics.Elements.Add("reticuleY", new UiRectangle(new Vector3(0f), new Vector4(1f), 0.0075f, 0.1f));
        
        UiGraphics.Elements.Add("cursor", new UiRectangle(new Vector3(0f), new Texture("pointer.png", 0), 0.075f, 0.075f));
        UiGraphics.Elements.Add("hand", new UiRectangle(new Vector3(0f), new Texture("handCursor.png", 0), 0.075f, 0.075f));
        UiGraphics.InitProgram();
    }

    public void SetCoords(Vector2 coords)
    {
        EarthAngle = coords * MathF.PI / 180f;
        _currentPos = coords * MathF.PI / 180f;
        _lastPos = coords * MathF.PI / 180f;
    }

    public override void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        _mapCities.Material.Color = new Vector4(38f / 255f, 81 / 255f, 181f / 255f, 1f);
        
        GL.ClearColor(ScreenHandler.LcdBlack.X, ScreenHandler.LcdBlack.Y, ScreenHandler.LcdBlack.Z, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            var button = (UiButton)UiGraphics.Elements["bUp"];
            if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
            {
                button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
                button.EngineObject.Material.Color.X = 1f;
                button.EngineObject.Material.Color.Z = 1f;
                
                if (mouse.IsDown && mouse.DownButton == MouseButton.Left) Zoom = MathF.Max(0.05f, Zoom / (deltaTime * 1.4f + 1f));
            }
            else
            {
                button.EngineObject.Material.Color.X = 0.15f;
                button.EngineObject.Material.Color.Z = 0.15f;
            }
            
            button = (UiButton)UiGraphics.Elements["bDown"];
            if (button.PointCollision(collision.LookingAtUv * 2f - Vector2.One))
            {
                button.Activate(mouse.IsDown && mouse.DownButton == MouseButton.Left);
                button.EngineObject.Material.Color.X = 1f;
                button.EngineObject.Material.Color.Z = 1f;
                
                if (mouse.IsDown && mouse.DownButton == MouseButton.Left) Zoom = MathF.Min(3f, Zoom * (deltaTime * 1.4f + 1f));
            }
            else
            {
                button.EngineObject.Material.Color.X = 0.15f;
                button.EngineObject.Material.Color.Z = 0.15f;
            }
        
        
            if (mouse.IsDown && mouse.DownButton == MouseButton.Left && collision.LookingAtObject.Id == ScreenObjectId)
            {
                _earthHeldLenght++;
                const float dpi = 1.75f;
                var xi = Zoom * dpi;
                var yi = Zoom * dpi;
                EarthAngle.X = -((_currentPos.X - collision.LookingAtUv.X) * xi - _lastPos.X);
                EarthAngle.Y = MathF.Max(-MathF.PI / 2f * 0.9888f, MathF.Min(MathF.PI / 2f * 0.9888f, (_currentPos.Y - collision.LookingAtUv.Y) * yi + _lastPos.Y));
                //_earthMap.Transform.Quaternion = Quaternion.FromEulerAngles(new Vector3(-_earthAngle.Y, _earthAngle.X, 0f));

                if (EarthAngle.X <= -MathF.PI) EarthAngle.X += 2f * MathF.PI;
                if (EarthAngle.X >= MathF.PI) EarthAngle.X -= 2f * MathF.PI;
            }
        }

        _mapCountries.PointSize = MathF.Max(1f, 1f / Zoom);
        _mapCities.PointSize = MathF.Max(1f, 1f / Zoom) * 2f;
        MapTarget.PointSize = MathF.Max(1f, 0.1f / Zoom) * 10f;

        ((UiRectangle)UiGraphics.Elements["reticuleX"]).EngineObject.Transform.Scale.X = MathF.Min(0.5f, MathF.Max(0.02f, 0.05f / Zoom));
        ((UiRectangle)UiGraphics.Elements["reticuleY"]).EngineObject.Transform.Scale.Y = MathF.Min(0.5f, MathF.Max(0.02f, 0.05f / Zoom));

        var cameraPos = Earth.GpsToSphereCoords(new Vector2(EarthAngle.X * 180f / MathF.PI, EarthAngle.Y * 180f / MathF.PI)) * 10f;
        _mapView = Matrix4.LookAt(
            cameraPos,
            Vector3.Zero, 
            -Vector3.UnitY
        );
        _mapProjection = Matrix4.CreateOrthographic(Zoom, Zoom, 0.01f, 9.99f);
        MapShader.Draw(_mapView * _mapProjection, cameraPos);
        
        
        UiRectangle cursor;
        if (mouse.IsDown && mouse.DownButton == MouseButton.Left)
        {
            cursor = (UiRectangle)UiGraphics.Elements["hand"];
            UiGraphics.Elements["cursor"].GetEngineObject().IsVisible = false;
        }
        else
        {
            cursor = (UiRectangle)UiGraphics.Elements["cursor"];
            UiGraphics.Elements["hand"].GetEngineObject().IsVisible = false;
        }
        if (collision.LookingAtObject.Id == ScreenObjectId)
        {
            cursor.EngineObject.IsVisible = true;
            cursor.EngineObject.Transform.Position.X = collision.LookingAtUv.X * 2f - 1f + cursor.EngineObject.Transform.Scale.X / 2f;
            cursor.EngineObject.Transform.Position.Y = collision.LookingAtUv.Y * 2f - 1f - cursor.EngineObject.Transform.Scale.Y / 2f;

            if (mouse.IsDown && mouse.DownButton == MouseButton.Left && _earthHeldLenght == 1) _currentPos = collision.LookingAtUv;
        }
        else cursor.EngineObject.IsVisible = false;

        if (_earthHeldLenght > 0 && (!mouse.IsDown || collision.LookingAtObject.Id != ScreenObjectId))
        {
            _earthHeldLenght = 0;
            _lastPos = EarthAngle;
        }
        
        UiGraphics.GraphicsProgram.Draw(viewport.ToVector2());
        
        fonts["Pixel"].DrawText("+", (UiGraphics.Elements["bUp"].GetEngineObject().Transform.Position.Xy / 2f + new Vector2(0.5f)) * ScreenResolution - new Vector2(9f, 15f), 0.7f, new Vector4(0.55f), ScreenResolution);
        fonts["Pixel"].DrawText("-", (UiGraphics.Elements["bDown"].GetEngineObject().Transform.Position.Xy / 2f + new Vector2(0.5f)) * ScreenResolution - new Vector2(9f, 15f), 0.7f, new Vector4(0.55f), ScreenResolution);
        
        fonts["Pixel"].DrawText("MAP", new Vector2(25f, ScreenResolution.Y - 45f), 0.4f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText(MathF.Round(Zoom * 100f) / 100f + "x", new Vector2(25f, ScreenResolution.Y - 75f), 0.4f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("LON: " + MathF.Floor(MathHelper.RadiansToDegrees(-EarthAngle.X) * 1000f) / 1000f, new Vector2(25f, ScreenResolution.Y - 105f), 0.4f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("LAT: " + MathF.Floor(MathHelper.RadiansToDegrees(EarthAngle.Y) * 1000f) / 1000f, new Vector2(25f, ScreenResolution.Y - 135f), 0.4f, new Vector4(1f), ScreenResolution);
        
        var currentCity = _cities.FindCityOnCoords(EarthAngle.Yx * new Vector2(1f, -1f) * 180f / MathF.PI, 15.5f, Settings.Instance.MapCitiesMinPop);
        fonts["Pixel"].DrawText(currentCity.Country.Name != null! ? Station.LaserRadius + "/" + MathF.Ceiling(CityTargets.PopToRadius(currentCity.Population) * 100f) / 100f + " km" : "", new Vector2(25f, 145f), 0.45f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText(currentCity.Name + (currentCity.Capital == Capital.Primary ? " (Capital)" : ""), new Vector2(25f, 105f), 0.45f, new Vector4(1f, currentCity.IsDestroyed ? 0f : 1f, currentCity.IsDestroyed ? 0f : 1f, 1f), ScreenResolution);
        fonts["Pixel"].DrawText(currentCity.Country.Name != null! ? currentCity.Country.Name + ", " + currentCity.Region : "---", new Vector2(25f, 78f), 0.3f, new Vector4(1f), ScreenResolution);
        fonts["Pixel"].DrawText("POP: " + currentCity.Population.ToString("N0"), new Vector2(25f, 45f), 0.4f, new Vector4(1f), ScreenResolution);
    }

    public void SetTargetPosition(Vector2 coords, int offset)
    {
        var s = Earth.GpsToSphereCoords(new Vector2(-coords.X, coords.Y));
        float[] d = [s.X, s.Y, s.Z];
        
        MapShader.VertexBuffer.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, (MapShader.VertexBuffer.Data.Length - offset) * sizeof(float), 3 * sizeof(float), d);
        MapShader.VertexBuffer.Unbind();
    }

    public void Reset()
    {
        Zoom = 1.5f;
        _earthHeldLenght = 0;
        
        EarthAngle = _cities.CitiesWithPop(1_000_000)[Random.Shared.Next(0, _cities.CitiesWithPop(1_000_000).Count - 1)].Coordinates.Yx * MathF.PI / 180f * new Vector2(-1f, 1f);
        _currentPos = EarthAngle;
        _lastPos = EarthAngle;
    }

    private void LoadMap(string path)
    {
        using var sr = new StreamReader(path);
        
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true
        };

        
        var line = sr.ReadLine();
        while ((line = sr.ReadLine()) != null)
        {
            var split = line.Split(';');

            var json = split[1]
                .Replace("\"\"", "\"")
                .Replace("[[[", "[[")
                .Replace("]]]", "]]")
                .Replace("\"{\"coordinates\": [[", "{\"coordinates\": [[[")
                .Replace("]], \"type\"", "]]], \"type\"")
                .Replace("}\"", "}")
                .Replace("[[[[", "[[[").Replace("]]]]", "]]]");

            _countryShapes.Add(JsonSerializer.Deserialize<MapCountryShape>(json, options));
            Console.WriteLine(split[5]);
        }
    }

    private EngineObject CreateEarthMap(List<MapCountryShape> shapes)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        uint count = 0;

        foreach (var shape in shapes)
        {
            foreach (var polygon in shape.coordinates)
            {
                foreach (var vector in polygon)
                {
                    var s = Earth.GpsToSphereCoords(new Vector2(-vector[0], vector[1]));
                    for (int i = 0; i < 3; i++)
                    {
                        verts.Add(s[i]);
                    }
                    inds.Add(count);
                    count++;
                }
                //inds.Add(RenderEngine.PrimitiveIndex);
            }
        }

        return new EngineObject(
            "Earth Map",
            new Transform(Vector3.Zero),
            new MeshData(verts.ToArray(), inds.ToArray(), OpenTK.Graphics.OpenGL.PrimitiveType.Points),
            new Material(new Vector3(0f, 0f, 1f))
        );
    }
    
    private EngineObject CreateEarthCities(List<City> cities)
    {
        var verts = new List<float>();
        var inds = new List<uint>();
        uint count = 0;

        foreach (var city in cities)
        {
            var s = Earth.GpsToSphereCoords(new Vector2(-city.Coordinates.Y, city.Coordinates.X));
            for (int i = 0; i < 3; i++)
            {
                verts.Add(s[i]);
            }
            inds.Add(count);
            count++;
        }

        return new EngineObject(
            "Cities",
            new Transform(Vector3.Zero),
            new MeshData(verts.ToArray(), inds.ToArray(), OpenTK.Graphics.OpenGL.PrimitiveType.Points),
            new Material(new Vector3(1f, 0f, 0f))
        );
    }
    
    private EngineObject CreateStation()
    {
        return new EngineObject(
            "Station",
            new Transform(Vector3.Zero),
            new MeshData([-1f, 0f, 0f], [0], OpenTK.Graphics.OpenGL.PrimitiveType.Points),
            new Material(new Vector3(0f, 1f, 0f)),
            pointSize: 10f
        );
    }
    
    private EngineObject CreateTarget()
    {
        return new EngineObject(
            "Target",
            new Transform(Vector3.Zero),
            new MeshData([0f, -1f, 0f], [0], OpenTK.Graphics.OpenGL.PrimitiveType.Points),
            new Material(new Vector3(1f, 0f, 0f)),
            pointSize: 7f
        );
    }
    
    private EngineObject CreateHitmark()
    {
        return new EngineObject(
            "Hitmark",
            new Transform(Vector3.Zero),
            new MeshData([0f, 0f, 0f, 0f, 0f, 0f], [0, 1], OpenTK.Graphics.OpenGL.PrimitiveType.Lines),
            new Material(new Vector3(1f, 0f, 0f))
        );
    }
}