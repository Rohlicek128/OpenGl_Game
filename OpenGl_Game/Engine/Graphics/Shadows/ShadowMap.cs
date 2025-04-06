using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using InternalFormat = OpenTK.Graphics.OpenGL.InternalFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;

namespace OpenGl_Game.Engine.Graphics.Shadows;

public class ShadowMap : ShaderProgram
{
    public Framebuffer DepthMapFramebuffer;
    public readonly int TextureHandle;
    public Vector2i ShadowSize;
    public float MaxDistance;
    public float CameraOffset;
    public Vector2 PlaneDims;
    
    public Matrix4 LightSpace;

    public ShadowMap(Vector2i shadowSize, float maxDistance, float cameraOffset, Vector2 planeDims, ShaderProgram sceneProgram) : base([
            new Shader(@"ShadowDepthShaders\shadowDepth.vert", ShaderType.VertexShader),
            new Shader(@"ShadowDepthShaders\shadowDepth.frag", ShaderType.FragmentShader)
        ], sceneProgram.Objects, sceneProgram.VertexBuffer.Attributes
    )
    {
        ShadowSize = shadowSize;
        MaxDistance = maxDistance;
        CameraOffset = cameraOffset;
        PlaneDims = planeDims;
        
        DepthMapFramebuffer = new Framebuffer();
        TextureHandle = GenerateDepthTexture();
        DepthMapFramebuffer.AttachTexture(TextureHandle, OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment.DepthAttachment, OpenTK.Graphics.OpenGL.Compatibility.TextureTarget.Texture2d);
        
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
        
        GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.DepthComponent32, 
            ShadowSize.X, ShadowSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);

        return handle;
    }

    public override void SetUniforms(params object[] param)
    {
        LightSpace = GetLightSpaceMatrix((Light)param[0], (Camera)param[1]);
        SetUniform("lightSpace", LightSpace);
    }

    public override void Draw(params object[] param)
    {
        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        //GL.CullFace(TriangleFace.Front);
        //GL.Disable(EnableCap.CullFace);

        var scene = (ShaderProgram)param[3];
        Use();
        scene.ArrayBuffer.Bind();
        scene.IndexBuffer.Bind();
        
        SetUniforms(param[0], param[1]);
        
        GL.Viewport(0, 0, ShadowSize.X, ShadowSize.Y);
        DepthMapFramebuffer.Bind();
        GL.Clear(ClearBufferMask.DepthBufferBit);
        
        var offset = 0;
        foreach (var engineObject in Objects)
        {
            if (engineObject.IsVisible && engineObject.IsShadowVisible)
            {
                var model = engineObject.GetModelMatrix();
                SetUniform("model", model);
                GL.DrawElements(engineObject.MeshData.PrimitiveType, engineObject.MeshData.Indices.Length, DrawElementsType.UnsignedInt, offset);
            }
            offset += engineObject.MeshData.Indices.Length * sizeof(uint);
        }
        
        DepthMapFramebuffer.Unbind();
        var viewport = (Vector2i)param[2];
        GL.Viewport(0, 0, viewport.X, viewport.Y);
        GL.CullFace(TriangleFace.Back);
        //GL.Enable(EnableCap.CullFace);
    }

    public override void DeleteAll()
    {
        Delete();
        foreach (var texture in DepthMapFramebuffer.AttachedTextures) texture.Delete();
        DepthMapFramebuffer.Delete();
    }

    public Matrix4 GetLightSpaceMatrix(Light dirLight, Camera camera)
    {
        //var aspect =  (float)ShadowSize.X / ShadowSize.Y;
        //var frustumSize = 1000f;
        //var lightProjection = Matrix4.CreateOrthographic(frustumSize / 2f,  frustumSize / 2f, 0.01f, 250f);
        
        var lightProjection = Matrix4.CreateOrthographic(MaxDistance, MaxDistance, PlaneDims.X, PlaneDims.Y);
        var lightView = Matrix4.LookAt(
            (-Vector3.UnitY * Matrix3.CreateFromQuaternion(dirLight.Transform.Quaternion)) * CameraOffset,
            Vector3.Zero, 
            new Vector3(0f, 1f, 0f)
        );
        return lightView * lightProjection;
    }
}