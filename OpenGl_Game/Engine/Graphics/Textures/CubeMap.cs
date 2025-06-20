using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace OpenGl_Game.Engine.Graphics.Textures;

public class CubeMap
{
    public int Handle;

    public MeshData MeshData;

    private float[] _vertices =
    [
        -0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f, 0.5f, -0.5f,
        -0.5f, 0.5f, -0.5f,

        -0.5f, -0.5f, 0.5f,
        0.5f, -0.5f, 0.5f,
        0.5f, 0.5f, 0.5f,
        -0.5f, 0.5f, 0.5f,

        -0.5f, 0.5f, -0.5f,
        -0.5f, -0.5f, -0.5f,
        -0.5f, -0.5f, 0.5f,
        -0.5f, 0.5f, 0.5f,

        0.5f, -0.5f, -0.5f,
        0.5f, 0.5f, -0.5f,
        0.5f, 0.5f, 0.5f,
        0.5f, -0.5f, 0.5f,

        -0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, -0.5f,
        0.5f, -0.5f, 0.5f,
        -0.5f, -0.5f, 0.5f,

        0.5f, 0.5f, -0.5f,
        -0.5f, 0.5f, -0.5f,
        -0.5f, 0.5f, 0.5f,
        0.5f, 0.5f, 0.5f,
    ];
    private uint[] _indices =
    [
        // front and back
        0, 3, 2,
        2, 1, 0,
        4, 5, 6,
        6, 7, 4,
        // left and right
        11, 8, 9,
        9, 10, 11,
        12, 13, 14,
        14, 15, 12,
        // bottom and top
        16, 17, 18,
        18, 19, 16,
        20, 21, 22,
        22, 23, 20
    ];

    public CubeMap(string dirPath, bool isDirectory = true)
    {
        Handle = GL.GenTexture();
        MeshData = new MeshData(_vertices, _indices);
        Bind();

        GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
        GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
        GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);
        GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameteri(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        
        var image = new ImageResult();
        if (!isDirectory)
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            image = ImageResult.FromStream(File.OpenRead(RenderEngine.DirectoryPath + @"Assets\" + dirPath), ColorComponents.RedGreenBlue);
        }
        for (uint i = 0; i < 6; i++)
        {
            if (isDirectory)
            {
                StbImage.stbi_set_flip_vertically_on_load(1);
                image = ImageResult.FromStream(File.OpenRead(RenderEngine.DirectoryPath + @"Assets\" + dirPath + @"\" + i + ".jpg"), ColorComponents.RedGreenBlue);
            }
            
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat.Rgb, 
                image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, image.Data);
        }
        
        Unbind();
    }

    public void Bind()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
    }
    
    public void Unbind()
    {
        GL.BindTexture(TextureTarget.TextureCubeMap, 0);
    }

    public void Delete()
    {
        GL.DeleteTexture(Handle);
    }
}