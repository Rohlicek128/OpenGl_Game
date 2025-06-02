using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Screens;

public abstract class ScreenPage
{
    private UiGraphics _uiGraphics;
    public UiGraphics UiGraphics
    {
        get => _uiGraphics;
        set => _uiGraphics = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    private Vector2i _screenResolution;
    public Vector2i ScreenResolution
    {
        get => _screenResolution;
        set => _screenResolution = value;
    }
    
    private int _screenObjectId;
    public int ScreenObjectId
    {
        get => _screenObjectId;
        set => _screenObjectId = value;
    }

    protected ScreenPage(Vector2i screenResolution, int screenObjectId)
    {
        _screenResolution = screenResolution;
        _screenObjectId = screenObjectId;
        _uiGraphics = new UiGraphics();
    }

    public virtual void RenderPage(CollisionShader collision, Mouse mouse, Vector2i viewport, Dictionary<string, FontMap> fonts, float deltaTime)
    {
        
    }
}