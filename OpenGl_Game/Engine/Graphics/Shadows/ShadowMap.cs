using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL.Compatibility;
using OpenTK.Mathematics;
using InternalFormat = OpenTK.Graphics.OpenGL.InternalFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;

namespace OpenGl_Game.Engine.Graphics.Shadows;

public class ShadowMap
{
    public Framebuffer DepthMapFramebuffer;
    public readonly int TextureHandle;
    public Vector2i ShadowSize;
    public float ShadowMaxDistance;

    public ShaderProgram ShadowProgram;
    public Matrix4 LightSpace;

    public ShadowMap(Vector2i shadowSize, float maxDistance, ShaderProgram sceneProgram)
    {
        ShadowProgram = new ShaderProgram([
            new Shader(@"ShadowDepthShaders\shadowDepth.vert", (OpenTK.Graphics.OpenGL.ShaderType)ShaderType.VertexShader),
            new Shader(@"ShadowDepthShaders\shadowDepth.frag", (OpenTK.Graphics.OpenGL.ShaderType)ShaderType.FragmentShader)
        ], sceneProgram.Objects, sceneProgram.VertexBuffer.Attributes);
        
        ShadowSize = shadowSize;
        ShadowMaxDistance = maxDistance;
        DepthMapFramebuffer = new Framebuffer();
        TextureHandle = GenerateDepthTexture();
        DepthMapFramebuffer.AttachTexture(TextureHandle, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2d);
        
        DepthMapFramebuffer.Bind();
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        DepthMapFramebuffer.Unbind();
    }

    public unsafe int GenerateDepthTexture()
    {
        var handle = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2d, handle);

        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToBorder);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToBorder);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
        
        GL.TexParameterf(TextureTarget.Texture2d, TextureParameterName.TextureBorderColor, [1f, 1f, 1f, 1f]);
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, (OpenTK.Graphics.OpenGL.Compatibility.InternalFormat)InternalFormat.DepthComponent, 
            ShadowSize.X, ShadowSize.Y, 0, (OpenTK.Graphics.OpenGL.Compatibility.PixelFormat)PixelFormat.DepthComponent, (OpenTK.Graphics.OpenGL.Compatibility.PixelType)PixelType.Float, null);

        return handle;
    }

    public void RenderDepthMap(Light light, Vector2i viewport, ShaderProgram sceneProgram, Camera camera)
    {
        GL.CullFace(TriangleFace.Front);
        
        ShadowProgram.Use();
        sceneProgram.ArrayBuffer.Bind();
        sceneProgram.IndexBuffer.Bind();
        
        LightSpace = GetLightSpaceMatrix(light, camera);
        ShadowProgram.SetUniform("lightSpace", LightSpace);
        
        GL.Viewport(0, 0, ShadowSize.X, ShadowSize.Y);
        DepthMapFramebuffer.Bind();
        GL.Clear(ClearBufferMask.DepthBufferBit);
        
        var offset = 0;
        foreach (var engineObject in ShadowProgram.Objects)
        {
            if (engineObject.IsVisible)
            {
                var model = engineObject.GetModelMatrix();
                ShadowProgram.SetUniform("model", model);
                GL.DrawElements(PrimitiveType.Triangles, engineObject.IndicesData.Length, DrawElementsType.UnsignedInt, offset);
            }
            offset += engineObject.IndicesData.Length * sizeof(uint);
        }
        
        DepthMapFramebuffer.Unbind();
        GL.Viewport(0, 0, viewport.X, viewport.Y);
        GL.CullFace(TriangleFace.Back);
    }

    public Matrix4 GetLightSpaceMatrix(Light dirLight, Camera camera)
    {
        var lightProjection = Matrix4.CreateOrthographic(-ShadowMaxDistance, ShadowMaxDistance, 0.01f, 20f);
        var lightView = Matrix4.LookAt(
            dirLight.Transform.Position + camera.Transform.Position,
            camera.Transform.Position,
            new Vector3(0f, 1f, 0f)
        );
        return lightView * lightProjection;
    }
}