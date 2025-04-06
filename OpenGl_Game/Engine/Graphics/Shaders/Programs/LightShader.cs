using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class LightShader : ShaderProgram
{
    public Dictionary<LightTypes, List<Light>> Lights { get; set; }
    
    public LightShader(Dictionary<LightTypes, List<Light>> lights, VertexAttribute[] verticesAttribs) : base(
        [
            new Shader(@"geometryShaders\vertexShader.vert", ShaderType.VertexShader),
            new Shader(@"LightShaders\lightShader.frag", ShaderType.FragmentShader)
        ], [..Light.LightsDicToList(lights)], verticesAttribs, addTangent:true
    )
    {
        Lights = lights;
    }

    public override void Draw(params object[] param)
    {
        BindAll();
        
        DrawEachObject((Matrix4)param[0]);
        
        UnbindAll();
    }
    
    public override void DeleteAll()
    {
        Delete();
    }
}