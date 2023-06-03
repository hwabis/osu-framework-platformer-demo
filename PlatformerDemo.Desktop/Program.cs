using osu.Framework.Platform;
using osu.Framework;
using PlatformerDemo.Game;

namespace PlatformerDemo.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"PlatformerDemo"))
            using (osu.Framework.Game game = new PlatformerDemoGame())
                host.Run(game);
        }
    }
}
