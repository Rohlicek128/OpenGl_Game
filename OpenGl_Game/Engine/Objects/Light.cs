using OpenGl_Game.Engine.Graphics.Buffers;
using OpenGl_Game.Shaders;
using OpenTK.Mathematics;

namespace OpenGl_Game.Engine.Objects;

public class Light : EngineObject
{
    public Vector3 AttenuationParams;
    public LightTypes Type;

    public Vector3 PbrParams;
    
    public Light(string name, Transform transform, VerticesData verticesData, uint[] indicesData, Material material, Vector3 pbrParams, Vector3 attenParams, LightTypes type)
        : base(name, transform, verticesData, indicesData, material)
    {
        Material.Ambient = Material.Color * pbrParams.X;
        Material.Diffuse = Material.Color * pbrParams.Y;
        Material.Specular = Material.Color * pbrParams.Z;
        AttenuationParams = attenParams;
        Type = type;
        PbrParams = pbrParams;
    }

    public void SetUniformsForDirectional(ShaderProgram program)
    {
        program.SetUniform("dirLight.direction", Transform.Rotation);
        program.SetUniform("dirLight.ambient", Material.Ambient);
        program.SetUniform("dirLight.diffuse", Material.Diffuse);
        program.SetUniform("dirLight.specular", Material.Specular);
    }
    
    public void SetUniformsForPoint(ShaderProgram program, int index)
    {
        program.SetUniform($"pointLight[{index}].position", Transform.Position);
        program.SetUniform($"pointLight[{index}].ambient", Material.Color * PbrParams.X);
        program.SetUniform($"pointLight[{index}].diffuse", Material.Color * PbrParams.Y);
        program.SetUniform($"pointLight[{index}].specular", Material.Color * PbrParams.Z);
        program.SetUniform($"pointLight[{index}].attenParams", AttenuationParams);
    }

    public static List<Light> LightsDicToList(Dictionary<LightTypes, List<Light>> lights)
    {
        var result = new List<Light>();
        foreach (var lightList in lights)
        {
            result.AddRange(lightList.Value);
        }

        return result;
    }

    public static Vector3 HsvToRgb(Vector3 hsv)
    {
        if (hsv.X < 0) hsv.X += 360;
        if (hsv.X > 360) hsv.X = (hsv.X % 360);

        var c = hsv.Z * hsv.Y;
        var x = c * (1 - Math.Abs((hsv.X / 60) % 2 - 1));
        var m = hsv.Z - c;

        float rPrime = 0, gPrime = 0, bPrime = 0;

        switch (hsv.X)
        {
            case >= 0 and < 60:
                rPrime = c;
                gPrime = x;
                bPrime = 0;
                break;
            case >= 60 and < 120:
                rPrime = x;
                gPrime = c;
                bPrime = 0;
                break;
            case >= 120 and < 180:
                rPrime = 0;
                gPrime = c;
                bPrime = x;
                break;
            case >= 180 and < 240:
                rPrime = 0;
                gPrime = x;
                bPrime = c;
                break;
            case >= 240 and < 300:
                rPrime = x;
                gPrime = 0;
                bPrime = c;
                break;
            case >= 300 and < 360:
                rPrime = c;
                gPrime = 0;
                bPrime = x;
                break;
        }
        
        var r = rPrime + m;
        var g = gPrime + m;
        var b = bPrime + m;

        return new Vector3(Math.Clamp(r, 0, 1), Math.Clamp(g, 0, 1), Math.Clamp(b, 0, 1));
    }
}