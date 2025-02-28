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

    public MeshData MeshData;

    public TexturesPbr Textures;

    public bool IsVisible;

    public EngineObject(string name, Transform transform, MeshData meshData, TexturesPbr textures)
    {
        Name = name;
        Transform = transform;
        MeshData = meshData;
        Material = new Material(
            new Vector3(1f),
            new Vector3(1f),
            new Vector3(1f),
            32f
        );
        Textures = textures;
        IsVisible = true;
    }
    
    public EngineObject(string name, Transform transform, MeshData meshData, Material material)
    {
        Name = name;
        Transform = transform;
        MeshData = meshData;
        Material = material;
        Textures = new TexturesPbr();
        Textures.FillRest();
        IsVisible = true;
    }

    public static EngineObject CreateEmpty()
    {
        return new EngineObject(
            "N/A",
            new Transform(new Vector3(0f)),
            new MeshData([], []),
            new Material(new Vector3(1f))
        );
    }

    public Matrix4 GetModelMatrix()
    {
        //var model = Matrix4.CreateRotationX(Transform.Rotation.X) * Matrix4.CreateRotationY(Transform.Rotation.Y) * Matrix4.CreateRotationZ(Transform.Rotation.Z);
        var model = Matrix4.CreateFromQuaternion(Transform.Quaternion);
        model *= Matrix4.CreateScale(Transform.Scale) * Matrix4.CreateTranslation(Transform.Position);
        return model;
    }

    public void DrawObject(ShaderProgram program, int offset, Matrix4 view)
    {
        var model = GetModelMatrix();
        program.SetUniform("model", model);
        program.SetUniform("inverseModel", model.Inverted().Transposed());
        program.SetUniform("inverseModelView", (model * view).Inverted().Transposed());
        program.SetUniform("textureScaling", Textures.Scaling);
        
        program.SetUniform("material.color", Material.Color);
        program.SetUniform("material.diffuse", Material.Diffuse);
        program.SetUniform("material.specular", Material.Specular);
        program.SetUniform("material.shininess", Material.Shininess);
        
        program.SetUniform("material.diffuseMap", 0);
        program.SetUniform("material.specularMap", 1);
        program.SetUniform("material.normalMap", 2);
        program.SetUniform("material.hasNormalMap", Textures.ContainsType(TextureTypes.Normal) ? 1 : 0);
        Textures.ActiveAndBindAll();
        
        GL.DrawElements(MeshData.PrimitiveType, MeshData.Indices.Length, DrawElementsType.UnsignedInt, offset);
    }

}