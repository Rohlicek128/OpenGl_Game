using System.Drawing;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Engine.Objects;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform;
using Bitmap = System.Drawing.Bitmap;
using FramebufferAttachment = OpenTK.Graphics.OpenGL.Compatibility.FramebufferAttachment;
using TextureTarget = OpenTK.Graphics.OpenGL.Compatibility.TextureTarget;
using VertexAttribType = OpenGl_Game.Engine.Graphics.Buffers.VertexAttribType;

namespace OpenGl_Game.Engine.Graphics.Shadows;

public class Ssao
{
    public ShaderProgram Program;
    public Framebuffer Framebuffer;
    
    public Vector3[] SampleKernels;
    public Vector3[] Noise;
    public Texture NoiseTexture;

    public unsafe Ssao(Shader[] shaders, Vector2i viewport, int kernelCount, int noiseSize)
    {
        var screenQuad = EngineObject.CreateEmpty();
        screenQuad.MeshData.Vertices = MeshConstructor.CreateRenderQuad();
        Program = new ShaderProgram(shaders, [screenQuad], [new VertexAttribute(VertexAttribType.PosAndTex, 4)]);
        
        Framebuffer = new Framebuffer();
        Framebuffer.AttachTexture(
            new Texture(0, viewport, null, InternalFormat.Red, PixelType.Float, PixelFormat.Red),
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2d
        );
        
        var random = new Random();
        SampleKernels = CreateSampleKernels(random, kernelCount);
        NoiseTexture = CreateNoiseTexture(random, noiseSize);
    }

    public void RenderSsao(GBuffer gBuffer, Matrix4 projection, Vector2i viewport, float val, Matrix4 view)
    {
        Framebuffer.Bind();
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        Program.Use();
        Program.ArrayBuffer.Bind();
        
        gBuffer.BindBufferTextures();
        Program.SetUniform("gPosition", 0);
        Program.SetUniform("gNormal", 1);
        NoiseTexture.ActiveAndBind(3);
        Program.SetUniform("noiseTexture", 3);

        for (int i = 0; i < SampleKernels.Length; i++)
        {
            Program.SetUniform($"samples[{i}]", SampleKernels[i]);
        }
        Program.SetUniform("kernelSize", 64);
        Program.SetUniform("radius", val);
        Program.SetUniform("bias", 0.015f);
        
        Program.SetUniform("projection", projection);
        Program.SetUniform("view", view);
        Program.SetUniform("noiseScale", viewport / (int)Math.Sqrt(Noise.Length));
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        
        Program.ArrayBuffer.Unbind();
        Framebuffer.Unbind();
    }

    public Vector3[] CreateSampleKernels(Random random, int count)
    {
        var result = new Vector3[count];
        
        for (int i = 0; i < result.Length; i++)
        {
            var sample = new Vector3(
                random.NextSingle() * 2f - 1f,
                random.NextSingle() * 2f - 1f,
                random.NextSingle()
            );
            sample = Vector3.Normalize(sample);
            sample *= random.NextSingle();

            var scale = (float)i / result.Length;
            scale = Lerp(0.1f, 1f, scale * scale);
            sample *= scale;
            
            result[i] = sample;
        }

        return result;
    }

    public Vector3[] CreateNoise(Random random, int size)
    {
        var result = new Vector3[size * size];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new Vector3(
                random.NextSingle() * 2f - 1f,
                random.NextSingle() * 2f - 1f,
                0f
            );
        }

        return result;
    }

    public Texture CreateNoiseTexture(Random random, int noiseSize)
    {
        Noise = CreateNoise(random, noiseSize);

        /*var bitmap = new Bitmap(noiseSize, noiseSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        for (int i = 0; i < noiseSize; i++)
        {
            for (int j = 0; j < noiseSize; j++)
            {
                bitmap.SetPixel(i, j, Color.FromArgb(
                    0,
                    (int)((Noise[j + noiseSize * i].X + 1f) / 2f * 255),
                    (int)((Noise[j + noiseSize * i].Y + 1f) / 2f * 255),
                    (int)((Noise[j + noiseSize * i].Z + 1f) / 2f * 255)
                ));
            }
        }
        bitmap.Save(RenderEngine.DirectoryPath + @"Assets\noise.jpg");

        var texture = new Texture("noise.jpg", 0, format:InternalFormat.Rgba16f, type:PixelType.Float, pixel:PixelFormat.Rgb);*/
        
        var texture = new Texture(GL.GenTexture(), 0, InternalFormat.Rgba16f, PixelType.Float, PixelFormat.Rgb);
        GL.ActiveTexture(TextureUnit.Texture0 + texture.Index);
        texture.Bind();
        
        GL.TexParameteri(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, TextureParameterName.TextureWrapS, (int) OpenTK.Graphics.OpenGL.Compatibility.TextureWrapMode.Repeat);
        GL.TexParameteri(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, TextureParameterName.TextureWrapT, (int) OpenTK.Graphics.OpenGL.Compatibility.TextureWrapMode.Repeat);
        GL.TexParameteri(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int) OpenTK.Graphics.OpenGL.Compatibility.TextureMinFilter.Nearest);
        GL.TexParameteri(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int) OpenTK.Graphics.OpenGL.Compatibility.TextureMagFilter.Nearest);
        
        GL.TexImage2D(OpenTK.Graphics.OpenGL.TextureTarget.Texture2d, 0, texture.Format, 
            noiseSize, noiseSize, 0, texture.Pixel, texture.Type, Noise);

        texture.Unbind();
        return texture;
    }

    private float Lerp(float a, float b, float f)
    {
        return a + f * (b - a);
    }

    public void Delete()
    {
        Program.Delete();
        Framebuffer.Delete();
        NoiseTexture.Delete();
    }
    
}