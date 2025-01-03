namespace OpenGl_Game.Engine.Graphics.Textures;

public class TextureTypes
{
    public string Value;

    public TextureTypes(string value)
    {
        Value = value;
    }

    public static TextureTypes Diffuse => new("diffuseMap");
    public static TextureTypes Specular => new("specularMap");

    public override string ToString()
    {
        return Value;
    }
}