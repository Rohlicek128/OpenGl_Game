namespace OpenGl_Game.Engine.Graphics.Textures;

/*public class TextureTypes
{
    public string Value;

    public TextureTypes(string value)
    {
        Value = value;
    }

    public static TextureTypes Diffuse => new("diffuseMap");
    public static TextureTypes Specular => new("specularMap");
    public static TextureTypes Normal => new("normalMap");
    public static TextureTypes Overlay => new("overlay");
    public static TextureTypes Emissive => new("emissive");

    public override string ToString()
    {
        return Value;
    }
}*/

public enum TextureTypes
{
    Diffuse, Specular, Normal, Overlay, Emissive
}