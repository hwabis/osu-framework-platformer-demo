using osu.Framework.Testing;

namespace PlatformerDemo.Game.Tests.Visual
{
    public partial class PlatformerDemoTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new PlatformerDemoTestSceneTestRunner();

        private partial class PlatformerDemoTestSceneTestRunner : PlatformerDemoGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
