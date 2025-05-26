using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Game.Buttons;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.UI.Elements;

public class UiButton : ButtonHandler, UiElement
{
    public UiRectangle UiShape { get; set; }
    
    public UiButton(Vector3 position, Vector3 color, float width, float height, EventHandler e = null)
    {
        if (e == null) AddEvent(MyEvent);
        else AddEvent(e);

        UiShape = new UiRectangle(position, color, width, height);
        UiShape.EngineObject.Name = "Button #" + EngineObject.ObjectIdCounter;

        EngineObject = UiShape.EngineObject;
        Type = ButtonTypes.Press;
    }

    public bool PointCollision(Vector2 point)
    {
        return point.X >= EngineObject.Transform.Position.X - EngineObject.Transform.Scale.X / 2f && point.X <= EngineObject.Transform.Position.X + EngineObject.Transform.Scale.X / 2f &&
               point.Y >= EngineObject.Transform.Position.Y - EngineObject.Transform.Scale.Y / 2f && point.Y <= EngineObject.Transform.Position.Y + EngineObject.Transform.Scale.Y / 2f;
    }

    private void SetButtonValue(bool down)
    {
        if (down)
        {
            if (ButtonValue >= 1f) ButtonValue = 0.6f;
            else if (ButtonValue <= 0f) ButtonValue = 0.4f;
        }
        else
        {
            if (Math.Abs(ButtonValue - 0.6f) < 0.001f) ButtonValue = 0f;
            else if (Math.Abs(ButtonValue - 0.4f) < 0.001f) ButtonValue = 1f;
        }
    }

    private protected override void MyEvent(object sender, params object?[] param)
    {
        if (param[0] == null) return;
        SetButtonValue((bool)param[0]);

        //if (ButtonValue >= 1f) EngineObject.Material.Color.Y = 1f;
        //else if (ButtonValue <= 0f) EngineObject.Material.Color.Y = 0f;
    }

    public EngineObject GetEngineObject()
    {
        return EngineObject;
    }
}