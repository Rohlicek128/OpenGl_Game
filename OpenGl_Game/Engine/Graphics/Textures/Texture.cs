using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using StbImageSharp;

namespace OpenGl_Game.Engine.Graphics.Textures;

public class Texture
{
    public readonly int Handle;
    public uint Index;

    public Texture(string path, uint index)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        var image = ImageResult.FromStream(File.OpenRead(@"C:\Files\Code\.NET\OpenGl_Game\OpenGl_Game\Assets\" + path), ColorComponents.RedGreenBlueAlpha);
        
        Handle = GL.GenTexture();
        Index = index;
        GL.ActiveTexture(TextureUnit.Texture0 + Index);
        Bind();

        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

        GL.GenerateMipmap(TextureTarget.Texture2d);

        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, 
            image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        Unbind();
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2d, Handle);
    }
    
    public void ActiveAndBind()
    {
        GL.ActiveTexture(TextureUnit.Texture0 + Index);
        Bind();
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2d, 0);
    }

    public void Delete()
    {
        GL.DeleteTexture(Handle);
    }
}