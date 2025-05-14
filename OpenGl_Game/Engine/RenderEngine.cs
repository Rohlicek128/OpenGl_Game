using OpenGl_Game.Engine.Editor;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.PostProcess;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.Objects.Collisions;
using OpenGl_Game.Engine.UI;
using OpenGl_Game.Engine.UI.Obsolete;
using OpenGl_Game.Game;
using OpenGl_Game.Game.Buttons;
using OpenGl_Game.Game.Screens;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using FontMap = OpenGl_Game.Engine.Graphics.UI.Text.FontMap;

namespace OpenGl_Game.Engine;

public class RenderEngine : GameWindow
{
    public const string DirectoryPath = @"C:\Files\Code\.NET\OpenGl_Game\OpenGl_Game\";
    //public const string DirectoryPath = @"C:\Users\adam\RiderProjects\OpenGl_Game\OpenGl_Game\";
    public const uint PrimitiveIndex = uint.MaxValue;
    public const int MaxObjectIds = 255;

    private Camera _mainCamera;
    private float _fpsTimer;
    private int _fpsCount, _fpsDisplay;
    
    private bool _isRotation = true;
    private bool _isMouseGrabbed = true;
    private bool _wireframeMode = false;
    private bool _isPostProcess = true;
    private bool _moveEarthMode = true;
    private bool _isFullscreen = false;
    private bool _debugMode = false;
    private bool _testBool = false;

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

    private List<TexturesPbr> _textures;
    private CubeMap _skybox;

    private EngineObject[] _objects;
    private Dictionary<LightTypes, List<Light>> _lights;

    private TimerManager _timerManager;
    private WindowManager _windowManager;
    private EditorManager _editorManager;

    private float _testVal = 1f;
    
    private Earth _earth;
    private Station _station;
    
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

        _textures = [];

        _fonts = new Dictionary<string, FontMap>();
        _timerManager = new TimerManager(150);
        _editorManager = new EditorManager();
        
        _mainCamera = new Camera(new Vector3(-0.9f, 0.85f, 0f), 1f, 0.09f, 95f, 0.025f, 2_000_000f);
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
        _skybox = new CubeMap("black1x1.png", false);
        
        _textures.Add(new TexturesPbr());
        _textures[0].AddTexture(TextureTypes.Diffuse, new Texture("container.png", 0));
        _textures[0].AddTexture(TextureTypes.Specular, new Texture("white1x1.png", 1));
        _textures[0].AddTexture(TextureTypes.Normal, new Texture("normal_test.jpg", 2));
        
        _textures.Add(new TexturesPbr());
        _textures[1].AddTexture(TextureTypes.Diffuse, new Texture("concrete_pavers_diff_2k.jpg", 0));
        _textures[1].AddTexture(TextureTypes.Specular, new Texture("concrete_pavers_rough_2k.jpg", 1));
        
        //Fonts
        var text = EngineObject.CreateEmpty();
        text.MeshData.Vertices = new float[6 * 4];
        _fontShader = new FontShader(text);
        
        _fonts.Add("Cascadia", new FontMap("CascadiaCode.ttf", _fontShader));
        _fonts.Add("ATName", new FontMap("ATNameSansTextTrial-ExtraBold.otf", _fontShader));
        _fonts.Add("Pixel", new FontMap("04B_03_.TTF", _fontShader));
        _fonts.Add("Wide", new FontMap("Vipnagorgialla Bd.otf", _fontShader));
        _fonts.Add("Brigends", new FontMap("brigends.ttf", _fontShader));
        
        //Materials
        var material1 = new Material(new Vector3(1f, 0f, 0f));
        var material2 = new Material(
            new Vector3(0.2f, 0.1f, 1f),
            new Vector3(1f, 0.8f, 0.65f),
            new Vector3(0.5f, 0.3f, 0.1f),
            256f
        );
        
        //Objects
        var cube = new EngineObject(
            "Chill Cube", 
            new Transform(new Vector3(0f, 0f, 0f)), 
            MeshConstructor.CreateCube(),
            material1
        );
        cube.Material.Color = new Vector3(1f, 1f, 0f);
        var floor = new EngineObject(
            "Floor", 
            new Transform(new Vector3(0f, -0.501f, 0f)), 
            MeshConstructor.CreatePlane(1f),
            _textures[1]
        );
        floor.Material.Shininess = 32f;
        floor.Material.Specular = new Vector3(0.5f);
        floor.Transform.Scale *= 20f;
        floor.Textures.Scaling = 0.5f;
        
