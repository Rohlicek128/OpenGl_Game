using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Screens.Navigation;

public class NavigationScreen : ScreenHandler
{
    public NavigationScreen(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Navigation Screen",
            new Transform(
                //new Vector3(-2.5346785f, -0.14945818f, -1.8248692f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
                new Vector3(-0.951065f, 0.9476954f, -0.59256047f), new Vector3(-1.9985559f, -0.20022066f, 0.3939149f),
                new Vector3(0.45f, 0.05f, 0.45f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]},
                {TextureTypes.Emissive, new Texture("white1x1.png", 4)}
            })
        );
        
        Pages.Add(new MapPage(screenResolution, EngineObject.Id));
        Pages.Add(new AimPage(new Vector2i(300, 300), EngineObject.Id));
    }
}