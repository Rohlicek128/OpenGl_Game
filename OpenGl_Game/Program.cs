using OpenGl_Game.Engine;

namespace OpenGl_Game;

class Program
{
    static void Main(string[] args)
    {
        using var engine = new RenderEngine(1080, 1080, "OpenGl Game");
        engine.Run();
    }
}