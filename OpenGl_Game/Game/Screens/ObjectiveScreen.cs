using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Shaders.Programs;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Graphics.UI.Text;
using OpenGl_Game.Engine.Objects;
using OpenTK.Graphics.OpenGLES2;
using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Screens;

public class ObjectiveScreen : ScreenHandler
{
    public ObjectiveScreen(Vector2i screenResolution) : base(screenResolution)
    {
        EngineObject = new EngineObject(
            "Objective Screen",
            new Transform(new Vector3(-2.5346785f, -0.14945818f, -0.8248692f), new Vector3(-MathF.PI/2f, MathHelper.DegreesToRadians(-55), -MathF.PI/2f),
                new Vector3(0.6f, 0.05f, 0.6f)),
            MeshConstructor.CreateCube(),
            new TexturesPbr(new Dictionary<TextureTypes, Texture>
            {
                {TextureTypes.Diffuse, Framebuffer.AttachedTextures[0]},
                {TextureTypes.Overlay, new Texture("white1x1.png", 1)}
            })
        );
        IsTurnOn = false;
    }

    public override void RenderScreen(CollisionShader collision, ShaderProgram program, Matrix4 world, Matrix4 view, Vector2i viewport, FontMap font, float boost)
    {
        GL.Viewport(0, 0, ScreenResolution.X, ScreenResolution.Y);
        Framebuffer.Bind();

        if (IsTurnOn)
        {
            var bg = 0.2f;
            GL.ClearColor(bg, bg, bg, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            //program.DrawGeometryMesh(world, view);
            font.DrawText("STATION 42", new Vector2(25f, ScreenResolution.Y - 60f), 0.75f, new Vector4(1f), ScreenResolution);
            font.DrawText("ORBITAL LASER", new Vector2(25f, ScreenResolution.Y - 80f), 0.35f, new Vector4(1f), ScreenResolution);
            font.DrawText("SPEED: " + boost + "x", new Vector2(25f, ScreenResolution.Y - 150f), 0.8f, new Vector4(1f), ScreenResolution);

            if (collision.LookingAtObject.Id == EngineObject.Id)
            {
                font.DrawText("o", collision.LookingAtUv * ScreenResolution, 0.3f, new Vector4(1f), ScreenResolution);
            }
        }
        else
        {
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        
        Framebuffer.Unbind();
        GL.Viewport(0, 0, viewport.X, viewport.Y);
    }
}