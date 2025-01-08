using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Shaders;

public class Shader
{
    public readonly int Handle;

    public Shader(string path, ShaderType type)
    {
        Handle = GL.CreateShader(type);
        GL.ShaderSource(Handle, File.ReadAllText(RenderEngine.DirectoryPath + @"Engine\Graphics\Shaders\" + path));
        GL.CompileShader(Handle);
    }
}