using OpenGl_Game.Engine.Editor;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.PostProcess;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.Objects.Collisions;
using OpenGl_Game.Engine.UI;
using OpenGl_Game.Game;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using FontMap = OpenGl_Game.Engine.Graphics.UI.Text.FontMap;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Engine;

public class RenderEngine : GameWindow
{
    //public const string DirectoryPath = @"C:\Files\Code\.NET\OpenGl_Game\OpenGl_Game\";
    public const string DirectoryPath = @"C:\Users\adam\RiderProjects\OpenGl_Game\OpenGl_Game\";

    private Camera _camera;
    private float _anim, _fpsTimer;
    private int _fpsCount, _fpsDisplay;
    
    private bool _isRotation = true;
    private bool _isMouseGrabbed = true;
    private bool _wireframeMode = false;
    private bool _isPostProcess = true;
    private bool _MoveEarthMode = true;

    private Mouse _mouse;
    private Vector2i _viewport;
    private int _renderScale;
    
    private ShaderProgram _geometryShader;
    private ShaderProgram _lightingShader;
    private ShaderProgram _lightShader;
    private ShaderProgram _skyboxProgram;
    private GBuffer _gBuffer;
    
    private PostProcess _postProcess;
    
    private Dictionary<string, FontMap> _fonts;
    private Canvas _canvas;

    private ShadowMap _shadows;
    private Ssao _ssao;

    private List<TexturesPbr> _textures;
    private CubeMap _skybox;

    private EngineObject[] _objects;
    private Dictionary<LightTypes, List<Light>> _lights;
    public const uint PrimitiveIndex = uint.MaxValue;

    private TimerManager _timerManager;
    private WindowManager _windowManager;
    private EditorManager _editorManager;

    private CollisionBox _collisionBox;

    private float _testVal = 1f;
    
    private Earth _earth;
    private Station _station;
    
    public RenderEngine(int width, int height, string title) : base(
            GameWindowSettings.Default, 
            new NativeWindowSettings()
            {
                Title = title,
                Size = new Vector2i(width, height),
                WindowBorder = WindowBorder.Resizable,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 2)
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

        _collisionBox = new CollisionBox(new Transform(new Vector3(0f)));
        
        _camera = new Camera(new Vector3(0f, 0f, -3f), 3f, 0.03f, 95f);
        _camera.UpdateSensitivityByAspect(_viewport);
    }

