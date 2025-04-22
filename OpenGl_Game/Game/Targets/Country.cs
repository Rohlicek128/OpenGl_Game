using OpenTK.Mathematics;

namespace OpenGl_Game.Game.Targets;

public struct Country
{
    public string Name { get; set; }
    public string Code3 { get; set; }
    public Vector2 Center { get; set; }

    public Country(string name, string code3, Vector2 center)
    {
        Name = name;
        Code3 = code3;
        Center = center;
    }
}