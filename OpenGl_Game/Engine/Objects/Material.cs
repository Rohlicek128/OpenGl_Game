using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public struct Material
{
    public Vector3 Color;
    public Vector3 Ambient;
    public Vector3 Diffuse;
    public Vector3 Specular;
    public float Shininess;

    public Material(Vector3 color, Vector3 diffuse, Vector3 specular, float shininess)
    {
        Color = color;
        Diffuse = diffuse;
        Specular = specular;
        Shininess = shininess;
    }
    
    public Material(Vector3 color)
    {
        Color = color;
        Diffuse = new Vector3(1f);
        Specular = new Vector3(0.5f);
        Shininess = 32f;
    }
}