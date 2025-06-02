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
        _graphics.Elements.Add("dot", new UiRectangle(new Vector3(0f), new Vector4(0f, 0f, 0f, 1f), 0.01f, 0.01f));
        _graphics.InitProgram();
        
        Draw();
        _graphics.Elements["bg"].GetEngineObject().IsVisible = false;
    }

    public void Draw()
    {
        _graphics.Elements["dot"].GetEngineObject().Transform.Position.Y = 0.12f;
        _graphics.Elements["dot"].GetEngineObject().Material.Color.X = 0f;
        GL.Viewport(0, 0, ImageSize.X, ImageSize.Y);
        
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Framebuffer.Bind();
        
        _graphics.GraphicsProgram.Draw(ImageSize.ToVector2());
        
        Framebuffer.Unbind();
    }

    public EngineObject CreateDot()
    {
        return new EngineObject(
            "Dot",
            new Transform(Vector3.Zero),
            new MeshData([0f, 0f, 0f, 0f], [0], PrimitiveType.Points),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, new Texture("black1x1.png", 0)},
            }),
            pointSize: 30f
        );
    }
}