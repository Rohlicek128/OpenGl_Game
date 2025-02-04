using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Graphics.Text;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI;
using OpenGl_Game.Shaders;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;
using Attribute = OpenGl_Game.Engine.Graphics.Buffers.Attribute;
using FontMap = OpenGl_Game.Engine.Graphics.Text.FontMap;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using InternalFormat = OpenTK.Graphics.OpenGL.Compatibility.InternalFormat;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game;

public class RenderEngine : GameWindow
{
    public static string DirectoryPath = @"C:\Files\Code\.NET\OpenGl_Game\OpenGl_Game\";
    
    private Camera _camera;
    private float _anim, _fpsTimer;
    private int _fpsCount, _fpsDisplay;
    
    private bool _isRotation = true;
    private bool _isMouseGrabbed = true;
    private bool _wireframeMode = false;
    private bool _framebufferSee;

    private Mouse _mouse;
    private Vector2i _viewport;

    private List<ShaderProgram> _programs;
    private ShaderProgram _skyboxProgram;
    private ShaderProgram _postProcessShader;
    private Framebuffer _ppFramebuffer;
    private Renderbuffer _ppRenderbuffer;
    
    private Dictionary<string, FontMap> _fonts;

    private ShadowMap _shadowMap;

    private List<TexturesPbr> _textures;
    private CubeMap _skybox;

    private EngineObject[] _objects;
    private Dictionary<LightTypes, List<Light>> _lights;
    private HeightMap _terrain;
    public static readonly uint PrimitiveIndex = uint.MaxValue;

    private TimerManager _timerManager;
    private WindowManager _windowManager;
    
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
        _mouse = new Mouse();
        _viewport = new Vector2i(width, height);
        CenterWindow();

        _textures = [];
        _programs = [];

        _fonts = new Dictionary<string, FontMap>();
        _timerManager = new TimerManager(150);
        
