using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using StbImageSharp;

namespace OpenGl_Game.Engine.Graphics.Textures;

public class Texture
{
    public readonly int Handle;
    public uint Index;

    public ImageResult Image;
    public InternalFormat Format;
    public PixelType Type;
    public PixelFormat Pixel;

    public unsafe Texture(uint index, Vector2i size, void* data, InternalFormat format = InternalFormat.Rgba, PixelType type = PixelType.UnsignedByte, PixelFormat pixel = PixelFormat.Rgba)
    {
        Handle = GL.GenTexture();
        Index = index;
        Format = format;
        Type = type;
        Pixel = pixel;
        GL.ActiveTexture(TextureUnit.Texture0 + Index);
        Bind();

        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

        GL.GenerateMipmap(TextureTarget.Texture2d);
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, format, 
            size.X, size.Y, 0, pixel, type, data);

        Unbind();
    }
    
    public Texture(int handle, uint index, InternalFormat format = InternalFormat.Rgba, PixelType type = PixelType.UnsignedByte, PixelFormat pixel = PixelFormat.Rgba)
    {
        Handle = handle;
        Index = index;
        Format = format;
        Type = type;
        Pixel = pixel;
    }
    
    public Texture(string path, uint index, TextureMagFilter minFilter = TextureMagFilter.Nearest, TextureMagFilter magFilter = TextureMagFilter.Nearest, InternalFormat format = InternalFormat.Rgba, PixelType type = PixelType.UnsignedByte, PixelFormat pixel = PixelFormat.Rgba)
    {
        Image = LoadImage(path);
        Handle = GL.GenTexture();
        Index = index;
        Format = format;
        Type = type;
        Pixel = pixel;
        GL.ActiveTexture(TextureUnit.Texture0 + Index);
        Bind();

        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) minFilter);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) magFilter);

        GL.GenerateMipmap(TextureTarget.Texture2d);

        GL.TexImage2D(TextureTarget.Texture2d, 0, Format, 
            Image.Width, Image.Height, 0, Pixel, Type, Image.Data);

        Unbind();
    }
    
    public ImageResult LoadImage(string path)
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
        return ImageResult.FromStream(File.OpenRead(RenderEngine.DirectoryPath + @"Assets\" + path), ColorComponents.RedGreenBlueAlpha);
    }

    public Vector4 SampleTexture(Vector2i coords)
    {
        if (coords.X >= Image.Width) coords.X = Image.Width - 1;
        if (coords.Y >= Image.Height) coords.Y = Image.Height - 1;
        
        var pixelIndex = (coords.X + Image.Width * coords.Y) * (int)Image.SourceComp;
        var r = Image.Data[pixelIndex + 0];
        var g = Image.Data[pixelIndex + 1];
        var b = Image.Data[pixelIndex + 2];
        var a = Image.Data[pixelIndex + 3];
        return new Vector4(r, g, b, a);
    }

    public unsafe void Resize(Vector2i viewport)
    {
        Bind();
        GL.TexImage2D(TextureTarget.Texture2d, 0, Format, viewport.X, viewport.Y, 0, PixelFormat.Rgba, Type, null);
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
    
    public void ActiveAndBind(uint index)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + index);
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