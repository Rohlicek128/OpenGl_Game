using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Engine.UI.Elements;
using OpenGl_Game.Game.Targets;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl_Game.Game.Screens.Objective;

public class ObjectiveScreen : ScreenHandler
{
    public ObjectiveScreen(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Objective Screen",
            new Transform(new Vector3(-0.9748638f, 0.9476954f, 0.65129125f), new Vector3(-1.1603967f, -0.18453844f, 2.7553217f),
                new Vector3(0.45f, 0.05f, 0.45f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]},
                {TextureTypes.Emissive, new Texture("white1x1.png", 4)}
            })
        );
        
        Pages.Add(new ObjectivePage(screenResolution, EngineObject.Id));
        Pages.Add(new LogPage(screenResolution, EngineObject.Id));
    }

    public void SetCoords(Vector2 coords)
    {
        ((LogPage)Pages[1]).CurrentCoords = coords;
    }
}