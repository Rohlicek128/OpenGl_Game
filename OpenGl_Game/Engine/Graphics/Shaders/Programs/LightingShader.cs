using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shadows;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class LightingShader : ShaderProgram
{
    public Dictionary<LightTypes, List<Light>> Lights { get; set; }
    
    public LightingShader(Dictionary<LightTypes, List<Light>> lights) : base(
        [
            new Shader(@"gLightingShaders\lightingShader.vert", ShaderType.VertexShader),
            new Shader(@"gLightingShaders\lightingShader.frag", ShaderType.FragmentShader)
        ], [MeshConstructor.CreateScreenQuad()], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)]
    )
    {
        Lights = lights;
    }

    public override void SetUniforms(params object[] param)
    {
        var camera = (Camera)param[0];
        var shadow = (ShadowMap)param[1];
        var ssaoTexture = (int)param[2];
        
        SetUniform("gPosition", 0);
        SetUniform("gNormal", 1);
        SetUniform("gAlbedoSpec", 2);
        
        SetUniform("shadowMap", 3);
        GL.ActiveTexture(TextureUnit.Texture3);
        GL.BindTexture(TextureTarget.Texture2d, shadow.TextureHandle);

        SetUniform("ssaoMap", 4);
        GL.ActiveTexture(TextureUnit.Texture4);
        GL.BindTexture(TextureTarget.Texture2d, ssaoTexture);
        
        SetUniform("lightSpace", shadow.LightSpace);
        SetUniform("viewPos", camera.Transform.Position);
    }

    public override void Draw(params object[] param)
    {
        GL.Disable(EnableCap.DepthTest);
        BindAll();
        
        SetUniforms(param);
        
        foreach (var lightList in Lights)
        {
            if (lightList.Key == LightTypes.Directional) lightList.Value[0].SetUniformsForDirectional(this);
            else if (lightList.Key == LightTypes.Point)
            {
                for (int i = 0; i < lightList.Value.Count; i++) lightList.Value[i].SetUniformsForPoint(this, i, (int)param[3]);
            }
        }
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        UnbindAll();
        GL.Enable(EnableCap.DepthTest);
    }
    
    public override void DeleteAll()
    {
        Delete();
    }
}