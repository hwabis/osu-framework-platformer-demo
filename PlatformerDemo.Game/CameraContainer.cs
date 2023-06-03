﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace Zlelda.Game
{
    // TODO: Stops following at the edges
    public partial class CameraContainer : Container
    {
        public Vector2 TargetPosition = Vector2.Zero;
        public float SmoothingFactor = 0.01f;

        public CameraContainer()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.TopLeft;
        }

        protected override void Update()
        {
            base.Update();

            Position = Vector2.Lerp(Position, -TargetPosition, SmoothingFactor * (float)Time.Elapsed);
        }
    }
}
