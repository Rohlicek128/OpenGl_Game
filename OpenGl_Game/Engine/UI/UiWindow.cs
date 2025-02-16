using OpenGl_Game.Engine.Graphics.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.UI;

public class UiWindow
{
    public Transform Transform;
    public Vector4 Color;

    public bool IsVisible;
    public string Header;

    private bool _isHeld;
    private Vector2i _startPos;
    private Vector2i _origoPos;

    public UiWindow(ShaderProgram program, string header, Vector4 color)
    {
        Transform = new Transform(new Vector3(50f, 800f, 0f), new Vector3(0f), new Vector3(500f, 800f, 0f));
        Header = header;
        Color = color;
        IsVisible = false;
        
        _startPos = Vector2i.Zero;
        _origoPos = Vector2i.Zero;

        /*var vertices = ObjFileLoader.CreateQuadVertices(1f);
        
        program.VertexBuffer.Bind();
        GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertices.Length * sizeof(float), vertices);
        program.VertexBuffer.Unbind();*/
    }

    public bool CheckCollision(Vector2i mouse)
    {
        return mouse.X >= Transform.Position.X && mouse.X <= Transform.Position.X + Transform.Scale.X &&
               mouse.Y <= Transform.Position.Y && mouse.Y >= Transform.Position.Y - 50f;
    }

    public bool MoveWindow(Mouse mouse)
    {
        if (!mouse.IsDown || (!CheckCollision(mouse.ScreenPosition) && !_isHeld))
        {
            _isHeld = false;
            return false;
        }

        if (!_isHeld)
        {
            _isHeld = true;
            _startPos.X = mouse.ScreenPosition.X;
            _startPos.Y = mouse.ScreenPosition.Y;
            _origoPos.X = (int)Transform.Position.X;
            _origoPos.Y = (int)Transform.Position.Y;
        }

        Transform.Position.X = _origoPos.X + mouse.ScreenPosition.X - _startPos.X;
        Transform.Position.Y = _origoPos.Y + mouse.ScreenPosition.Y - _startPos.Y;

        return true;
    }

    public void DrawWindow(ShaderProgram program, Vector2 viewport, Dictionary<string, FontMap> fonts)
    {
        var position = Vector3.Zero;
        position.X = Transform.Scale.X / 2f + Transform.Position.X;
        position.Y = -Transform.Scale.Y / 2f + Transform.Position.Y;
        
        var model = Matrix4.CreateScale(Transform.Scale / new Vector3(viewport, 1f)) *
                    Matrix4.CreateTranslation(position / new Vector3(viewport, 1f) * 2f - Vector3.One);
        
        program.SetUniform("color", Color);
        program.SetUniform("model", model);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        fonts["Cascadia"].DrawText(Header, new Vector2(Transform.Position.X + 25f, Transform.Position.Y - 40f), 0.5f, new Vector3(1f), viewport);
    }
}