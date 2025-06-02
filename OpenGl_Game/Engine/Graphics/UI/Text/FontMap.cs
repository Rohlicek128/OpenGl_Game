using System.Drawing;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.UI.Text;

public class FontMap
{
    public Dictionary<char, FontCharacter> Characters { get; set; }

    private FontShader _fontProgram; 
    
    public unsafe FontMap(string fontPath, FontShader program, uint pixelHeight = 70)
    {
        Characters = new Dictionary<char, FontCharacter>();
        _fontProgram = program;
        
        FT_LibraryRec_* ft;
        FT_FaceRec_* face;

        FT.FT_Init_FreeType(&ft);

        var path = (byte*)Marshal.StringToHGlobalAnsi(RenderEngine.DirectoryPath + @"Assets\Fonts\" + fontPath);
        if (FT.FT_New_Face(ft, path, 0, &face) != FT_Error.FT_Err_Ok)
        {
            Console.WriteLine("Failed to load font!");
        }
        else
        {
            FT.FT_Set_Pixel_Sizes(face, 0, pixelHeight);
        
            GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
        
            for (int i = 0; i < 128; i++)
            {
                if (FT.FT_Load_Char(face, (char)i, FT_LOAD.FT_LOAD_RENDER) != FT_Error.FT_Err_Ok)
                {
                    Console.WriteLine("Error with char: " + (char)i);
                    continue;
                }
            
                var handle = GL.GenTexture();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2d, handle);

                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
                GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

                var data = face->glyph->bitmap.buffer;
                var w = face->glyph->bitmap.width;
                var h = face->glyph->bitmap.rows;
                GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Red, 
                    (int)w, (int)h, 0, PixelFormat.Red, PixelType.UnsignedByte, data);
            
                Characters.Add((char)i, new FontCharacter(
                    handle,
                    new Vector2(face->glyph->bitmap.width, face->glyph->bitmap.rows),
                    new Vector2(face->glyph->bitmap_left, face->glyph->bitmap_top),
                    (int)face->glyph->advance.x)
                );
            }
            GL.BindTexture(TextureTarget.Texture2d, 0);
        }
        FT.FT_Done_Face(face);
        FT.FT_Done_FreeType(ft);
    }

    public void DrawText(string text, Vector2 position, float scale, Vector4 color, Vector2 viewport, int substract = 0)
    {
        _fontProgram.Use();
        _fontProgram.ArrayBuffer.Bind();
        
        GL.ActiveTexture(TextureUnit.Texture0);
        _fontProgram.SetUniform("text", 0);
        _fontProgram.SetUniform("textColor", color);
        
        _fontProgram.SetUniform("viewport", viewport);

        float[] vertices = [
            0f, 0f,  0f, 0f,
            0f, 0f,  0f, 1f,
            0f, 0f,  1f, 1f,
            0f, 0f,  0f, 0f,
            0f, 0f,  1f, 1f,
            0f, 0f,  1f, 0f
        ];
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length - substract; i++)
        {
            if (chars[i] > 128)
            {
                position.X += 1 * scale;
                continue;
            }
            var ch = Characters[chars[i]];

            var xpos = position.X + ch.Bearing.X * scale;
            var ypos = position.Y - (ch.Size.Y - ch.Bearing.Y) * scale;

            var w = ch.Size.X * scale;
            var h = ch.Size.Y * scale;
            
            vertices[0] = xpos;
            vertices[1] = ypos + h;
            vertices[4] = xpos;
            vertices[5] = ypos;
            vertices[8] = xpos + w;
            vertices[9] = ypos;
            vertices[12] = xpos;
            vertices[13] = ypos + h;
            vertices[16] = xpos + w;
            vertices[17] = ypos;
            vertices[20] = xpos + w;
            vertices[21] = ypos + h;
            
            GL.BindTexture(TextureTarget.Texture2d, ch.Handle);
            
            _fontProgram.VertexBuffer.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
            _fontProgram.VertexBuffer.Unbind();
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            position.X += (ch.Advance >> 6) * scale;
        }
        _fontProgram.ArrayBuffer.Unbind();
        _fontProgram.Unbind();
        GL.BindTexture(TextureTarget.Texture2d, 0);
    }

    public void Delete()
    {
        foreach (var c in Characters) GL.DeleteTexture(c.Value.Handle);
        _fontProgram.Delete();
    }
    
}

public struct FontCharacter
{
    public int Handle;
    public Vector2 Size;
    public Vector2 Bearing;
    public int Advance;

    public FontCharacter(int handle, Vector2 size, Vector2 bearing, int advance)
    {
        Handle = handle;
        Size = size;
        Bearing = bearing;
        Advance = advance;
    }
}