        /*var teapot = MeshConstructor.LoadFromFile(@"rungholt\house.obj", verticesAttribs);
        teapot.Transform.Scale /= 80f;
        teapot.Transform.Position *= teapot.Transform.Scale;
        teapot.Transform.Position.Z += 3f;
        teapot.Material.Color = new Vector3(0.8f, 0.05f, 0.2f);
        teapot.Material.Shininess = 600f;*/

        //var terrain = new Terrain("new-zealand-height-map.jpg", verticesAttribs);
        _earth = new Earth(new Transform(new Vector3(0f), new Vector3(0f, MathF.PI, 0f), new Vector3(6378 / 2f)), 400, 0.025f);
        _earth.EarthObject.Transform.Position = new Vector3(0f, -_earth.EarthObject.Transform.Scale[Earth.EarthAxis] * 1.0639f, 0f);
        _earth.CollisionSphere.Transform.Position = _earth.EarthObject.Transform.Position;
        //_earth.EarthObject.Transform.Position = new Vector3(-MathF.Cos(MathHelper.DegreesToRadians(35f)) * _earth.EarthObject.Transform.Scale.X * 1.0639f, MathF.Sin(MathHelper.DegreesToRadians(35f)) * _earth.EarthObject.Transform.Scale.X * 1.0639f, 0f);
        _station = new Station();
        
        var laser = new EngineObject(
            "Laser",
            new Transform(Vector3.Zero, new Vector3(0f, MathF.PI / 2f, 0f), new Vector3(1f, 1f, 1f)),
            MeshConstructor.LoadObjFromFileAssimp(@"Station\laserCylinder.obj"),
            new Material(new Vector3(20f, 0f, 0f))
        );
        laser.Transform.Scale[Earth.EarthAxis] = 200f;
        laser.Transform.Position[Earth.EarthAxis] = -100;
        //laser.Transform.Quaternion = Quaternion.FromEulerAngles(12f, 0f, 0f) * laser.Transform.Quaternion;
        
