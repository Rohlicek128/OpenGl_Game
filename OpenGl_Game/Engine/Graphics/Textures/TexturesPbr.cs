using OpenGl_Game.Shaders;
using OpenTK.Graphics.OpenGL;

namespace OpenGl_Game.Engine.Graphics.Textures;

public class TexturesPbr
{
    public Dictionary<TextureTypes, Texture> Textures;
    public float Scaling;

    public TexturesPbr()
    {
        Textures = new Dictionary<TextureTypes, Texture>();
        Scaling = 0f;
    }

    public TexturesPbr(Dictionary<TextureTypes, Texture> textures)
    {
        Textures = textures;
        Scaling = 0f;
    }

    public void FillRest()
    {
        if (!Textures.ContainsKey(TextureTypes.Diffuse)) AddTexture(TextureTypes.Diffuse, new Texture("white1x1.png", 0));
        if (!Textures.ContainsKey(TextureTypes.Specular)) AddTexture(TextureTypes.Specular, new Texture("white1x1.png", 1));
        //if (!Textures.ContainsKey(TextureTypes.Normal)) AddTexture(TextureTypes.Normal, new Texture("black1x1.png", 2));
    }

    public bool ContainsType(TextureTypes type)
    {
        return Textures.Any(texture => texture.Key.Value.Equals(type.Value));
    }

    public void AddTexture(TextureTypes type, Texture texture)
    {
        Textures.TryAdd(type, texture);
    }

    public void ActiveAndBindAll()
    {
        foreach (var texture in Textures) texture.Value.ActiveAndBind();
        
        /*Textures[TextureTypes.Diffuse].ActiveAndBind(TextureUnit.Texture0);
        program.SetUniform(TextureTypes.Diffuse.Value, 0);
        
        Textures[TextureTypes.Specular].ActiveAndBind(TextureUnit.Texture1);
        program.SetUniform(TextureTypes.Specular.Value, 1);*/
    }

    public void DeleteAll()
    {
        foreach (var texture in Textures) texture.Value.Delete();
    }
}