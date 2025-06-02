using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI;
using OpenGl_Game.Engine.UI.Elements;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;

namespace OpenGl_Game.Game;

public class BurnEffect
{
    private UiGraphics _graphics;
    public Framebuffer Framebuffer { get; set; }
    public Vector2i ImageSize { get; set; }
    
    public unsafe BurnEffect(Texture colorMap, Vector2i size)
    {
        ImageSize = size;
        Framebuffer = new Framebuffer();
        Framebuffer.AttachTexture(new Texture(0, size, null, minFilter: TextureMinFilter.Linear, magFilter: TextureMagFilter.Linear), FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d);

        _graphics = new UiGraphics();
        _graphics.Elements.Add("bg", new UiRectangle(Vector3.Zero, colorMap, 2f, 2f));
        _graphics.Elements.Add("dot", new UiRectangle(new Vector3(0f), new Texture("Station\\circle.png", 0, minFilter: TextureMinFilter.Linear, magFilter: TextureMagFilter.Linear), 0.0005f, 0.001f));
        _graphics.InitProgram();
        
        Draw();
        _graphics.Elements["bg"].GetEngineObject().IsVisible = false;
    }

    public void Draw(Vector2? coords = null)
    {
        if (coords != null)
        {
            _graphics.Elements["dot"].GetEngineObject().Transform.Position.X = coords.Value.X / 180f;
            _graphics.Elements["dot"].GetEngineObject().Transform.Position.Y = coords.Value.Y / 90;
        }
        GL.Viewport(0, 0, ImageSize.X, ImageSize.Y);
        
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Framebuffer.Bind();
        
        _graphics.GraphicsProgram.Draw(ImageSize.ToVector2());
        
        Framebuffer.Unbind();
    }

    public void SetSize(float sizeKm)
    {
        _graphics.Elements["dot"].GetEngineObject().Transform.Scale.X = MathF.Log(sizeKm * 10f) / 40000f * 10f;
        _graphics.Elements["dot"].GetEngineObject().Transform.Scale.Y = MathF.Log(sizeKm * 10f) / 20000f * 10f;
    }
}