        //lights
        var dirLight = new Light(
            "Dir light", 
            new Transform(Vector3.UnitY * 12f), 
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f)),
            new Vector3(0.2f, 1f, 1f),
            new Vector3(1.0f, 0.045f * 10, 0.0075f * 10),
            LightTypes.Directional
        );
        dirLight.Transform.Scale *= 20f;
        //dirLight.Transform.Rotation = new Vector3(1.433031f, -1.5707964f, 2.588305f);
        dirLight.Transform.SetRotationByPosition(Vector3.Zero);
        //dirLight.IsVisible = false;
        var pointLight = new Light(
            "Point light", 
            new Transform(new Vector3(-1.9560018f, 2.9544713f, 0f)), 
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f)), //1f, 0.95f, 0.81f
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.45f, 0.75f),
            LightTypes.Point
        );
        pointLight.Transform.Scale *= 0.2f;
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
        
        _objects = [
            _earth.EarthObject, _station.StationObject,
            .._station.Buttons.Values.Select(b => b.EngineObject),
            .._station.Screens.Values.Select(b => b.EngineObject)
        ];
        
        _lights = new Dictionary<LightTypes, List<Light>>
        {
            { LightTypes.Directional, [dirLight] },
            { LightTypes.Point, [pointLight, laserLight] }
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
        _canvas = new Canvas(reticule);
        
        _windowManager = new WindowManager();
        
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        GL.Enable(EnableCap.PrimitiveRestart);
        GL.PrimitiveRestartIndex(PrimitiveIndex);
        
        //GL.Enable(EnableCap.ProgramPointSize);
        //GL.PointSize(10f);

        Console.WriteLine("start");
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
        foreach (var textures in _textures) textures.DeleteAll();
        foreach (var font in _fonts) font.Value.Delete();
        //_windowManager.Delete();
        
        Console.WriteLine("end");
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        //Aim
        var nav = (NavigationScreen)_station.Screens.Values.ToList()[1];
        if (nav.IsTurnOn)
        {
            Resize(nav.ScreenResolution, 1);
            DrawScene(nav.AimCamera, nav.ScreenResolution, 1, nav.SceneFramebuffer);
            Resize(_viewport, _renderScale);
        }
        
        //Scene
        //Shadows
        GL.Enable(EnableCap.DepthTest);
        _shadows.Draw(_lights[LightTypes.Directional][0], _mainCamera, _viewport, _geometryShader);
        
        DrawScene(_testBool ? nav.AimCamera : _mainCamera, _viewport, _renderScale, _postProcessShader.Framebuffer);
        _postProcessShader.Draw(_isPostProcess ? -1 : _gBuffer.NormalsTexture.Handle); //_shadows.TextureHandle : _gBuffer.NormalsTexture.Handle
        _postProcessShader.DrawShaders();
        GL.Disable(EnableCap.DepthTest);
        foreach (var screen in _station.Screens) screen.Value.RenderScreen(_collisionShader, _mouse, _viewport / _renderScale, _fonts);
        
        
        //var pixel = ReadPixel(_viewport.X / 2, _viewport.Y / 2);
        //Console.WriteLine(pixel[0] + ", " + pixel[1] + ", " + pixel[2] + ", " + pixel[3]);
        
        //Text
        _fonts["Brigends"].DrawText(_fpsDisplay + " fps", new Vector2(25f, _viewport.Y - 60f), 0.75f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText(_mainCamera.Fov + " fov", new Vector2(25f, _viewport.Y - 110f), 0.75f, new Vector4(1f), _viewport);
        if (_debugMode)
        {
            var tempCam = _testBool ? nav.AimCamera : _mainCamera;
            _fonts["Pixel"].DrawText("PLAYER X: "+ (MathF.Floor(tempCam.Transform.Position.X * 1000f) / 1000f) + " Y: " + (MathF.Floor(tempCam.Transform.Position.Y * 1000f) / 1000f) + " Z: "+ (MathF.Floor(tempCam.Transform.Position.Z * 1000f) / 1000f),
                new Vector2(25f, _viewport.Y - 160f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("FRONT  X: "+ (MathF.Floor(tempCam.Front.X * 1000f) / 1000f) + " Y: " + (MathF.Floor(tempCam.Front.Y * 1000f) / 1000f) + " Z: "+ (MathF.Floor(tempCam.Front.Z * 1000f) / 1000f),
                new Vector2(25f, _viewport.Y - 210f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("Boost: " + _mainCamera.BoostSpeed, new Vector2(25f, _viewport.Y - 260f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("FOV: " + _mainCamera.Fov + ", Zoom: " + _mainCamera.ZoomFov, new Vector2(25f, _viewport.Y - 310f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("VALUE: Y: " + _mainCamera.Yaw + ", P: " + _mainCamera.Pitch, new Vector2(25f, _viewport.Y - 360f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("Altitude: " + Math.Floor(-(_earth.EarthObject.Transform.Scale[Earth.EarthAxis] + _earth.EarthObject.Transform.Position[Earth.EarthAxis]) * 2f) + " km", new Vector2(25f, _viewport.Y - 410f), 0.5f, new Vector4(1f), _viewport);
            
            //var hit = _earth.CollisionSphere.CheckCollision(new Ray(_camera.Transform.Position, _camera.Front));
            //if (!hit.HasHit) _fonts["Pixel"].DrawText("RAY: NO COLLISION", new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
            //else _fonts["Pixel"].DrawText("RAY: HIT, POS: " + hit.HitPos + ", LEN: " + hit.Distance, new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
            
            _fonts["Pixel"].DrawText("LUV: " + _collisionShader.LookingAtUv.X + ", " + _collisionShader.LookingAtUv.Y, new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("Coords: " + Earth.QuaternionToGpsCoords(_earth.EarthObject.Transform.Quaternion), new Vector2(25f, _viewport.Y - 510f), 0.5f, new Vector4(1f), _viewport);
            _fonts["Pixel"].DrawText("TESTBOOL: " + _testBool, new Vector2(25f, _viewport.Y - 560f), 0.5f, new Vector4(1f), _viewport);
            
            _fonts["Pixel"].DrawText(_collisionShader.LookingAtObject.Name + " [" + _collisionShader.LookingAtObject.Id + "]", new Vector2(_viewport.X / 2f + 10f, _viewport.Y / 2f + 10f), 0.4f, new Vector4(1f), _viewport);
        }
        _fonts["Pixel"].DrawText("FROM ORBIT v0.2.4", new Vector2(_viewport.X - 255f, _viewport.Y - 30f), 0.35f, new Vector4(1f, 1f, 1f, 0.2f), _viewport);
        
        GL.DepthFunc(DepthFunction.Less);
        
        //UI Windows
        _canvas.Draw(_viewport);
        //_windowManager.DrawWindows(_viewport, _fonts);
        
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

    public void DrawScene(Camera camera, Vector2i resolution, int scale, Framebuffer framebuffer)
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
        
        _geometryShader.Draw();
        _gBuffer.Unbind();
        
        _gBuffer.DepthShader.Draw(resolution);
        
        if (_wireframeMode) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        
        //SSAO
        //_ssao.RenderSsao(_gBuffer, projectionMat, _viewport, _testVal, viewMat);
        
        //Collisions
        _collisionShader.Draw(_geometryShader.WorldMat, _geometryShader.ViewMat, resolution);
        _collisionShader.SetLookingAtUv(_gBuffer.NormalViewTexture.Handle, _postProcessShader, resolution);
        
        _outlineShader.RenderSilhouette(_geometryShader.WorldMat, _geometryShader.ViewMat, _collisionShader.LookingAtObject.Id);
        
        GL.Enable(EnableCap.DepthTest);
        _gBuffer.SetDefaultDepth(resolution);
        GL.DepthFunc(DepthFunction.Lequal);
        
        //Lighting Pass
        framebuffer.Bind();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gBuffer.BindBufferTextures();
        _lightingShader.Draw(camera, _shadows, _ssao.BlurFramebuffer.AttachedTextures[0].Handle);
        
        GL.Enable(EnableCap.Blend);
        
        _lightShader.Draw(_geometryShader.WorldMat, _geometryShader.ViewMat);
        _laserShader.Draw(_geometryShader.WorldMat, _geometryShader.ViewMat);
        
        //Post Process
        framebuffer.Unbind();
        GL.Viewport(0, 0, resolution.X, resolution.Y);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (!IsFocused) return;
        
        if (_moveEarthMode)
        {
            _earth.MoveEarth(KeyboardState, (float)args.Time, _mainCamera.BoostSpeed, [_lights[LightTypes.Directional][0]]);
            _lights[LightTypes.Point][1].Transform.Position[Earth.EarthAxis] = _earth.EarthObject.Transform.Position[Earth.EarthAxis] + _earth.EarthObject.Transform.Scale[Earth.EarthAxis] + 3f;
        }
        else _mainCamera.Move(KeyboardState, (float)args.Time);

        //_lights[LightTypes.Directional][0].Transform.Position =
        //    -_lights[LightTypes.Directional][0].Transform.Quaternion.ToEulerAngles() *
        //    _earth.EarthObject.Transform.Scale.X;

        _lights[LightTypes.Directional][0].Transform.Position = Vector3.Transform(
            -Vector3.UnitY,
            _lights[LightTypes.Directional][0].Transform.Quaternion.Normalized()
        ) * (_earth.EarthObject.Transform.Position[Earth.EarthAxis] + _earth.EarthObject.Transform.Scale[Earth.EarthAxis] * 2f + 2000f) - Vector3.UnitY *
        _earth.EarthObject.Transform.Scale;
        
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) _mainCamera.SpeedBoost = true;
        if (KeyboardState.IsKeyReleased(Keys.LeftShift)) _mainCamera.SpeedBoost = false;

        if (_timerManager.CheckTimer("Esc", (float)args.Time, KeyboardState.IsKeyDown(Keys.Escape)))
        {
            _isMouseGrabbed = !_isMouseGrabbed;
        }
        if (_timerManager.CheckTimer("M", (float)args.Time, KeyboardState.IsKeyDown(Keys.M)))
        {
            _isRotation = !_isRotation;
        }
        if (_timerManager.CheckTimer("V", (float)args.Time, KeyboardState.IsKeyDown(Keys.V)))
        {
            _wireframeMode = !_wireframeMode;
        }
        if (_timerManager.CheckTimer("P", (float)args.Time, KeyboardState.IsKeyDown(Keys.P)))
        {
            _isPostProcess = !_isPostProcess;
        }
        if (_timerManager.CheckTimer("B", (float)args.Time, KeyboardState.IsKeyDown(Keys.B)))
        {
            _moveEarthMode = !_moveEarthMode;
        }
        if (_timerManager.CheckTimer("F11", (float)args.Time, KeyboardState.IsKeyDown(Keys.F11)))
        {
            _isFullscreen = !_isFullscreen;
            WindowState = _isFullscreen ? WindowState.Fullscreen : WindowState.Normal;
        }
        if (_timerManager.CheckTimer("F3", (float)args.Time, KeyboardState.IsKeyDown(Keys.F3)))
        {
            _debugMode = !_debugMode;
        }
        
        if (_timerManager.CheckTimer("K", (float)args.Time, KeyboardState.IsKeyDown(Keys.K)))
        {
            _geometryShader.AddEngineObject(new EngineObject(
                "box", 
                new Transform(new Vector3(5f, 0f, 0f)), 
                MeshConstructor.CreateCube(),
                new Material(new Vector3(0f, 0f, 1f))
            ), _geometryShader.Attributes);
            Console.WriteLine("added");
        }
        
        var eo = _lights[LightTypes.Directional][0]; //_lights[LightTypes.Directional][0] : _station.Screens.First().Value.EngineObject
        var sens = (float)args.Time * 1f;
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
        if (KeyboardState.IsKeyDown(Keys.RightAlt)) eo.Transform.Position.Y -= sens;
        
        if (KeyboardState.IsKeyDown(Keys.Enter))
        {
            Console.WriteLine(eo.Transform.Position + ", " + eo.Transform.Quaternion.ToEulerAngles());
        }
        
        if (_timerManager.CheckTimer("+", (float)args.Time, KeyboardState.IsKeyDown(Keys.G)))
        {
            //_camera.Fov = Math.Min(160f, _camera.Fov + 2f);
            //_camera.UpdateSensitivityByFov();
            //_testVal += 0.25f;
            _mainCamera.Sensitivity += 0.1f;
        }
        if (_timerManager.CheckTimer("-", (float)args.Time, KeyboardState.IsKeyDown(Keys.H)))
        {
            //_camera.UpdateSensitivityByFov();
            //_camera.Fov = Math.Max(1f, _camera.Fov - 2f);
            //_testVal -= 0.25f;
            _mainCamera.Sensitivity -= 0.1f;
        }

        if (!_mouse.IsDown) _laserShader.Objects[0].IsVisible = KeyboardState.IsKeyDown(Keys.L);
        
        _lights[LightTypes.Point][1].IsLighting = _laserShader.Objects[0].IsVisible;
        if (_lights[LightTypes.Point][1].IsLighting)
        {
            _lights[LightTypes.Point][1].Transform.Position.X = (Random.Shared.NextSingle() * 2f - 1f) / 2f;
            _lights[LightTypes.Point][1].Transform.Position.Z = (Random.Shared.NextSingle() * 2f - 1f) / 2f;
        }

        if (_timerManager.CheckTimer("I", (float)args.Time, KeyboardState.IsKeyDown(Keys.I))) _postProcessShader.Banding += 1;
        if (_timerManager.CheckTimer("O", (float)args.Time, KeyboardState.IsKeyDown(Keys.O))) _postProcessShader.Banding -= 1;
        if (_timerManager.CheckTimer("Backspace", (float)args.Time, KeyboardState.IsKeyDown(Keys.Backspace))) _testBool = !_testBool;
        
        _mainCamera.UpdateCameraFront();
        if (_mouse.IsDown) _mouse.PressLenght++;
    }

    public static float[] ReadPixel(int x, int y)
    {
        var pixel = new float[4];
        GL.ReadPixels(x, y, 1, 1, PixelFormat.Rgba, PixelType.Float, pixel);
        return pixel;
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (!IsFocused) return;
        
        _mouse.ScreenPosition.X = (int)e.X;
        _mouse.ScreenPosition.Y = _viewport.Y - (int)e.Y;

        if (!_isMouseGrabbed)
        {
            CursorState = CursorState.Normal;

            foreach (var window in _windowManager.Windows)
            {
                if (window.MoveWindow(_mouse))
                {
                    //Cursor = MouseCursor.PointingHand;
                }
            }
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
            button.Activate(e.OffsetY);
        }
        
        
        if (_mouse.IsDown)
        {
            _mainCamera.ZoomFov = Math.Min(0.9f, Math.Max(0.1f, _mainCamera.ZoomFov - e.OffsetY / 20f));
            _mainCamera.Fov = _mainCamera.BaseFov * _mainCamera.ZoomFov;
        }
        else
        {
            _mainCamera.BoostSpeed = Math.Min(1000f, Math.Max(_mainCamera.BaseSpeed, _mainCamera.BoostSpeed + e.OffsetY * 3f));
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
                button.Activate(false);
            }
        }
        if (e.Button == MouseButton.Right)
        {
            _mainCamera.Fov = _mainCamera.BaseFov;
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        _mouse.IsDown = true;
        _mouse.DownButton = e.Button;

        if (e.Button == MouseButton.Left)
        {
            if (_station.Buttons.TryGetValue(_collisionShader.LookingAtObject.Id, out var button) && button.Type == ButtonTypes.Press)
            {
                button.Activate(true);
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
        
        Resize(_viewport, _renderScale);
    }

    private void Resize(Vector2i resolution, int scale)
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