    public WindowIcon CreateWindowIcon(string path)
    {
        //StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromStream(File.OpenRead(DirectoryPath + @"Assets\" + path), ColorComponents.RedGreenBlueAlpha);
        
        return new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, image.Data));
    }

    protected override void OnLoad()
    {
        IsVisible = true;
        
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
        var fontProgram = new ShaderProgram([
            new Shader(@"TextShaders\textShader.vert", ShaderType.VertexShader),
            new Shader(@"TextShaders\textShader.frag", ShaderType.FragmentShader)
        ], [text], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)], BufferUsage.DynamicDraw);
        
        _fonts.Add("Cascadia", new FontMap("CascadiaCode.ttf", fontProgram));
        _fonts.Add("ATName", new FontMap("ATNameSansTextTrial-ExtraBold.otf", fontProgram));
        _fonts.Add("Pixel", new FontMap("04B_03_.TTF", fontProgram));
        _fonts.Add("Wide", new FontMap("Vipnagorgialla Bd.otf", fontProgram));
        
        //Materials
        var material1 = new Material(new Vector3(1f, 0f, 0f));
        var material2 = new Material(
            new Vector3(0.2f, 0.1f, 1f),
            new Vector3(1f, 0.8f, 0.65f),
            new Vector3(0.5f, 0.3f, 0.1f),
            256f
        );
        
        //Objects
        var playerBox = new EngineObject(
            "Player", 
            new Transform(new Vector3(0f, 0f, 0f)), 
            MeshConstructor.CreateCube(),
            material1
        );
        playerBox.Transform.Scale = new Vector3(0.35f);
        var box = new EngineObject(
            "box", 
            new Transform(new Vector3(0f, 0f, 0f)), 
            MeshConstructor.CreateCube(),
            _textures[0]
        );
        box.Material.Color = new Vector3(1f);
        var rollingCube = new EngineObject(
            "Chill Cube", 
            new Transform(new Vector3(0f, 0f, 0f)), 
            MeshConstructor.CreateCube(),
            material1
        );
        rollingCube.Material.Color = new Vector3(1f, 1f, 0f);
        var rotatingCube = new EngineObject(
            "Chill Cube #2", 
            new Transform(new Vector3(0f, 0f, -5f)), 
            MeshConstructor.CreateCube(),
            _textures[0]
        );
        var anotherCube = new EngineObject(
            "Another Cube", 
            new Transform(new Vector3(-7f, 1f, 5f)),
            MeshConstructor.CreateCube(),
            material2
        );
        anotherCube.Transform.Scale = new Vector3(0.3f, 1f, 0.5f);
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
        _earth = new Earth(new Transform(new Vector3(0f), new Vector3(0f, MathF.PI, 0f), new Vector3(6378 / 6f)), 200, 0.025f);
        _earth.EarthObject.Transform.Position = new Vector3(-_earth.EarthObject.Transform.Scale.X * 1.0639f, 0f, 0f);
        _earth.CollisionSphere.Transform.Position = _earth.EarthObject.Transform.Position;
        //_earth.EarthObject.Transform.Position = new Vector3(-MathF.Cos(MathHelper.DegreesToRadians(35f)) * _earth.EarthObject.Transform.Scale.X * 1.0639f, MathF.Sin(MathHelper.DegreesToRadians(35f)) * _earth.EarthObject.Transform.Scale.X * 1.0639f, 0f);
        _station = new Station(_camera, []);
        
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
            new Transform(new Vector3(-4f, 5f, 0f)), 
            MeshConstructor.CreateCube(),
            new Material(new Vector3(1f, 0.95f, 0.81f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.45f, 0.075f),
            LightTypes.Point
        );
        pointLight.Transform.Scale *= 0.2f;
        var pointLight2 = new Light(
            "Point light2", 
            new Transform(new Vector3(-6f, 1.5f, 1f)), 
            MeshConstructor.CreateCube(),
            new Material(new Vector3(0f, 1f, 0f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.045f, 0.0075f),
            LightTypes.Point
        );
        pointLight2.Transform.Scale *= 0.25f;
        
        _objects = [_earth.EarthObject, _station.StationObject];
        
        _lights = new Dictionary<LightTypes, List<Light>>
        {
            { LightTypes.Directional, [dirLight] },
            { LightTypes.Point, [pointLight] }
        };
        
        //_editorManager.Save(playerBox);
        
        //Geometry Shader
        _geometryShader = new ShaderProgram([
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"geometryShaders\geometryShader.frag", ShaderType.FragmentShader)
        ], _objects.ToList(), verticesAttribs, addTangent:true);
        //Lighting Shader
        var screenQuad = EngineObject.CreateEmpty();
        screenQuad.MeshData.Vertices = MeshConstructor.CreateRenderQuad();
        _lightingShader = new ShaderProgram([
            new Shader(@"gLightingShaders\lightingShader.vert", ShaderType.VertexShader),
            new Shader(@"gLightingShaders\lightingShader.frag", ShaderType.FragmentShader)
        ], [screenQuad], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)]);
        
        //Light Shader
        _lightShader = new ShaderProgram([
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"LightShaders\lightShader.frag", ShaderType.FragmentShader)
        ], [..Light.LightsDicToList(_lights)], verticesAttribs, addTangent:true);
        
        //Skybox
        var skybox = new EngineObject(
            "Skybox", 
            new Transform(new Vector3(0f)), 
            _skybox.MeshData,
            new Material(new Vector3(1f))
        );
        _skyboxProgram = new ShaderProgram([
            new Shader(@"SkyboxShaders\skybox.vert", ShaderType.VertexShader),
            new Shader(@"SkyboxShaders\skybox.frag", ShaderType.FragmentShader)
        ], [skybox], [new VertexAttribute(VertexAttributeType.Position, 3)]);
        
        //Post Process
        _postProcess = new PostProcess([
            new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
            new Shader(@"PostProcessShaders\postProcessShader.frag", ShaderType.FragmentShader)
        ], _viewport / _renderScale);
        
        //gBuffer
        _gBuffer = new GBuffer(_viewport);
        
        //Shadows
        //_shadows = new ShadowMap(new Vector2i(8192, 8192), 1100f, 300f, new Vector2(0.01f, 400f), _geometryShader);
        _shadows = new ShadowMap(new Vector2i(4096, 4096), 25f, 9f, new Vector2(1f, 20f), _geometryShader);
        _ssao = new Ssao([
                new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
                new Shader(@"shadersSSAO\ssaoShader.frag", ShaderType.FragmentShader)], [
                new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
                new Shader(@"shaderBlurSSAO\ssaoBlurShader.frag", ShaderType.FragmentShader)
        ], _viewport, 128, 4);
        
        //UI
        _canvas = new Canvas();
        
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
        
        Console.WriteLine("start");
    }

    protected override void OnUnload()
    {
        _geometryShader.Delete();
        _lightShader.Delete();
        _skyboxProgram.Delete();
        
        _postProcess.Delete();
        _shadows.Delete();
        _gBuffer.DeleteGBuffer();
        _ssao.Delete();
        foreach (var textures in _textures) textures.DeleteAll();
        foreach (var font in _fonts) font.Value.Delete();
        _windowManager.Delete();
        
        Console.WriteLine("end");
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        //Shadows
        GL.Enable(EnableCap.DepthTest);
        GL.CullFace(TriangleFace.Front);
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shadows.RenderDepthMap(_lights[LightTypes.Directional][0], _viewport, _geometryShader, _camera);
        GL.CullFace(TriangleFace.Back);
        
        if (_wireframeMode) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        
        GL.Viewport(0, 0, _viewport.X / _renderScale, _viewport.Y / _renderScale);
        //Scene
        GL.Disable(EnableCap.Blend);
        _gBuffer.Bind();
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        //Geometry Pass
        var viewMat = _camera.GetViewMatrix4();
        var projectionMat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov), (float) _viewport[0] / _viewport[1], 0.025f, 5000f);
        var worldMat = viewMat * projectionMat;
        
        _geometryShader.DrawGeometryMesh(worldMat, projectionMat, viewMat);
        _gBuffer.Unbind();
        
        if (_wireframeMode) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        
        //SSAO
        //_ssao.RenderSsao(_gBuffer, projectionMat, _viewport, _testVal, viewMat);
        
        //Lighting Pass
        _postProcess.Bind();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gBuffer.BindBufferTextures();
        _lightingShader.DrawMeshLighting(_lights, _camera, _shadows, _ssao);
        
        GL.Enable(EnableCap.Blend);
        
        //_gBuffer.SetDefaultDepth(_viewport);
        _lightShader.DrawGeometryMesh(worldMat, projectionMat, viewMat);
        //Skybox
        /*viewMat = _camera.GetViewMatrix4().ClearTranslation();
        worldMat = viewMat * projectionMat;
        
        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Lequal);
        _skyboxProgram.DrawMesh(worldMat);*/
        
        //Post Process
        _postProcess.Unbind();
        GL.Viewport(0, 0, _viewport.X, _viewport.Y);
        _postProcess.DrawPostProcess(_isPostProcess ? -1 : _shadows.TextureHandle); //_shadows.TextureHandle : _gBuffer.NormalsTexture.Handle
        GL.Disable(EnableCap.DepthTest);
        
        //Text
        _fonts["Wide"].DrawText(_fpsDisplay + " fps", new Vector2(25f, _viewport.Y - 60f), 0.75f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText(_camera.Fov + " fov", new Vector2(25f, _viewport.Y - 110f), 0.75f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText("PLAYER X: "+ (MathF.Floor(_camera.Front.X * 1000f) / 1000f) + " Y: " + (MathF.Floor(_camera.Front.Y * 1000f) / 1000f) + " Z: "+ (MathF.Floor(_camera.Front.Z * 1000f) / 1000f),
            new Vector2(25f, _viewport.Y - 160f), 0.5f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText("STATION  X: "+ (MathF.Floor(_earth.EarthObject.Transform.Position.X * 1000f) / 1000f) + " Y: " + (MathF.Floor(_earth.EarthObject.Transform.Position.Y * 1000f) / 1000f) + " Z: "+ (MathF.Floor(_earth.EarthObject.Transform.Position.Z * 1000f) / 1000f),
            new Vector2(25f, _viewport.Y - 210f), 0.5f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText("Boost: " + _camera.BoostSpeed, new Vector2(25f, _viewport.Y - 260f), 0.5f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText("Grayscale: " + _postProcess.Grayscale, new Vector2(25f, _viewport.Y - 310f), 0.5f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText("VALUE: Y: " + _camera.Yaw + ", P: " + _camera.Pitch, new Vector2(25f, _viewport.Y - 360f), 0.5f, new Vector4(1f), _viewport);
        _fonts["Pixel"].DrawText("Altitude: " + Math.Floor(-(_earth.EarthObject.Transform.Scale.X + _earth.EarthObject.Transform.Position.X) * 6f) + " km", new Vector2(25f, _viewport.Y - 410f), 0.5f, new Vector4(1f), _viewport);

        var hit = _earth.CollisionSphere.CheckCollision(new Ray(_camera.Transform.Position, _camera.Front));
        if (!hit.HasHit) _fonts["Pixel"].DrawText("RAY: NO COLLISION", new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
        else _fonts["Pixel"].DrawText("RAY: HIT, POS: " + hit.HitPos + ", LEN: " + hit.Distance, new Vector2(25f, _viewport.Y - 460f), 0.5f, new Vector4(1f), _viewport);
        
        _fonts["Pixel"].DrawText("FROM ORBIT v0.0.14", new Vector2(_viewport.X - 255f, _viewport.Y - 30f), 0.35f, new Vector4(1f, 1f, 1f, 0.2f), _viewport);
        
        GL.DepthFunc(DepthFunction.Less);
        
        //UI Windows
        _canvas.DrawCanvas(_viewport);
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

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (_isRotation) _anim += (float)args.Time;
        
        //_lights[LightTypes.Point][1].Transform.Position.Y = (float)Math.Cos(-_anim) * 4f + 4f;
        //_lights[LightTypes.Point][1].Material.Color = Light.HsvToRgb(new Vector3(_anim * 40f, 1f, 1f));
        
        if (!IsFocused) return;

        if (_MoveEarthMode) _earth.MoveEarth(KeyboardState, (float)args.Time, _camera.BoostSpeed, [_lights[LightTypes.Directional][0]]);
        else _camera.Move(KeyboardState, (float)args.Time);

        _lights[LightTypes.Directional][0].Transform.Position =
            -_lights[LightTypes.Directional][0].Transform.Quaternion.ToEulerAngles() *
            _earth.EarthObject.Transform.Scale.X;
        
        //var prevTransform = _camera.Transform.Position;
        //if (_collisionBox.CheckCollision(_camera.Transform.Position)) _camera.Transform.Position = prevTransform;
        
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) _camera.SpeedBoost = true;
        if (KeyboardState.IsKeyReleased(Keys.LeftShift)) _camera.SpeedBoost = false;

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
            _MoveEarthMode = !_MoveEarthMode;
        }
        
        var sens = (float)args.Time * 2f;
        if (KeyboardState.IsKeyDown(Keys.Insert)) _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(sens, 0f, 0f) * _lights[LightTypes.Directional][0].Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.Delete)) _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(-sens, 0f, 0f) * _lights[LightTypes.Directional][0].Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.Home)) _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(0f, sens, 0f) * _lights[LightTypes.Directional][0].Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.End)) _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(0f, -sens, 0f) * _lights[LightTypes.Directional][0].Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.PageUp)) _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(0f, 0f, sens) * _lights[LightTypes.Directional][0].Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.PageDown)) _lights[LightTypes.Directional][0].Transform.Quaternion = Quaternion.FromEulerAngles(0f, 0f, -sens) * _lights[LightTypes.Directional][0].Transform.Quaternion;
        if (KeyboardState.IsKeyDown(Keys.Enter))
        {
            _lights[LightTypes.Directional][0].Transform.SetRotationByPosition(Vector3.Zero);
            Console.WriteLine(_lights[LightTypes.Directional][0].Transform.Position.ToString());
        }
        
        if (_timerManager.CheckTimer("+", (float)args.Time, KeyboardState.IsKeyDown(Keys.G)))
        {
            //_camera.Fov = Math.Min(160f, _camera.Fov + 2f);
            //_camera.UpdateSensitivityByFov();
            //_testVal += 0.25f;
            _camera.Sensitivity += 1f;
        }
        if (_timerManager.CheckTimer("-", (float)args.Time, KeyboardState.IsKeyDown(Keys.H)))
        {
            //_camera.UpdateSensitivityByFov();
            //_camera.Fov = Math.Max(1f, _camera.Fov - 2f);
            //_testVal -= 0.25f;
            _camera.Sensitivity -= 1f;
        }

        if (_timerManager.CheckTimer("I", (float)args.Time, KeyboardState.IsKeyDown(Keys.I))) _postProcess.Grayscale += 1f;
        if (_timerManager.CheckTimer("O", (float)args.Time, KeyboardState.IsKeyDown(Keys.O))) _postProcess.Grayscale -= 1f;
        
        _camera.UpdateCameraFront();
        if (_mouse.IsDown) _mouse.PressLenght++;
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
        
            _camera.Mouse.X = e.X + _viewport[0] / 2f;
            _camera.Mouse.Y = -(e.Y + _viewport[1] / 2f);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsFocused) return;
        
        _camera.BoostSpeed = Math.Min(1000f, Math.Max(_camera.BaseSpeed, _camera.BoostSpeed + e.OffsetY * 3f));
    }
    
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _mouse.IsDown = false;
        _mouse.PressLenght = 0;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        _mouse.IsDown = true;
    }

    protected override void OnResize(ResizeEventArgs e)
    { 
        GL.Viewport(0, 0, e.Width, e.Height);

        var viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        _viewport.X = viewport[2];
        _viewport.Y = viewport[3];
        
        _camera.UpdateSensitivityByAspect(_viewport);

        _postProcess.Resize(_viewport / _renderScale);
        _gBuffer.Resize(_viewport / _renderScale);
        _ssao.Framebuffer.AttachedTextures[0].Resize(_viewport / _renderScale);
        _ssao.BlurFramebuffer.AttachedTextures[0].Resize(_viewport / _renderScale);
    }
}