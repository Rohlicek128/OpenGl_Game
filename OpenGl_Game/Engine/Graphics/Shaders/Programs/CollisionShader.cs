using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class CollisionShader : ShaderProgram
{
    public EngineObject LookingAtObject { get; set; }
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
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        ((GeometryShader)param[2]).SetUniforms();
        
        DrawEachObject((Matrix4)param[0]);

        //var viewport = (Vector2i)param[1];
        //var lookingAtId = (int)(RenderEngine.ReadPixel(viewport.X / 2, viewport.Y / 2)[0] * RenderEngine.MaxObjectIds / 20f);
        //LookingAtObject = Objects.FirstOrDefault(eo => eo.Id == lookingAtId, _emptyObject);
        
        UnbindAll();
    }

    public override void DeleteAll()
    {
        Delete();
    }
}