        _camera = new Camera(new Vector3(0f, 0f, -3f), 3f, 0.14f, 95f);
        _camera.UpdateSensitivityByAspect(_viewport);
        _camera.UpdateSensitivityByFov();
    }

    protected override unsafe void OnLoad()
    {
        IsVisible = true;
        
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
        var text = EngineObject.CreateEmpty();
        text.VerticesData.Data = new float[6 * 4];
        var fontProgram = new ShaderProgram([
            new Shader(@"TextShaders\textShader.vert", ShaderType.VertexShader),
            new Shader(@"TextShaders\textShader.frag", ShaderType.FragmentShader)
        ], [text], [new Attribute(AttribType.PosAndTex, 4)], BufferUsage.DynamicDraw);
        
        _fonts.Add("Cascadia", new FontMap("CascadiaCode.ttf", fontProgram));
        _fonts.Add("ATName", new FontMap("ATNameSansTextTrial-ExtraBold.otf", fontProgram));
        
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
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            material1
        );
        playerBox.Transform.Scale = new Vector3(1f, 10f, 2f);
        var rollingCube = new EngineObject(
            "Chill Cube", 
            new Transform(new Vector3(0f, 0f, 0f)), 
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            material1
        );
        rollingCube.Material.Color = new Vector3(1f, 1f, 0f);
        var rotatingCube = new EngineObject(
            "Chill Cube #2", 
            new Transform(new Vector3(0f, 0f, -5f)), 
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            _textures[0]
        );
        var anotherCube = new EngineObject(
            "Another Cube", 
            new Transform(new Vector3(-7f, 1f, 5f)),
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            material2
        );
        anotherCube.Transform.Scale = new Vector3(0.3f, 1f, 0.5f);
        var floor = new EngineObject(
            "Floor", 
            new Transform(new Vector3(0f, -0.501f, 0f)), 
            new VerticesData(ObjFileLoader.CreatePlaneVertices(0.5f), PrimitiveType.Triangles),
            ObjFileLoader.CreatePlaneIndices(),
            _textures[1]
        );
        floor.Material.Shininess = 32f;
        floor.Material.Specular = new Vector3(0.5f);
        floor.Transform.Scale *= 20f;
        
        /*var teapot = ObjFileLoader.LoadFromFile(@"teapot\teapot.obj", verticesAttribs);
        teapot.Transform.Scale /= 80f;
        teapot.Transform.Position *= teapot.Transform.Scale;
        teapot.Transform.Position.Z += 3f;
        teapot.Material.Color = new Vector3(0.8f, 0.05f, 0.2f);
        teapot.Material.Shininess = 600f;*/

        _terrain = new HeightMap("new-zealand-height-map.jpg", verticesAttribs);
        
        //lights
        var dirLight = new Light(
            "Dir light", 
            new Transform(new Vector3(-2.9260643f, 6.0104437f, -5.3240757f)), 
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            new Material(new Vector3(1f, 1f, 1f)),
            new Vector3(0.2f, 1f, 1f),
            new Vector3(1.0f, 0.045f * 10, 0.0075f * 10),
            LightTypes.Directional
        );
        dirLight.Transform.Scale *= 0.5f;
        //dirLight.Transform.Rotation = new Vector3(1.433031f, -1.5707964f, 2.588305f);
        dirLight.Transform.SetRotationByPosition(Vector3.Zero);
        //dirLight.IsVisible = false;
        var pointLight = new Light(
            "Point light", 
            new Transform(new Vector3(-2.3207285f, 5.0880294f, -4.088403f)), 
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            new Material(new Vector3(1f, 0.95f, 0.6f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.45f, 0.075f),
            LightTypes.Point
        );
        pointLight.Transform.Scale *= 0.25f;
        var pointLight2 = new Light(
            "Point light", 
            new Transform(new Vector3(-6f, 1.5f, 1f)), 
            new VerticesData(ObjFileLoader.CreateCubeVertices(), PrimitiveType.Triangles),
            ObjFileLoader.CreateCubeIndices(),
            new Material(new Vector3(0f, 1f, 0f)),
            new Vector3(0.25f, 1f, 1f),
            new Vector3(1.0f, 0.045f, 0.0075f),
            LightTypes.Point
        );
        pointLight2.Transform.Scale *= 0.25f;
        
        _objects = [rollingCube, rotatingCube, anotherCube, playerBox, _terrain.TerrainObject];
        
        _lights = new Dictionary<LightTypes, List<Light>>
        {
            { LightTypes.Directional, [dirLight] },
            { LightTypes.Point, [pointLight, pointLight2] }
        };
        
        //Shader Program
        _programs.Add(new ShaderProgram([
            new Shader(@"LightingShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"LightingShaders\fragmentShader.frag", ShaderType.FragmentShader)
        ], _objects.ToList(), verticesAttribs));
        //Light Program
        _programs.Add(new ShaderProgram([
            new Shader(@"LightingShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"LightShaders\lightShader.frag", ShaderType.FragmentShader)
        ], [..Light.LightsDicToList(_lights)], verticesAttribs));
        
        //Skybox
        var skybox = new EngineObject(
            "Skybox", 
            new Transform(new Vector3(0f)), 
            new VerticesData(_skybox.Vertices, PrimitiveType.Triangles),
            _skybox.Indices,
            new Material(new Vector3(1f))
        );
        _skyboxProgram = new ShaderProgram([
            new Shader(@"SkyboxShaders\skybox.vert", ShaderType.VertexShader),
            new Shader(@"SkyboxShaders\skybox.frag", ShaderType.FragmentShader)
        ], [skybox], [new Attribute(AttribType.Position, 3)]);
        
        //Post Process
        var screenQuad = EngineObject.CreateEmpty();
        screenQuad.VerticesData.Data =
        [
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f,  0.0f, 1.0f,
            1.0f, -1.0f,  1.0f, 0.0f,
            1.0f,  1.0f,  1.0f, 1.0f
        ];
        _postProcessShader = new ShaderProgram([
            new Shader(@"PostProcessShaders\postProcessShader.vert", ShaderType.VertexShader),
            new Shader(@"PostProcessShaders\postProcessShader.frag", ShaderType.FragmentShader)
        ], [screenQuad], [new Attribute(AttribType.PosAndTex, 4)]);
        _ppFramebuffer = new Framebuffer();
        _ppFramebuffer.AttachTexture(new Texture(0, _viewport, null), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);
        _ppRenderbuffer = new Renderbuffer(InternalFormat.Depth24Stencil8, _viewport);
        _ppFramebuffer.AttachRenderbuffer(FramebufferAttachment.DepthStencilAttachment, _ppRenderbuffer.Handle);
        
        //Shadows
        _shadowMap = new ShadowMap(new Vector2i(8192, 8192), 80f, _programs[0]);
        
        //UI
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
        foreach (var program in _programs) program.Delete();
        _skyboxProgram.Delete();
        _postProcessShader.Delete();
        _shadowMap.Delete();
        foreach (var textures in _textures) textures.DeleteAll();
        foreach (var font in _fonts) font.Value.Delete();
        _windowManager.Delete();
        
        Console.WriteLine("end");
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        //Shadows
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shadowMap.RenderDepthMap(_lights[LightTypes.Directional][0], _viewport, _programs[0], _camera);
        
        //Scene
        _ppFramebuffer.Bind();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        if (_wireframeMode)
        {
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
        }

        var viewMat = _camera.GetViewMatrix4();
        var projectionMat = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_camera.Fov), (float) _viewport[0] / _viewport[1], 0.025f, 10000f);
        var worldMat = viewMat * projectionMat;
        
        foreach (var program in _programs) program.DrawMesh(worldMat, _lights, _camera, _skybox.Handle, _shadowMap);
        
        //Skybox
        viewMat = _camera.GetViewMatrix4().ClearTranslation();
        worldMat = viewMat * projectionMat;
        
        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Lequal);
        _skyboxProgram.DrawMesh(worldMat, _lights, _camera, _skybox.Handle, _shadowMap);
        
        if (_wireframeMode) GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        
        //Text
        _fonts["ATName"].DrawText(_fpsDisplay + " fps", new Vector2(25f, _viewport.Y - 60f), 0.75f, new Vector3(1f), _viewport);
        _fonts["Cascadia"].DrawText(_camera.Fov + " fov", new Vector2(25f, _viewport.Y - 110f), 0.75f, new Vector3(1f), _viewport);
        _fonts["Cascadia"].DrawText("X: "+ _camera.Transform.Position.X + " Y: " + _camera.Transform.Position.Y + " Z: "+ _camera.Transform.Position.Z,
            new Vector2(25f, _viewport.Y - 160f), 0.5f, new Vector3(1f), _viewport);
        _fonts["Cascadia"].DrawText("Boost: " + _camera.BoostSpeed, new Vector2(25f, _viewport.Y - 210f), 0.5f, new Vector3(1f), _viewport);
        
        GL.DepthFunc(DepthFunction.Less);
        
        //Post Process
        _ppFramebuffer.Unbind();
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        _postProcessShader.Use();
        _postProcessShader.ArrayBuffer.Bind();
        GL.Disable(EnableCap.DepthTest);
        GL.ActiveTexture(TextureUnit.Texture0);
        if (_framebufferSee) _ppFramebuffer.AttachedTextures[0].Bind();
        else GL.BindTexture(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, _shadowMap.TextureHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        _postProcessShader.ArrayBuffer.Unbind();
        
        //UI Windows
        _windowManager.DrawWindows(_viewport, _fonts);
        
        GL.Enable(EnableCap.CullFace);
        
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
        
        _lights[LightTypes.Point][0].Transform.Position.X = (float)Math.Sin(-_anim * 2f) * 10f;
        _lights[LightTypes.Point][0].Transform.Position.Y = (float)Math.Cos(-_anim) + 2.5f;
        _lights[LightTypes.Point][0].Transform.Position.Z = (float)Math.Cos(-_anim * 2f) * 5f;
        
        _lights[LightTypes.Point][1].Transform.Position.Y = (float)Math.Cos(-_anim) * 4f + 4f;
        _lights[LightTypes.Point][1].Material.Color = Light.HsvToRgb(new Vector3(_anim * 40f, 1f, 1f));
        
        if (!IsFocused) return;

        var speedTime = (float)(_camera.Speed * args.Time * (_camera.SpeedBoost ? _camera.BoostSpeed : _camera.BaseSpeed));

        if (KeyboardState.IsKeyDown(Keys.W)) _camera.Transform.Position += Vector3.Normalize(Vector3.Cross(_camera.Right, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.S)) _camera.Transform.Position -= Vector3.Normalize(Vector3.Cross(_camera.Right, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.A)) _camera.Transform.Position -= Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.D)) _camera.Transform.Position += Vector3.Normalize(Vector3.Cross(_camera.Front, _camera.Up)) * speedTime;
        if (KeyboardState.IsKeyDown(Keys.Space) || KeyboardState.IsKeyDown(Keys.E)) _camera.Transform.Position += _camera.Up * speedTime;
        if (KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.Q)) _camera.Transform.Position -= _camera.Up * speedTime;
        
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
            _framebufferSee = !_framebufferSee;
        }
        
        const float sens = 2f;
        if (KeyboardState.IsKeyDown(Keys.Insert)) _lights[LightTypes.Directional][0].Transform.Position.X += (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.Delete)) _lights[LightTypes.Directional][0].Transform.Position.X -= (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.Home)) _lights[LightTypes.Directional][0].Transform.Position.Y += (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.End)) _lights[LightTypes.Directional][0].Transform.Position.Y -= (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.PageUp)) _lights[LightTypes.Directional][0].Transform.Position.Z += (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.PageDown)) _lights[LightTypes.Directional][0].Transform.Position.Z -= (float)args.Time * sens;
        if (KeyboardState.IsKeyDown(Keys.Enter))
        {
            _lights[LightTypes.Directional][0].Transform.SetRotationByPosition(Vector3.Zero);
            Console.WriteLine(_lights[LightTypes.Directional][0].Transform.Position.ToString());
        }
        _lights[LightTypes.Directional][0].Transform.SetRotationByPosition(Vector3.Zero);
        
        if (_timerManager.CheckTimer("+", (float)args.Time, KeyboardState.IsKeyDown(Keys.KeyPadAdd)))
        {
            _camera.Fov = Math.Min(160f, _camera.Fov + 1f);
            _camera.UpdateSensitivityByFov();
        }
        if (_timerManager.CheckTimer("-", (float)args.Time, KeyboardState.IsKeyDown(Keys.KeyPadSubtract)))
        {
            _camera.Fov = Math.Max(1f, _camera.Fov - 1f);
            _camera.UpdateSensitivityByFov();
        }
        
        _camera.UpdateCameraFront();
        _objects[3].Transform = _camera.Transform;
        _objects[3].Transform.Rotation = _camera.Front;
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
        
        _camera.BoostSpeed = Math.Min(1000f, Math.Max(_camera.BaseSpeed, _camera.BoostSpeed + e.OffsetY * 5f));
    }
    
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        _mouse.IsDown = false;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        _mouse.IsDown = true;
    }

    protected override unsafe void OnResize(ResizeEventArgs e)
    { 
        GL.Viewport(0, 0, e.Width, e.Height);

        var viewport = new int[4];
        GL.GetInteger(GetPName.Viewport, viewport);
        _viewport.X = viewport[2];
        _viewport.Y = viewport[3];
        
        _camera.UpdateSensitivityByAspect(_viewport);
        
        //Resize Post Process
        _ppFramebuffer.AttachedTextures[0].Bind();
        GL.TexImage2D((OpenTK.Graphics.OpenGL.TextureTarget)TextureTarget.Texture2d, 0, (OpenTK.Graphics.OpenGL.InternalFormat)InternalFormat.Rgba, 
            _viewport.X, _viewport.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _ppFramebuffer.AttachedTextures[0].Unbind();
        
        _ppRenderbuffer.Delete();
        _ppRenderbuffer = new Renderbuffer(InternalFormat.Depth24Stencil8, _viewport);
        _ppFramebuffer.AttachRenderbuffer(FramebufferAttachment.DepthStencilAttachment, _ppRenderbuffer.Handle);
    }
}