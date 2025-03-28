namespace OpenGl_Game.Engine.Graphics.PostProcess;

public interface PostProcessShader
{
    public void UseProgram();
    public void UnbindProgram();

    public void SetUniforms();
}