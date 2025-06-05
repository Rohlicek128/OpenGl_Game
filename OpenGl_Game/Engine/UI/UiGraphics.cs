using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;

namespace OpenGl_Game.Engine.UI;

public class UiGraphics
{
    public UiGraphicsProgram GraphicsProgram { get; private set; }
    
    public Dictionary<string, UiElement> Elements { get; set; }

    public UiGraphics()
    {
        Elements = [];
    }

    /// <summary>
    /// After all elements have been added, it creates a buffer on the GPU for them to be stored in
    /// </summary>
    public void InitProgram()
    {
        var objects = Elements.Values.Select(e => e.GetEngineObject()).ToList();
        
        GraphicsProgram = new UiGraphicsProgram(objects);
    }

    public static MeshData GenMeshFromSize(float width = 1f, float height = 1f, float texScaling = 1f, bool isCentered = true)
    {
        width /= 2f;
        height /= 2f;

        var c = 1f;
        if (!isCentered)
        {
            width *= 2f;
            height *= 2f;
            c = 0f;
        }
        
        float[] vertices =
        [
            -width * c,  height,  0.0f, texScaling,
            -width * c, -height * c,  0.0f, 0.0f,
            width, -height * c,  texScaling, 0.0f,
            //-width * c,  height,  0.0f, texScaling,
            //width, -height * c,  texScaling, 0.0f,
            width,  height,  texScaling, texScaling
        ];
        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0
        ];

        return new MeshData(vertices, indices);
    }
}