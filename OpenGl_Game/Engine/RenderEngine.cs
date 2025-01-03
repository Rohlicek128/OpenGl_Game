using OpenGl_Game.Buffers;
using OpenGl_Game.Engine;
using OpenGl_Game.Engine.Graphics;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Text;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Attribute = OpenGl_Game.Engine.Graphics.Buffers.Attribute;

namespace OpenGl_Game;

public class RenderEngine : GameWindow
{
    private Camera _camera;
    private bool _isRotation = true;
    private bool _isMouseGrabbed = true;
    private float _anim, _fpsTimer;
    private int _fpsCount, _fpsDisplay;

    private Vector2 _viewport;

    private List<ShaderProgram> _programs;
    private ShaderProgram _skyboxProgram;
    
    private ShaderProgram _fontProgram;
    private Dictionary<string, FontMap> _fonts;

    private List<TexturesPbr> _textures;
    private CubeMap _skybox;

    private EngineObject[] _objects;
    private Dictionary<LightTypes, List<Light>> _lights;
    
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
        _viewport = new Vector2(width, height);
        CenterWindow();

        _textures = [];
        _programs = [];

        _fonts = new Dictionary<string, FontMap>();
        
        _camera = new Camera(new Vector3(0f, 0f, -3f), 3f, 0.14f, 90f);
        _camera.UpdateSensitivityByAspect(_viewport);
        _camera.UpdateSensitivityByFov();
    }

    protected override void OnLoad()
    {
        IsVisible = true;
        
        //GL.ClearColor(new Color4<Rgba>(0f, 0f, 0f, 1f));
        GL.ClearColor(new Color4<Rgba>(28f / 255f, 34f / 255f, 41f / 255f, 1f));
        
        Attribute[] verticesAttribs = [
            new(AttribType.Position, 3), 
            new(AttribType.TextureCoords, 2), 
            new(AttribType.Normal, 3)
        ];
        
        //Texture
        _skybox = new CubeMap("Skybox");
        
        _textures.Add(new TexturesPbr());
        _textures[0].AddTexture(TextureTypes.Diffuse, new Texture("container.png", 0));
        _textures[0].AddTexture(TextureTypes.Specular, new Texture("container_s.png", 1));
        
        _textures.Add(new TexturesPbr());
        _textures[1].AddTexture(TextureTypes.Diffuse, new Texture("concrete_pavers_diff_2k.jpg", 0));
        _textures[1].AddTexture(TextureTypes.Specular, new Texture("concrete_pavers_rough_2k.jpg", 1));
        
        //Fonts
        _fonts.Add("Cascadia", new FontMap("CascadiaCode.ttf"));
        _fonts.Add("ATName", new FontMap("ATNameSansTextTrial-ExtraBold.otf"));
        
        //Materials
        var material1 = new Material(new Vector3(1f, 0f, 0f));
        var material2 = new Material(
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, 0.8f, 0.65f),
            new Vector3(0.5f, 0.3f, 0.1f),
            256f
        );
        
        //Objects
        var rollingCube = new EngineObject(
            "Chill Cube", 
            new Vector3(0f, 0f, 0f), 
            ObjFileLoader.CreateCubeVertices(),
            ObjFileLoader.CreateCubeIndices(),
            material1
        );
        rollingCube.Material.Color = new Vector3(1f, 1f, 0f);
        var rotatingCube = new EngineObject(
            "Chill Cube #2", 
            new Vector3(0f, 0f, -5f), 
            ObjFileLoader.CreateCubeVertices(),
            ObjFileLoader.CreateCubeIndices(),
            _textures[0]
        );
        var floor = new EngineObject(
            "Floor", 
            new Vector3(0f, -0.501f, 0f), 
            ObjFileLoader.CreateQuadVertices(0.5f),
            ObjFileLoader.CreateQuadIndices(),
            _textures[1]
        );
        floor.Material.Shininess = 32f;
        floor.Material.Specular = new Vector3(0.5f);
        floor.Transform.Scale *= 20f;
        
        var teapot = ObjFileLoader.LoadFromFile(@"teapot\teapot.obj", verticesAttribs);
        teapot.Transform.Scale /= 80f;
        teapot.Transform.Position *= teapot.Transform.Scale;
        teapot.Transform.Position.Z += 3f;
        teapot.Material.Color = new Vector3(0.8f, 0.05f, 0.2f);
        teapot.Material.Shininess = 600f;
        
        //lights
        var dirLight = new Light(
            "Dir light", 
            new Vector3(0f, 7f, 0f), 
            ObjFileLoader.CreateCubeVertices(),
            ObjFileLoader.CreateCubeIndices(),
            new Material(new Vector3(1f, 1f, 1f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.045f * 10, 0.0075f * 10),
            LightTypes.Directional
        );
        dirLight.Transform.Scale *= 0.5f;
        dirLight.Transform.Rotation = new Vector3(1.433031f, -1.5707964f, 2.588305f);
        dirLight.IsVisible = false;
        var pointLight = new Light(
            "Point light", 
            new Vector3(5f, 2f, 2f), 
            ObjFileLoader.CreateCubeVertices(),
            ObjFileLoader.CreateCubeIndices(),
            new Material(new Vector3(1f, 0.91f, 0.5f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.045f, 0.0075f),
            LightTypes.Point
        );
        pointLight.Transform.Scale *= 0.25f;
        var pointLight2 = new Light(
            "Point light", 
            new Vector3(-3f, 1.5f, 1f), 
            ObjFileLoader.CreateCubeVertices(),
            ObjFileLoader.CreateCubeIndices(),
            new Material(new Vector3(0f, 1f, 0f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.045f, 0.0075f),
            LightTypes.Point
        );
        pointLight2.Transform.Scale *= 0.25f;
        
        _objects = [rollingCube, teapot, rotatingCube, floor];
        
        _lights = new Dictionary<LightTypes, List<Light>>
        {
            { LightTypes.Directional, [dirLight] },
            { LightTypes.Point, [pointLight, pointLight2] }
        };
        
        //Shader Program
        _programs.Add(new ShaderProgram([
            new Shader("vertexShader.vert", ShaderType.VertexShader),
            new Shader("fragmentShader.frag", ShaderType.FragmentShader)
        ], _objects.ToList(), verticesAttribs));
        //Light Program
        _programs.Add(new ShaderProgram([
            new Shader("vertexShader.vert", ShaderType.VertexShader),
            new Shader("lightShader.frag", ShaderType.FragmentShader)
        ], [..Light.LightsDicToList(_lights)], verticesAttribs));
        
        //Skybox
        var skybox = new EngineObject(
            "Skybox", 
            new Vector3(0f), 
            _skybox.Vertices,
            _skybox.Indices,
            new Material(new Vector3(1f))
        );
        _skyboxProgram = new ShaderProgram([
            new Shader("skybox.vert", ShaderType.VertexShader),
            new Shader("skybox.frag", ShaderType.FragmentShader)
        ], [skybox], [new Attribute(AttribType.Position, 3)]);
        
        //Text
        var text = EngineObject.CreateEmpty();
        text.VerticesData = new float[6 * 4];
        _fontProgram = new ShaderProgram([
            new Shader("textShader.vert", ShaderType.VertexShader),
            new Shader("textShader.frag", ShaderType.FragmentShader)
        ], [text], [new Attribute(AttribType.PosAndTex, 4)], BufferUsage.DynamicDraw);
        
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
    }

    protected override void OnUnload()
    {
        foreach (var program in _programs) program.Delete();
        _skyboxProgram.Delete();
        _fontProgram.Delete();
        foreach (var textures in _textures) textures.DeleteAll();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var viewMat = _camera.GetViewMatrix4();
        var projectionMat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov), (float) _viewport[0] / _viewport[1], 0.025f, 500f);
        var worldMat = viewMat * projectionMat;
        
        foreach (var program in _programs) program.DrawMesh(worldMat, _lights, _camera, _skybox.Handle);
        
        //Skybox
        viewMat = _camera.GetViewMatrix4().ClearTranslation();
        worldMat = viewMat * projectionMat;
        
        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Lequal);
        _skyboxProgram.DrawMesh(worldMat, _lights, _camera, _skybox.Handle);
        
        //Text
        _fonts["ATName"].DrawText(_fontProgram, _fpsDisplay + " fps", new Vector2(25f, _viewport.Y - 60f), 0.75f, new Vector3(1f), _viewport);
        _fonts["Cascadia"].DrawText(_fontProgram, _camera.Fov + " fov", new Vector2(25f, _viewport.Y - 110f), 0.75f, new Vector3(1f), _viewport);
        
        GL.Enable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);
        
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
        _objects[1].Transform.Rotation.Y = _anim / 2f;
        _objects[0].Transform.Position.X = (float)Math.Sin(_anim * 2f) * 3f + 5f;
        _objects[0].Transform.Rotation.Z = (float)Math.Sin(-_anim * 2f) * 5f + 2f;
        
        _lights[LightTypes.Point][0].Transform.Position.X = (float)Math.Sin(-_anim) * 5f;
        _lights[LightTypes.Point][0].Transform.Position.Y = (float)Math.Cos(-_anim / 2) * 2f + 3f;
        _lights[LightTypes.Point][0].Transform.Position.Z = (float)Math.Cos(-_anim) * 5f;
        
        _lights[LightTypes.Point][1].Transform.Position.Y = (float)Math.Cos(-_anim) * 4f + 4f;
        _lights[LightTypes.Point][1].Material.Color = Light.HsvToRgb(new Vector3(_anim * 40f, 1f, 1f));
        
        if (!IsFocused) return;

        var speedTime = (float)(_camera.Speed * args.Time * (_camera.SpeedBoost ? 5f : 1.25f));

        if (KeyboardState.IsKeyDown(Keys.W)) _camera.Transform.Position += Vector3.Normalize(Vector3.Cross(_camera.Right, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.S)) _camera.Transform.Position -= Vector3.Normalize(Vector3.Cross(_camera.Right, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.A)) _camera.Transform.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.D)) _camera.Transform.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.E)) _camera.Transform.Position += _camera.Up * speedTime;
        if (KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.Q)) _camera.Transform.Position -= _camera.Up * speedTime;
        
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) _camera.SpeedBoost = true;
        if (KeyboardState.IsKeyReleased(Keys.LeftShift)) _camera.SpeedBoost = false;

        if (KeyboardState.IsKeyDown(Keys.Escape)) _isMouseGrabbed = !_isMouseGrabbed;
        if (KeyboardState.IsKeyDown(Keys.M)) _isRotation = !_isRotation;
        
        const float sens = 2f;
        if (KeyboardState.IsKeyDown(Keys.Insert)) _lights[LightTypes.Directional][0].Transform.Rotation.X += (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.Delete)) _lights[LightTypes.Directional][0].Transform.Rotation.X -= (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.Home)) _lights[LightTypes.Directional][0].Transform.Rotation.Y += (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.End)) _lights[LightTypes.Directional][0].Transform.Rotation.Y -= (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.PageUp)) _lights[LightTypes.Directional][0].Transform.Rotation.Z += (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.PageDown)) _lights[LightTypes.Directional][0].Transform.Rotation.Z -= (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.Enter)) Console.WriteLine(_lights[LightTypes.Directional][0].Transform.Rotation.ToString());
        
        _camera.UpdateCameraFront();
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (!IsFocused) return;

        if (!_isMouseGrabbed)
        {
            CursorState = CursorState.Normal;
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

        _camera.Fov -= e.OffsetY * 4f;
        _camera.Fov = Math.Min(160f, Math.Max(1f, _camera.Fov));
        _camera.UpdateSensitivityByFov();
    }
    
    protected override void OnResize(ResizeEventArgs e)
    { 
        GL.Viewport(0, 0, e.Width, e.Height);

        var viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        _viewport.X = viewport[2];
        _viewport.Y = viewport[3];
        
        _camera.UpdateSensitivityByAspect(_viewport);
    }
}