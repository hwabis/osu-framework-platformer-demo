using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using Zlelda.Game;

namespace PlatformerDemo.Game
{
    public partial class MainScreen : Screen
    {
        private CameraContainer camera;
        private Container<Path> paths;
        private SpriteText text;
        private BindableBool isGrounded = new BindableBool();
        private PlayerKeyBindingContainer playerContainer;
        private Player player;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Violet,
                    RelativeSizeAxes = Axes.Both,
                },
                text = new SpriteText
                {
                    Y = 20,
                    Text = "NO",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FontUsage.Default.With(size: 40)
                },
                camera = new CameraContainer
                {
                    Children = new Drawable[]
                    {
                        paths = new Container<Path>
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new Path[]
                            {
                                // This path stuff is kind of a mess, wonder if there's a better way for the coordinates
                                // so that you don't have to figure out what the correct positions and origins are
                                new Path
                                {
                                    Origin = Anchor.BottomLeft,
                                    X = 0,
                                    Y = 300,
                                },
                                new Path
                                {
                                    Origin = Anchor.BottomLeft,
                                    X = 0,
                                    Y = 200
                                },
                                new Path
                                {
                                    Origin = Anchor.BottomLeft,
                                    X = 50,
                                    Y = 150
                                },
                                new Path
                                {
                                    Origin = Anchor.TopLeft,
                                    X = 400,
                                    Y = 100
                                },
                            }
                        },
                        playerContainer = new PlayerKeyBindingContainer
                        {
                            Child = player = new Player
                            {
                                Height = 100,
                                Width = 50,
                                Paths = paths
                            }
                        }
                    }
                }
            };

            paths[0].AddVertex(new Vector2(0, 0));
            paths[0].AddVertex(new Vector2(400, 0));
            paths[0].AddVertex(new Vector2(500, -75));
            paths[0].AddVertex(new Vector2(600, -75));

            paths[1].AddVertex(new Vector2(0, 0));
            paths[1].AddVertex(new Vector2(100, 0));

            paths[2].AddVertex(new Vector2(0, 0));
            paths[2].AddVertex(new Vector2(100, 0));

            paths[3].AddVertex(new Vector2(0, 0));
            paths[3].AddVertex(new Vector2(100, 50));
            paths[3].AddVertex(new Vector2(200, 50));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isGrounded.BindTo(player.IsGrounded);
            isGrounded.ValueChanged += (ValueChangedEvent<bool> v) => text.Text = isGrounded.Value ? "YES" : "NO";
        }

        protected override void Update()
        {
            base.Update();

            camera.TargetPosition = playerContainer.Position + player.Position;
        }
    }
}
