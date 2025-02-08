using OpenGl_Game.Shaders;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.PostProcess;

public class TonePostProcess : PostProcess
{
    public int Banding;
    
    public TonePostProcess(Shader[] shaders, Vector2i viewport) : base(shaders, viewport)
    {
        Banding = 10;
    }

    public override void DrawPostProcess()
    {
        UseProgram();
        Program.SetUniform("banding", Banding);
        Draw();
    }
}