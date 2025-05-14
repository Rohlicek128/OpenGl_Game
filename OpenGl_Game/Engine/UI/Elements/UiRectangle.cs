using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.UI.Elements;

public class UiRectangle : UiElement
{
    public EngineObject EngineObject { get; set; }
    
    public UiRectangle(Vector3 position, Vector3 color, float width, float height)
    {
        EngineObject = new EngineObject(
            "Rectangle #" + (EngineObject.ObjectIdCounter + 1),
            new Transform(position, Vector3.Zero, new Vector3(width, height, 0f)),
            UiGraphics.GenMeshFromSize(isCentered:true),
            new Material(color)
        );
    }
    public UiRectangle(Vector3 position, Texture texture, float width, float height)
    {
        EngineObject = new EngineObject(
            "Rectangle #" + (EngineObject.ObjectIdCounter + 1),
            new Transform(position, Vector3.Zero, new Vector3(width, height, 0f)),
            UiGraphics.GenMeshFromSize(isCentered:true),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                { TextureTypes.Diffuse, texture }
            })
        );
    }
    
    public EngineObject GetEngineObject()
    {
        return EngineObject;
    }
}