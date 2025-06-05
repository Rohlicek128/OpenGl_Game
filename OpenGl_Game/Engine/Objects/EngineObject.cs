using OpenGl_Game.Engine.Graphics;
using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Engine.Graphics.Shaders;
using OpenGl_Game.Engine.Graphics.Textures;
using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public class EngineObject
{
    public int Id;
    public static int ObjectIdCounter = 0;
    
    public string Name;

    public Transform Transform;
    public Material Material;

    public MeshData MeshData;

    public TexturesPbr Textures;

    public bool IsVisible;
    public bool IsSelectable;
    public bool IsShadowVisible;
    public bool IsInverted;
    public int VisibleForId;
    
    public float PointSize;

    public EngineObject(string name, Transform transform, MeshData meshData, TexturesPbr textures, bool hasId = true, float pointSize = 1f)
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
        IsSelectable = true;
        IsShadowVisible = true;
        IsInverted = false;
        VisibleForId = 0;

        PointSize = pointSize;

        if (hasId)
        {
            ObjectIdCounter++;
            Id = ObjectIdCounter;
        }
    }

    public EngineObject(string name, Transform transform, MeshData meshData, Material material, bool hasId = true, float pointSize = 1f)
    {
        Name = name;
        Transform = transform;
        MeshData = meshData;
        Material = material;
        Textures = new TexturesPbr();
        Textures.FillRest();
        IsVisible = true;
        IsSelectable = true;
        IsShadowVisible = true;
        IsInverted = false;
        VisibleForId = 0;
        
        PointSize = pointSize;

        if (hasId)
        {
            ObjectIdCounter++;
            Id = ObjectIdCounter;
        }
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

    /// <summary>
    /// Creates the Model matrix for the object (Scale -> Origin -> Rotation -> Translation/Position)
    /// </summary>
    /// <returns>Model Matrix</returns>
    public Matrix4 GetModelMatrix()
    {
        //var model = Matrix4.CreateRotationX(Transform.Rotation.X) * Matrix4.CreateRotationY(Transform.Rotation.Y) * Matrix4.CreateRotationZ(Transform.Rotation.Z);
        var model =
            Matrix4.CreateScale(Transform.Scale) *
            Matrix4.CreateTranslation(Transform.Origin) *
            Matrix4.CreateFromQuaternion(Transform.Quaternion) *
            Matrix4.CreateTranslation(Transform.Position);
        return model;
    }

    /// <summary>
    /// Draws the object to the screen (uses 1 draw call)
    /// </summary>
    /// <param name="program"></param>
    /// <param name="offset"></param>
    /// <param name="view"></param>
    public void DrawObject(ShaderProgram program, int offset, Matrix4 view)
    {
        if (IsInverted) GL.CullFace(TriangleFace.Front);
        program.SetUniform("vecEoId", Id / (float)RenderEngine.MaxObjectIds);
        
        var model = GetModelMatrix();
        program.SetUniform("model", model);
        program.SetUniform("inverseModel", model.Inverted().Transposed());
        program.SetUniform("inverseModelView", (model * view).Inverted().Transposed());
        program.SetUniform("textureScaling", Textures.Scaling);
        
        program.SetUniform("material.color", Material.Color.Xyz);
        program.SetUniform("material.color", Material.Color);
        program.SetUniform("material.diffuse", Material.Diffuse);
        program.SetUniform("material.specular", Material.Specular);
        program.SetUniform("material.shininess", Material.Shininess);
        
        program.SetUniform("material.diffuseMap", 0);
        program.SetUniform("material.specularMap", 1);
        program.SetUniform("material.normalMap", 2);
        program.SetUniform("material.overlay", 3);
        program.SetUniform("material.emissive", 4);
        program.SetUniform("material.hasNormalMap", Textures.ContainsType(TextureTypes.Normal) ? 1 : 0);
        Textures.ActiveAndBindAll();
        
        Draw(offset);
        
        Textures.UnbindAll();
        if (IsInverted) GL.CullFace(TriangleFace.Back);
    }

    public void Draw(int offset)
    {
        GL.PointSize(PointSize);
        GL.DrawElements(MeshData.PrimitiveType, MeshData.Indices.Length, DrawElementsType.UnsignedInt, offset);
    }

}