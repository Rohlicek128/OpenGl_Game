using System.Net.Mime;
using OpenGl_Game.Engine.Editor;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.PostProcess;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Menus;
using OpenGl_Game.Engine.Menus.Cycle;
using OpenGl_Game.Engine.Menus.Main;
using OpenGl_Game.Engine.Menus.Pause;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.Objects.Collisions;
using OpenGl_Game.Engine.UI;
using OpenGl_Game.Engine.UI.Obsolete;
using OpenGl_Game.Game;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Buttons.LaserParams;
using OpenGl_Game.Game.Gauges;
using OpenGl_Game.Game.Gauges.Battery;
using OpenGl_Game.Game.Gauges.Speed;
using OpenGl_Game.Game.Gauges.Turn;
using OpenGl_Game.Game.Gauges.Warnings;
using OpenGl_Game.Game.Objectives;
using OpenGl_Game.Game.Screens;
using OpenGl_Game.Game.Screens.Navigation;
using OpenGl_Game.Game.Screens.Objective;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using FontMap = OpenGl_Game.Engine.Graphics.UI.Text.FontMap;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using MouseMoveEventArgs = OpenTK.Windowing.Common.MouseMoveEventArgs;

namespace OpenGl_Game.Engine;

public class RenderEngine : GameWindow
{
    public static readonly string DirectoryPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"bin\Debug\net8.0\", "");
    public const uint PrimitiveIndex = uint.MaxValue;
    public const int MaxObjectIds = 255;

    private Settings _settings;

    private Camera _mainCamera;
    private float _fpsTimer;
    private int _fpsCount, _fpsDisplay;
    private float _deltaTime;
    
    private bool _isMouseGrabbed = true;
    private bool _wireframeMode = false;
    private bool _isPostProcess = true;
    private bool _moveEarthMode = true;
    private bool _isFullscreen = false;
    private bool _beenResized = false;
    private bool _mainMenuChanged = false;
    private bool _debugMode = false;
    private bool _testBool = false;
    
    private int _renderAim;

    private Mouse _mouse;
    private Vector2i _viewport;
    private int _renderScale;
    
    private GeometryShader _geometryShader;
    private LightingShader _lightingShader;
    private LightShader _lightShader;
    private LaserShader _laserShader;
    private CollisionShader _collisionShader;
    private OutlineShader _outlineShader;
    private GBuffer _gBuffer;
    
    private PostProcessShader _postProcessShader;

    private FontShader _fontShader;
    private Dictionary<string, FontMap> _fonts;
    private Canvas _canvas;

    private ShadowMap _shadows;
    private Ssao _ssao;
    
    private CubeMap _skybox;

    private EngineObject[] _objects;
    private Dictionary<LightTypes, List<Light>> _lights;

    private TimerManager _timerManager;

    private float _testVal = 1f;
    
    private Earth _earth;
    private Station _station;

    //Menus
    private MainMenu _mainMenu;
    private PauseMenu _pauseMenu;
    private DayMenu _dayMenu;
    
