using OpenGl_Game.Engine.Graphics.PostProcess;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class CollisionShader : ShaderProgram
{
    public EngineObject LookingAtObject { get; set; }
    public Vector2 LookingAtUv { get; set; }
    private EngineObject _emptyObject;
    
    public CollisionShader(GeometryShader geometryShader) : base(
        [
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"collisionShaders\collision.frag", ShaderType.FragmentShader)
        ], geometryShader
    )
    {
        _emptyObject = EngineObject.CreateEmpty();
        LookingAtObject = EngineObject.CreateEmpty();
        LookingAtUv = Vector2.Zero;
    }

    public override void SetUniforms(params object[] param)
    {
        SetUniform("world", (Matrix4)param[0]);
        SetUniform("view", (Matrix4)param[1]);
    }

    public override void Draw(params object[] param)
    {
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        BindAll();
        SetUniforms(param[0], param[1]);
        
        DrawEachObject((Matrix4)param[1]);

        var viewport = (Vector2i)param[2];
        var lookingAtId = (int)(RenderEngine.ReadPixel(viewport.X / 2, viewport.Y / 2)[0] * RenderEngine.MaxObjectIds);
        LookingAtObject = Objects.FirstOrDefault(eo => eo.Id == lookingAtId, _emptyObject);
        
        UnbindAll();
    }

    public void SetLookingAtUv(int uvTexture, PostProcessShader postProcess, Vector2i viewport)
    {
        postProcess.Draw(uvTexture);
        var pixels = RenderEngine.ReadPixel(viewport.X / 2, viewport.Y / 2);
        LookingAtUv = LookingAtUv with { X = pixels[0], Y = pixels[1] };
    }

    public override void DeleteAll()
    {
        Delete();
    }
}