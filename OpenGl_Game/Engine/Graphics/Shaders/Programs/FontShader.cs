using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Graphics.Shaders.Programs;

public class FontShader : ShaderProgram
{
    public FontShader(EngineObject text) : base([
        new Shader(@"TextShaders\textShader.vert", ShaderType.VertexShader),
        new Shader(@"TextShaders\textShader.frag", ShaderType.FragmentShader)
    ], [text], [new VertexAttribute(VertexAttributeType.PosAndTex, 4)], BufferUsage.DynamicDraw)
    {
    }
}