    public RenderEngine(int width, int height, string title) : base(
            GameWindowSettings.Default, 
            new NativeWindowSettings
            {
                Title = title,
                Size = new Vector2i(width, height),
                WindowBorder = WindowBorder.Resizable,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 6),
                DepthBits = 24
            }
        )
    {
        Icon = CreateWindowIcon(@"Icon\logo_v0.png");
        _mouse = new Mouse();
        _viewport = new Vector2i(width, height);
        _renderScale = 1;
        CenterWindow();
        
        _settings = Settings.Instance;

        _fonts = new Dictionary<string, FontMap>();
        _timerManager = new TimerManager(150);

        _mainMenu = new MainMenu(this) { IsVisible = true };
        _pauseMenu = new PauseMenu(this, _mainMenu);
        
        _mainCamera = new Camera(new Vector3(-0.52f, 1.15f, 0f), 1f, _settings.PlayerSensitivity / 100f, _settings.PlayerFov, 0.025f, 2_000_000f);
        _mainCamera.UpdateSensitivityByAspect(_viewport);
    }

    public WindowIcon CreateWindowIcon(string path)
    {
        //StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromStream(File.OpenRead(DirectoryPath + @"Assets\" + path), ColorComponents.RedGreenBlueAlpha);
        
        return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, image.Data));
    }

    protected override void OnLoad()
    {
        VertexAttribute[] verticesAttribs = [
            new(VertexAttributeType.Position, 3), 
            new(VertexAttributeType.TextureCoords, 2), 
            new(VertexAttributeType.Normal, 3)
        ];
        
        //Texture
        _skybox = new CubeMap("white1x1.png", false);
        
        //Fonts
        var text = EngineObject.CreateEmpty();
        text.MeshData.Vertices = new float[6 * 4];
        _fontShader = new FontShader(text);
        
        _fonts.Add("ATName", new FontMap("ATNameSansTextTrial-ExtraBold.otf", _fontShader));
        _fonts.Add("Pixel", new FontMap("04B_03_.TTF", _fontShader));
        _fonts.Add("Brigends", new FontMap("brigends.ttf", _fontShader, 100));
        
        //Objects
        /*var floor = new EngineObject(
            "Floor", 
            new Transform(new Vector3(0f, -0.501f, 0f)), 
            MeshConstructor.CreatePlane(),
            new Material(new Vector4(1f))
        );
        floor.Material.Shininess = 32f;
        floor.Material.Specular = new Vector3(0.5f);
        floor.Transform.Scale *= 20f;
        floor.Textures.Scaling = 0.5f;*/
        
        
        _earth = new Earth(new Transform(new Vector3(0f), new Vector3(0f, MathF.PI, 0f), new Vector3(6378 / 2f)), _settings.EarthResolution, 0.025f);
        _earth.EarthObject.Transform.Position = new Vector3(0f, -_earth.EarthObject.Transform.Scale[Earth.EarthAxis] * (300f / 6378f + 1f), 0f);
        _earth.CollisionSphere.Transform.Position = _earth.EarthObject.Transform.Position;
        
        _station = new Station();
        
        _dayMenu = new DayMenu(this, ((ObjectivePage)_station.Screens.Values.ToArray()[0].Pages[0]).Objectives);
        
        var laser = new EngineObject(
            "Laser",
            new Transform(Vector3.Zero, new Vector3(0f, MathF.PI / 2f, 0f), new Vector3(1f, 1f, 1f)),
            MeshConstructor.LoadObjFromFileAssimp(@"Station\laserCylinder.obj"),
            new Material(new Vector3(40f, 1f, 1f))
        );
        laser.Transform.Scale[Earth.EarthAxis] = 200f;
        laser.Transform.Position[Earth.EarthAxis] = -100;
        //laser.Transform.Quaternion = Quaternion.FromEulerAngles(12f, 0f, 0f) * laser.Transform.Quaternion;
        
        //lights
        var sun = new Light(
            "Dir light", 
            new Transform(Vector3.UnitY * 12f, new Vector3(MathHelper.DegreesToRadians(100f), 0f, MathHelper.DegreesToRadians(45f))), 
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f)),
            new Vector3(0.2f, 1f, 1f),
            new Vector3(1.0f, 0.045f * 10, 0.0075f * 10),
            LightTypes.Directional
        );
        sun.Transform.Scale *= 20f;
        sun.LightForId = 1;
        
        var pointLight = new Light(
            "Point light", 
            new Transform(new Vector3(-1.2121286f, 1.9562248f, 1.1814175f), Vector3.Zero, Vector3.One * 0.1f),
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f, 0.95f, 0.81f)), // 247f / 255f, 221f / 255f, 190f / 255f
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.45f, 0.75f),
            LightTypes.Point
        );
        var laserLight = new Light(
            "Laser Light", 
            new Transform(Vector3.Zero), 
            MeshConstructor.CreateCube(),
            laser.Material,
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.045f, 0.0075f),
            LightTypes.Point
        );
        laserLight.IsVisible = false;
        laserLight.Transform.Position[Earth.EarthAxis] = _earth.EarthObject.Transform.Position[Earth.EarthAxis] + _earth.EarthObject.Transform.Scale[Earth.EarthAxis];
        laserLight.LightForId = 1;
        
        _objects = [
            _earth.EarthObject, _station.StationObject,
            .._station.Buttons.Values.Select(b => b.EngineObject),
            .._station.Screens.Values.Select(b => b.EngineObject)
        ];
        
        _lights = new Dictionary<LightTypes, List<Light>>
        {
            { LightTypes.Directional, [sun] },
            { LightTypes.Point, [pointLight, laserLight, ..((WarningPage)_station.Screens.Values.ToArray()[5].Pages[0])._warningLights] }
        };
        
        //Geometry Shader
        _geometryShader = new GeometryShader(_objects.ToList(), verticesAttribs);
        //Lighting Shader
        _lightingShader = new LightingShader(_lights);
        
        //Light Shader
        _lightShader = new LightShader(_lights, verticesAttribs);

        _laserShader = new LaserShader(laser, verticesAttribs);
        ((LaserButton)_station.Buttons.First().Value).LaserShader = _laserShader;
        
        //Post Process
        _outlineShader = new OutlineShader(_geometryShader, _viewport);
        
        _postProcessShader = new PostProcessShader(_viewport / _renderScale);
        _postProcessShader.Shaders.Add(_outlineShader);
        
        //Collision
        _collisionShader = new CollisionShader(_geometryShader);
        
        //gBuffer
        _gBuffer = new GBuffer(_viewport);
        
        //Shadows
        //_shadows = new ShadowMap(new Vector2i(8192, 8192), 1100f, 300f, new Vector2(0.01f, 400f), _geometryShader);
        _shadows = new ShadowMap(new Vector2i(8192, 8192), 14f, 10f, new Vector2(1f, 18f), _geometryShader);
        _ssao = new Ssao([
                new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
                new Shader(@"shadersSSAO\ssaoShader.frag", ShaderType.FragmentShader)], [
                new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
                new Shader(@"shaderBlurSSAO\ssaoBlurShader.frag", ShaderType.FragmentShader)
        ], _viewport, 128, 4);
        
        //UI
        var reticule = EngineObject.CreateEmpty();
        reticule.MeshData = MeshConstructor.CreateQuad();
        reticule.Transform.Scale *= 0.005f;
        reticule.Material.Color.W = 0.75f;
        _canvas = new Canvas(reticule);
        
        Reset(true);
        
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        GL.Enable(EnableCap.PrimitiveRestart);
        GL.PrimitiveRestartIndex(PrimitiveIndex);
        
        GL.Enable(EnableCap.ProgramPointSize);
        GL.PointSize(3f);

        Console.WriteLine("start");
        _isFullscreen = _settings.WindowMode == 0;
        WindowState = _isFullscreen ? WindowState.Fullscreen : WindowState.Normal;
        IsVisible = true;
    }

    protected override void OnUnload()
    {
        _geometryShader.DeleteAll();
        _lightShader.DeleteAll();
        _laserShader.DeleteAll();
        
        _postProcessShader.DeleteAll();
        _shadows.DeleteAll();
        _gBuffer.DeleteGBuffer();
        _ssao.Delete();
        foreach (var font in _fonts) font.Value.Delete();
        
        _station.Delete();
        _earth.Delete();
        
        Console.WriteLine("end");
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        var nav = (NavigationScreen)_station.Screens.Values.ToArray()[1];
        var page = (AimPage)nav.Pages[1];
        if (!_pauseMenu.IsVisible || _beenResized)
        {
            //Aim
            if (_renderAim >= 0 && nav.IsTurnOn && nav.PageIndex == 1 && !_mainMenu.IsVisible)
            {
                ResizeGame(page.ScreenResolution, 1);
                DrawScene(page.AimCamera, page.ScreenResolution, 1, page.SceneFramebuffer, 1);
                ResizeGame(_viewport, _renderScale);
                _renderAim = -1;
            }
            _renderAim++;
        
            //Shadows
            GL.Enable(EnableCap.DepthTest);
            _shadows.Draw(_lights[LightTypes.Directional][0], _mainCamera, _viewport, _geometryShader);
         
            //Scene
            DrawScene(_mainMenu.IsVisible ? _mainMenu.Camera : _mainCamera, _viewport, _renderScale, _postProcessShader.Framebuffer, _mainMenu.IsVisible ? 1 : 0);
            if (_beenResized) _beenResized = false;
        }
        _postProcessShader.Draw(_isPostProcess ? -1 : _gBuffer.ColorSpecTexture.Handle); //_shadows.TextureHandle : _gBuffer.NormalsTexture.Handle
        if (!_mainMenu.IsVisible)
        {
            _postProcessShader.DrawShaders();

            if (_mainMenuChanged) DayMenu.Opacity = 1f;
            _dayMenu.IsVisible = DayMenu.Opacity > 0f;
        }
        _mainMenuChanged = _mainMenu.IsVisible;
        GL.Disable(EnableCap.DepthTest);
        var q = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(-_station.Coordrinates.Y), 0f, MathHelper.DegreesToRadians(_station.Coordrinates.X));
        q.Xyz = _earth.EarthObject.Transform.Quaternion.Inverted().Xyz;
        q.W = _earth.EarthObject.Transform.Quaternion.Inverted().W;
        ((MapPage)nav.Pages[0]).SetTargetPosition(_station.Coordrinates.Yx, 6);
        if (!DayMenu.StayBlack) foreach (var screen in _station.Screens) screen.Value.RenderScreen(_collisionShader, _mouse, _viewport / _renderScale, _fonts, _deltaTime);
        
        
        //var pixel = ReadPixel(_viewport.X / 2, _viewport.Y / 2);
        //Console.WriteLine(pixel[0] + ", " + pixel[1] + ", " + pixel[2] + ", " + pixel[3]);
        
        //Text
        if (!_mainMenu.IsVisible) _fonts["Brigends"].DrawText(_fpsDisplay + " fps" + (_debugMode ? " (" + MathF.Round(_deltaTime * 10000f) / 10f + "ms)" : ""), new Vector2(25f, _viewport.Y - 60f), 0.5f, new Vector4(1f), _viewport);
        if (_debugMode && !_mainMenu.IsVisible)
        {
            _fonts["Pixel"].DrawText(_mainCamera.Fov + " fov", new Vector2(25f, _viewport.Y - 110f), 0.75f, new Vector4(1f), _viewport);
            
            var tempCam = _testBool ? page.AimCamera : _mainCamera;
            _fonts["Pixel"].DrawText("PLAYER X: "+ (MathF.Floor(tempCam.Transform.Position.X * 1000f) / 1000f) + " Y: " + (MathF.Floor(tempCam.Transform.Position.Y * 1000f) / 1000f) + " Z: "+ (MathF.Floor(tempCam.Transform.Position.Z * 1000f) / 1000f),
                new Vector2(25f, _viewport.Y - 160f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("FRONT  X: "+ (MathF.Floor(tempCam.Front.X * 1000f) / 1000f) + " Y: " + (MathF.Floor(tempCam.Front.Y * 1000f) / 1000f) + " Z: "+ (MathF.Floor(tempCam.Front.Z * 1000f) / 1000f) + " P: "+ (MathF.Floor(tempCam.Pitch * 1000f) / 1000f) + " Y: "+ (MathF.Floor(tempCam.Yaw * 1000f) / 1000f),
                new Vector2(25f, _viewport.Y - 210f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("Boost: " + _mainCamera.BoostSpeed + ", Speed: " + ((SpeedPage)_station.Screens.Values.ToArray()[3].Pages[0]).ActualSpeed +  " km/s", new Vector2(25f, _viewport.Y - 260f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("FOV: " + _mainCamera.Fov + ", Zoom: " + _mainCamera.ZoomFov, new Vector2(25f, _viewport.Y - 310f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("VALUE: Y: " + _mainCamera.Yaw + ", P: " + _mainCamera.Pitch, new Vector2(25f, _viewport.Y - 360f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("Altitude: " + Math.Floor(-(_earth.EarthObject.Transform.Scale[Earth.EarthAxis] + _earth.EarthObject.Transform.Position[Earth.EarthAxis]) * 2f) + " km", new Vector2(25f, _viewport.Y - 410f), 0.5f, new Vector4(1f), _viewport);
            
            //var hit = _earth.CollisionSphere.CheckCollision(new Ray(_camera.Transform.Position, _camera.Front));
            //if (!hit.HasHit) _fonts["Pixel"].DrawText("RAY: NO COLLISION", new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
            //else _fonts["Pixel"].DrawText("RAY: HIT, POS: " + hit.HitPos + ", LEN: " + hit.Distance, new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
            
            _fonts["Pixel"].DrawText("LUV: " + _collisionShader.LookingAtUv.X + ", " + _collisionShader.LookingAtUv.Y, new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("Coords: " + _station.Coordrinates.Yx, new Vector2(25f, _viewport.Y - 510f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("TESTBOOL: " + _testBool, new Vector2(25f, _viewport.Y - 560f), 0.5f, new Vector4(1f), _viewport);
            
            _fonts["Pixel"].DrawText(_collisionShader.LookingAtObject.Name + " [" + _collisionShader.LookingAtObject.Id + "]", new Vector2(_viewport.X / 2f + 10f, _viewport.Y / 2f + 10f), 0.4f, new Vector4(1f), _viewport);
        }
        else if (_station.Buttons.ContainsKey(_collisionShader.LookingAtObject.Id) && !_mainMenu.IsVisible)
        {
            _fonts["Pixel"].DrawText(_collisionShader.LookingAtObject.Name, new Vector2(_viewport.X / 2f + 10f, _viewport.Y / 2f + 10f), 0.4f, new Vector4(1f), _viewport);
        }
        _fonts["Pixel"].DrawText("FROM ORBIT v0.3.6", new Vector2(_viewport.X - 255f, _viewport.Y - 30f), 0.35f, new Vector4(1f, 1f, 1f, 0.2f), _viewport);
        
        GL.DepthFunc(DepthFunction.Less);
        
        //UI Windows
        if (!_mainMenu.IsVisible) _canvas.Draw(_viewport);
        
        //Menus
        _dayMenu.RenderMenu(_mouse, _viewport, _fonts, (float)args.Time);
        _pauseMenu.RenderMenu(_mouse, _viewport, _fonts, (float)args.Time);
        _mainMenu.RenderMenu(_mouse, _viewport, _fonts, (float)args.Time);
        
        
        Context.SwapBuffers();
        
        _fpsCount++;
        _fpsTimer += (float)args.Time;
        if (_fpsTimer >= 1f)
        {
            _fpsDisplay = _fpsCount;
            _fpsCount = 0;
            _fpsTimer = 0;
        }
    }
    
    public void DrawScene(Camera camera, Vector2i resolution, int scale, Framebuffer framebuffer, int visibleForId = 0)
    {
        if (_wireframeMode) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        
        GL.Viewport(0, 0, resolution.X / scale, resolution.Y / scale);
        //Scene
        GL.Disable(EnableCap.Blend);
        _gBuffer.Bind();
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        //Geometry Pass
        _geometryShader.ViewMat = camera.GetViewMatrix4();
        _geometryShader.ProjectionMat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(camera.Fov), (float) resolution.X / resolution.Y, 0.025f, 5000f);
        _geometryShader.WorldMat = _geometryShader.ViewMat * _geometryShader.ProjectionMat;
        
        _geometryShader.Draw(visibleForId);
        _gBuffer.Unbind();
        
        _gBuffer.DepthShader.Draw(resolution);
        
        if (_wireframeMode) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        
        //SSAO
        //_ssao.RenderSsao(_gBuffer, projectionMat, _viewport, _testVal, viewMat);
        
        //Collisions
        if (visibleForId != 1)
        {
            _collisionShader.Draw(_geometryShader.WorldMat, _geometryShader.ViewMat, resolution);
            _collisionShader.SetLookingAtUv(_gBuffer.NormalViewTexture.Handle, _postProcessShader, resolution);
        
            _outlineShader.RenderSilhouette(_geometryShader.WorldMat, _geometryShader.ViewMat, _collisionShader.LookingAtObject.Id);
        
            GL.Enable(EnableCap.DepthTest);
            _gBuffer.SetDefaultDepth(resolution);
            GL.DepthFunc(DepthFunction.Lequal);

            if (LaserButton.IsShooting)
            {
                _earth.BurnEffect.Draw();
                GL.Viewport(0, 0, resolution.X / scale, resolution.Y / scale);
            }
        }
        _earth.BurnEffect.Draw();
        GL.Viewport(0, 0, resolution.X / scale, resolution.Y / scale);
        
        //Lighting Pass
        framebuffer.Bind();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gBuffer.BindBufferTextures();
        _lightingShader.Draw(camera, _shadows, _ssao.BlurFramebuffer.AttachedTextures[0].Handle, visibleForId);
        
        GL.Enable(EnableCap.Blend);
        
        if (visibleForId != 1) _lightShader.Draw(_geometryShader.WorldMat, _geometryShader.ViewMat);
        if (visibleForId == 1) _laserShader.Draw(_geometryShader.WorldMat, _geometryShader.ViewMat);
        
        //Post Process
        framebuffer.Unbind();
        GL.Viewport(0, 0, resolution.X, resolution.Y);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;
        _deltaTime = _pauseMenu.IsVisible ? 0f : (float)args.Time;
        
        if (_moveEarthMode || _mainMenu.IsVisible)
        {
            var speed = _mainMenu.IsVisible ? 60f : ((SpeedPage)_station.Screens.Values.ToArray()[3].Pages[0]).ActualSpeed;
            _earth.MoveEarth(KeyboardState, _deltaTime,
                speed / Earth.Circumference * MathF.PI * 2f *
                (_debugMode ? _mainCamera.BoostSpeed : 1f),
                [_lights[LightTypes.Directional][0]], _debugMode);
            _earth.RotateEarth(_mainMenu.IsVisible ? 0f : _deltaTime, MathHelper.DegreesToRadians(((TurnPage)_station.Screens.Values.ToArray()[2].Pages[0]).TurnDegrees), [_lights[LightTypes.Directional][0]]);
            
            _station.Coordrinates = Earth.QuaternionToGpsCoords(_earth.EarthObject.Transform.Quaternion);
            ((ObjectiveScreen)_station.Screens.Values.ToList()[0]).SetCoords(_station.Coordrinates);

            _lights[LightTypes.Point][1].Transform.Position[Earth.EarthAxis] =
                _earth.EarthObject.Transform.Position[Earth.EarthAxis] +
                _earth.EarthObject.Transform.Scale[Earth.EarthAxis] + 3f +
                MathF.Max(0f, _earth.GetHeightOnEarth(_station.Coordrinates) - 0.5f) * (40f + Station.LaserRadius);
        }
        else _mainCamera.Move(KeyboardState, _deltaTime);

        _lights[LightTypes.Directional][0].Transform.Position = Vector3.Transform(
            -Vector3.UnitY,
            _lights[LightTypes.Directional][0].Transform.Quaternion.Normalized()
        ) * (_earth.EarthObject.Transform.Position[Earth.EarthAxis] + _earth.EarthObject.Transform.Scale[Earth.EarthAxis] * 2f + 2000f) - Vector3.UnitY *
        _earth.EarthObject.Transform.Scale;
        
        if (_mouse.DownButton == MouseButton.Left)
        {
            if (_station.Buttons.TryGetValue(_collisionShader.LookingAtObject.Id, out var button) && button.Type == ButtonTypes.Hold)
            {
                button.Activate(_mouse.IsDown, _deltaTime);
            }
        }
        
        if (_timerManager.CheckTimer("F11", (float)args.Time, KeyboardState.IsKeyDown(Keys.F11)))
        {
            _isFullscreen = !_isFullscreen;
            WindowState = _isFullscreen ? WindowState.Fullscreen : WindowState.Normal;

            if (!_isFullscreen && WindowState != WindowState.Fullscreen)
            {
                ClientSize = new Vector2i(1200, 800);
                CenterWindow();
            }
        }
        if (_timerManager.CheckTimer("CTRL_R", _deltaTime, KeyboardState.IsKeyDown(Keys.LeftControl) && KeyboardState.IsKeyDown(Keys.R)))
        {
            Reset();
            //_earth.SetRandomCoords();
            //_lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(new Vector3(MathHelper.DegreesToRadians(90f), 0f, MathHelper.DegreesToRadians(Random.Shared.NextSingle() * 360f)));
        }
        if (_mouse.IsDown) _mouse.PressLenght++;


        if (_mainMenu.IsVisible)
        {
            _debugMode = false;
            return;
        }
        
        
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) _mainCamera.SpeedBoost = true;
        if (KeyboardState.IsKeyReleased(Keys.LeftShift)) _mainCamera.SpeedBoost = false;

        if (_timerManager.CheckTimer("Esc", (float)args.Time, KeyboardState.IsKeyDown(Keys.Escape)))
        {
            _pauseMenu.IsVisible = !_pauseMenu.IsVisible && !_mainMenu.IsVisible;
        }
        if (_timerManager.CheckTimer("V", _deltaTime, KeyboardState.IsKeyDown(Keys.V)))
        {
            _wireframeMode = !_wireframeMode;
        }
        if (_timerManager.CheckTimer("P", _deltaTime, KeyboardState.IsKeyDown(Keys.P)))
        {
            _isPostProcess = !_isPostProcess;
        }
        if (_timerManager.CheckTimer("B", _deltaTime, KeyboardState.IsKeyDown(Keys.B)))
        {
            _moveEarthMode = !_moveEarthMode;
        }
        if (_timerManager.CheckTimer("F3", _deltaTime, KeyboardState.IsKeyDown(Keys.F3)))
        {
            _debugMode = !_debugMode;
        }
        
        var eo = _lights[LightTypes.Directional][0]; //_lights[LightTypes.Directional][0] : _station.Screens.Values.ToArray()[1].EngineObject
        var sens = (float)args.Time * 0.5f;
        if (KeyboardState.IsKeyDown(Keys.Insert)) eo.Transform.Quaternion = Quaternion.FromEulerAngles(sens, 0f, 0f) * eo.Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.Delete)) eo.Transform.Quaternion = Quaternion.FromEulerAngles(-sens, 0f, 0f) * eo.Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.Home)) eo.Transform.Quaternion = Quaternion.FromEulerAngles(0f, sens, 0f) * eo.Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.End)) eo.Transform.Quaternion = Quaternion.FromEulerAngles(0f, -sens, 0f) * eo.Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.PageUp)) eo.Transform.Quaternion = Quaternion.FromEulerAngles(0f, 0f, sens) * eo.Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.PageDown)) eo.Transform.Quaternion = Quaternion.FromEulerAngles(0f, 0f, -sens) * eo.Transform.Quaternion;

        if (KeyboardState.IsKeyDown(Keys.Up)) eo.Transform.Position.X += sens;
        if (KeyboardState.IsKeyDown(Keys.Down)) eo.Transform.Position.X -= sens;
        if (KeyboardState.IsKeyDown(Keys.Left)) eo.Transform.Position.Z += sens;
        if (KeyboardState.IsKeyDown(Keys.Right)) eo.Transform.Position.Z -= sens;
        if (KeyboardState.IsKeyDown(Keys.RightShift)) eo.Transform.Position.Y += sens;
        if (KeyboardState.IsKeyDown(Keys.RightControl)) eo.Transform.Position.Y -= sens;
        
        if (KeyboardState.IsKeyDown(Keys.Enter))
        {
            Console.WriteLine(eo.Transform.Position + ", " + eo.Transform.Quaternion.ToEulerAngles());
        }
        
        if (_timerManager.CheckTimer("+", _deltaTime, KeyboardState.IsKeyDown(Keys.KeyPadAdd)))
        {
            var map = (MapPage)_station.Screens.Values.ToList()[1].Pages[0];
            map.SetTargetPosition(map.EarthAngle * 180f / MathF.PI * new Vector2(-1f, 1f), 3); //SE -122.330441f, 47.610715f, AU 174.765252f, -36.849042f
        }
        if (_timerManager.CheckTimer("-", _deltaTime, KeyboardState.IsKeyDown(Keys.KeyPadSubtract)))
        {
        }

        if (!_mouse.IsDown)
        {
            _laserShader.Objects[0].IsVisible = KeyboardState.IsKeyDown(Keys.L);
            _lights[LightTypes.Point][1].Material.Color.X = MathF.Pow(Station.LaserRadius + 1f, 2.5f);
        }
        
        _lights[LightTypes.Point][1].IsLighting = _laserShader.Objects[0].IsVisible;
        if (_lights[LightTypes.Point][1].IsLighting)
        {
            _lights[LightTypes.Point][1].Transform.Position.X = (Random.Shared.NextSingle() * 2f - 1f) / 2f;
            _lights[LightTypes.Point][1].Transform.Position.Z = (Random.Shared.NextSingle() * 2f - 1f) / 2f;
        }
        
        if (LaserButton.IsShooting)
        {
            _lights[LightTypes.Point][1].Material.Color.X = MathF.Pow(Station.LaserRadius + 1f, 2.5f);
            
            Station.AllocationPercentage -= _deltaTime * (Station.BatteryMax / Station.AllocatedMax) * MathF.Pow(Station.LaserRadius, 0.5f) / 3f;
            _station.Buttons.Values.ToArray()[13].EngineObject.Material.Color = Vector4.Zero;
            _station.Buttons.Values.ToArray()[13].ButtonValue = 0f;
        }
        if (PrimeButton.IsPrimed)
        {
            Station.AllocationPercentage -= _deltaTime * (Station.BatteryMax / Station.AllocatedMax) / 750f;
        }
        if (AllocateButton.IsAllocating) AllocationGauge.AllocateBattery(_deltaTime, button: (AllocateButton)_station.Buttons.Values.ToArray()[19]);

        //_laserShader.Objects[0].Transform.Position[Earth.EarthAxis] = -100f / Station.LaserRadius * 5f;
        ((LogPage)_station.Screens.Values.ToArray()[0].Pages[2]).LogHits(LaserButton.IsShooting);

        if (_timerManager.CheckTimer("I", _deltaTime, KeyboardState.IsKeyDown(Keys.I))) _station.Screens.Values.ToList()[1].PageIndex += 1;
        if (_timerManager.CheckTimer("O", _deltaTime, KeyboardState.IsKeyDown(Keys.O))) _station.Screens.Values.ToList()[1].PageIndex -= 1;
        if (_timerManager.CheckTimer("Backspace", _deltaTime, KeyboardState.IsKeyDown(Keys.Backspace))) DayMenu.StayBlack = !DayMenu.StayBlack;
        if (_timerManager.CheckTimer("R", _deltaTime, KeyboardState.IsKeyDown(Keys.R)))
        {
            _mainCamera.Transform.Position = new Vector3(-0.52f, 1.15f, 0f);
            Station.BatteryPercentage = 1f;
            if (_debugMode) UpgradePage.Money += 10f;
        }
        
        //_lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(new Vector3(MathHelper.DegreesToRadians(115f), 0f, MathHelper.DegreesToRadians(Random.Shared.NextSingle() * 360f)));
        
        if (CursorState == CursorState.Grabbed && !DayMenu.StayBlack) _mainCamera.UpdateCameraFront(_moveEarthMode);
    }

    public static float[] ReadPixel(int x, int y)
    {
        var pixel = new float[4];
        GL.ReadPixels(x, y, 1, 1, PixelFormat.Rgba, PixelType.Float, pixel);
        return pixel;
    }

    public void Reset(bool resetMoney = false)
    {
        if (resetMoney) UpgradePage.Money = 1.42f;
        _station.Reset();
        _earth.SetRandomCoords();
        ((MapPage)_station.Screens.Values.ToArray()[1].Pages[0]).SetCoords(Earth.QuaternionToGpsCoords(_earth.EarthObject.Transform.Quaternion).Yx * new Vector2(-1f, 1f));
        
        _mainCamera.Front = new Vector3(-1f, 0f, 0f);
        
        _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(new Vector3(MathHelper.DegreesToRadians(90f), 0f, MathHelper.DegreesToRadians(Random.Shared.NextSingle() * 360f)));
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (!IsFocused) return;
        
        _mouse.ScreenPosition.X = (int)e.X;
        _mouse.ScreenPosition.Y = _viewport.Y - (int)e.Y;

        if (_pauseMenu.IsVisible || _mainMenu.IsVisible || DayMenu.StayBlack)
        {
            CursorState = CursorState.Normal;
        }
        else
        {
            CursorState = CursorState.Grabbed;
        
            _mainCamera.Mouse.X = e.X + _viewport[0] / 2f;
            _mainCamera.Mouse.Y = -(e.Y + _viewport[1] / 2f);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsFocused) return;
        
        if (_station.Buttons.TryGetValue(_collisionShader.LookingAtObject.Id, out var button) && button.Type == ButtonTypes.Continuous)
        {
            button.Activate(e.OffsetY, _deltaTime);
        }
        
        
        if (_mouse.IsDown)
        {
            _mainCamera.ZoomFov = Math.Min(0.9f, Math.Max(0.1f, _mainCamera.ZoomFov - e.OffsetY / 20f));
            _mainCamera.Fov = _mainCamera.BaseFov * _mainCamera.ZoomFov;
        }
        else if (_debugMode)
        {
            //((SpeedPage)_station.Screens.Last().Value.Pages[0]).TargetSpeed *= MathF.Pow(1.25f, e.OffsetY);
            _mainCamera.BoostSpeed *= MathF.Pow(1.25f, e.OffsetY);
        }
    }
    
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _mouse.IsDown = false;
        _mouse.DownButton = e.Button;
        _mouse.PressLenght = 0;

        if (e.Button == MouseButton.Left)
        {
            if (_station.Buttons.TryGetValue(_collisionShader.LookingAtObject.Id, out var button) && button.Type == ButtonTypes.Press)
            {
                button.Activate(false, _deltaTime);
            }
        }
        if (e.Button == MouseButton.Right)
        {
            _mainCamera.Fov = _mainCamera.BaseFov;
        }

        LaserButton.IsShooting = false;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        _mouse.IsDown = true;
        _mouse.DownButton = e.Button;

        if (e.Button == MouseButton.Left)
        {
            if (_station.Buttons.TryGetValue(_collisionShader.LookingAtObject.Id, out var button) && button.Type == ButtonTypes.Press)
            {
                button.Activate(true, _deltaTime);
            }
        }
        if (e.Button == MouseButton.Right)
        {
            _mainCamera.Fov = _mainCamera.BaseFov * _mainCamera.ZoomFov;
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    { 
        GL.Viewport(0, 0, e.Width, e.Height);

        var viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        _viewport.X = viewport[2];
        _viewport.Y = viewport[3];
        
        ResizeGame(_viewport, _renderScale);
        _beenResized = true;
    }

    private void ResizeGame(Vector2i resolution, int scale)
    {
        _mainCamera.UpdateSensitivityByAspect(resolution);

        var viewScaled = resolution / scale;
        _postProcessShader.Resize(viewScaled);
        _gBuffer.Resize(viewScaled);
        _ssao.Framebuffer.AttachedTextures[0].Resize(viewScaled);
        _ssao.BlurFramebuffer.AttachedTextures[0].Resize(viewScaled);
        _outlineShader.Resize(viewScaled);
    }
}