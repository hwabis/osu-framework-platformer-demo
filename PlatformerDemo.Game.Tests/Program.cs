using osu.Framework;
using osu.Framework.Platform;

namespace PlatformerDemo.Game.Tests
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost("visual-tests"))
            using (var game = new PlatformerDemoTestBrowser())
                host.Run(game);
        }
    }
}
