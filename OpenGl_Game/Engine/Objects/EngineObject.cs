using OpenGl_Game.Engine.Graphics;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public class EngineObject
{
    public string Name;

    public Transform Transform;
    public Material Material;
    
    public VerticesData VerticesData;
    public uint[] IndicesData;
    
    public TexturesPbr Textures;

    public bool IsVisible = true;

    public EngineObject(string name, Vector3 position, VerticesData verticesData, uint[] indicesData, TexturesPbr textures)
    {
        Name = name;
        Transform = new Transform(position);
        VerticesData = verticesData;
        IndicesData = indicesData;
        Material = new Material(
            new Vector3(1f),
            new Vector3(1f),
            new Vector3(1f),
            32f
        );
        Textures = textures;
    }
    
    public EngineObject(string name, Vector3 position, VerticesData verticesData, uint[] indicesData, Material material)
    {
        Name = name;
        Transform = new Transform(position);
        VerticesData = verticesData;
        IndicesData = indicesData;
        Material = material;
        Textures = new TexturesPbr();
        Textures.FillRest();
    }

    public static EngineObject CreateEmpty()
    {
        return new EngineObject(
            "N/A",
            new Vector3(0f),
            new VerticesData([], PrimitiveType.Triangles),
            [],
            new Material(new Vector3(1f))
        );
    }

    public Matrix4 GetModelMatrix()
    {
        var model = Matrix4.CreateRotationX(Transform.Rotation.X) * Matrix4.CreateRotationY(Transform.Rotation.Y) * Matrix4.CreateRotationZ(Transform.Rotation.Z);
        model *= Matrix4.CreateScale(Transform.Scale) * Matrix4.CreateTranslation(Transform.Position);
        return model;
    }

    public void DrawObject(ShaderProgram program, Camera camera, int offset)
    {
        var model = GetModelMatrix();
        program.SetUniform("model", model);
        program.SetUniform("inverseModel", model.Inverted().Transposed());
        
        program.SetUniform("material.color", Material.Color);
        program.SetUniform("material.diffuse", Material.Diffuse);
        program.SetUniform("material.specular", Material.Specular);
        program.SetUniform("material.shininess", Material.Shininess);
        
        program.SetUniform("viewPos", camera.Transform.Position);
        
        program.SetUniform("material.diffuseMap", 0);
        program.SetUniform("material.specularMap", 1);
        Textures.ActiveAndBindAll();
        
        GL.DrawElements(VerticesData.Type, IndicesData.Length, DrawElementsType.UnsignedInt, offset);
    }

}