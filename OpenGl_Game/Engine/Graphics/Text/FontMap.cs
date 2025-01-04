using System.Drawing;
using System.Runtime.InteropServices;
using FreeTypeSharp;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Graphics.Text;

public class FontMap
{
    public Dictionary<char, FontCharacter> Characters;
    
    public unsafe FontMap(string fontPath)
    {
        Characters = new Dictionary<char, FontCharacter>();
        
        FT_LibraryRec_* ft;
        FT_FaceRec_* face;

        FT.FT_Init_FreeType(&ft);

        var path = (byte*)Marshal.StringToHGlobalAnsi(@"C:\Files\Code\.NET\OpenGl_Game\OpenGl_Game\Assets\Fonts\" + fontPath);
        if (FT.FT_New_Face(ft, path, 0, &face) != FT_Error.FT_Err_Ok)
        {
            Console.WriteLine("Failed to load font!");
        }
        else
        {
            FT.FT_Set_Pixel_Sizes(face, 0, 70);
        
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

    public void DrawText(ShaderProgram program, string text, Vector2 position, float scale, Vector3 color, Vector2 viewport)
    {
        program.Use();
        program.ArrayBuffer.Bind();
        
        GL.ActiveTexture(TextureUnit.Texture0);
        program.SetUniform("text", 0);
        program.SetUniform("textColor", color);
        
        program.SetUniform("viewport", viewport);

        var chars = text.ToCharArray();
        foreach (var c in chars)
        {
            var ch = Characters[c];

            var xpos = position.X + ch.Bearing.X * scale;
            var ypos = position.Y - (ch.Size.Y - ch.Bearing.Y) * scale;

            var w = ch.Size.X * scale;
            var h = ch.Size.Y * scale;

            float[] vertices = [
                xpos, ypos + h,     0f, 0f,
                xpos, ypos,         0f, 1f,
                xpos + w, ypos,     1f, 1f,
                
                xpos, ypos + h,     0f, 0f,
                xpos + w, ypos,     1f, 1f,
                xpos + w, ypos + h, 1f, 0f
            ];
            
            GL.BindTexture(TextureTarget.Texture2d, ch.Handle);
            
            program.VertexBuffer.Bind();
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
            program.VertexBuffer.Unbind();
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            position.X += (ch.Advance >> 6) * scale;
        }
        program.ArrayBuffer.Unbind();
        GL.BindTexture(TextureTarget.Texture2d, 0);
    }

    public void Delete()
    {
        foreach (var c in Characters) GL.DeleteTexture(c.Value.Handle);
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