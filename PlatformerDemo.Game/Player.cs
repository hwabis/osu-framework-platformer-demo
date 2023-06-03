using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;
using Path = osu.Framework.Graphics.Lines.Path;

namespace Zlelda.Game
{
    public partial class Player : Box, IKeyBindingHandler<InputAction>
    {
        public Container<Path> Paths;
        public BindableBool IsGrounded = new BindableBool();

        private Vector2 velocity;
        private const float ground_acceleration_factor = 0.01f;
        private const int target_walk_velocity = 200;
        private const float air_acceleration_factor = 0.001f;

        private const float jumpVelocity = -600f;

        private const float minimum_velocity = 0.01f;

        private bool holdingLeft;
        private bool holdingRight;

        public Player()
        {
            Origin = Anchor.BottomCentre;
        }

        protected override void Update()
        {
            base.Update();

            float accelerationFactor = IsGrounded.Value ? ground_acceleration_factor : air_acceleration_factor;

            if (holdingRight && !holdingLeft)
                velocity.X = (float)Interpolation.Lerp(velocity.X, target_walk_velocity, accelerationFactor * (float)Time.Elapsed);
            else if (!holdingRight && holdingLeft)
                velocity.X = (float)Interpolation.Lerp(velocity.X, -target_walk_velocity, accelerationFactor * (float)Time.Elapsed);
            else
            {
                velocity.X = (float)Interpolation.Lerp(velocity.X, 0, accelerationFactor * (float)Time.Elapsed);

                if (Math.Abs(velocity.X) < minimum_velocity)
                    velocity.X = 0;
            }

            bool pointIsOnAPath = false;

            foreach (Path path in Paths)
            {
                if (isPointOnPath(Position, path))
                {
                    pointIsOnAPath = true;
                    break;
                }
            }

            if (!pointIsOnAPath)
                velocity.Y += 1.5f * (float)Time.Elapsed;

            Vector2 pendingPosition = Position + velocity * (float)Time.Elapsed * 0.001f;

            foreach (Path path in Paths)
            {
                float pathLeftVertexX = path.Vertices[0].X + path.Position.X;
                float pathRightVertexX = path.Vertices.Last().X + path.Position.X;
                bool pendingPositionIsWithinPath = pendingPosition.X >= pathLeftVertexX && pendingPosition.X <= pathRightVertexX;

                bool comingFromBelowFromAir = !isPointOnPath(Position, path) && velocity.Y <= 0;

                if (isLineCrossingPath(path, Position, pendingPosition, out Vector2 newPosition) && pendingPositionIsWithinPath && !comingFromBelowFromAir)
                {
                    Position = newPosition;
                    velocity.Y = (float)Interpolation.Lerp(velocity.Y, 0, ground_acceleration_factor * (float)Time.Elapsed);

                    if (Math.Abs(velocity.Y) < minimum_velocity)
                        velocity.Y = 0;

                    IsGrounded.Value = true;
                    return;
                }
            }

            Position = pendingPosition;
            IsGrounded.Value = false;
        }

        public bool OnPressed(KeyBindingPressEvent<InputAction> e)
        {
            switch (e.Action)
            {
                case InputAction.Jump:
                    if (e.Repeat || !IsGrounded.Value)
                        return false;
                    Position = new Vector2(Position.X, Position.Y - 1); // Hacky...
                    velocity.Y = jumpVelocity;
                    break;
                case InputAction.Right:
                    if (e.Repeat)
                        return false;
                    holdingRight = true;
                    break;
                case InputAction.Left:
                    if (e.Repeat)
                        return false;
                    holdingLeft = true;
                    break;
            }

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<InputAction> e)
        {
            switch (e.Action)
            {
                case InputAction.Right:
                    holdingRight = false;
                    break;
                case InputAction.Left:
                    holdingLeft = false;
                    break;
            }
        }

        // A lot of the code here and below was written with the help of ChatGPT :3
        private static bool isLineCrossingPath(Path path, Vector2 lineStart, Vector2 lineEnd, out Vector2 newPosition)
        {
            var vertexList = getPathVerticesToPathSpace(path);

            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                Vector2 b1 = vertexList[i];
                Vector2 b2 = vertexList[i + 1];

                if (isPointOnLine(lineStart, b1, b2))
                {
                    newPosition = getClosestPointOnPath(lineEnd, path);
                    return true;
                }

                if (isPointOnLine(lineEnd, b1, b2))
                {
                    newPosition = lineEnd;
                    return true;
                }

                if (doLinesIntersect(lineStart, lineEnd, b1, b2))
                {
                    newPosition = getClosestPointOnPath(lineEnd, path);
                    return true;
                }
            }

            newPosition = Vector2.Zero;
            return false;
        }

        private static bool isPointOnPath(Vector2 point, Path path)
        {
            var vertexList = getPathVerticesToPathSpace(path);

            for (int i = 0; i < vertexList.Count - 1; i++)
            {
                Vector2 b1 = vertexList[i];
                Vector2 b2 = vertexList[i + 1];

                if (isPointOnLine(point, b1, b2))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool isPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            float lineLength = Vector2.Distance(lineStart, lineEnd);
            float distanceFromStart = Vector2.Distance(lineStart, point);
            float distanceFromEnd = Vector2.Distance(lineEnd, point);

            return Math.Abs(lineLength - (distanceFromStart + distanceFromEnd)) < 0.0001;
        }

        private static bool doLinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float denom = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);
            if (denom == 0)
            {
                // Lines are parallel
                return false;
            }

            float ua = ((b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X)) / denom;
            float ub = ((a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X)) / denom;

            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
        }

        private static Vector2 getClosestPointOnPath(Vector2 point, Path path)
        {
            var vertices = getPathVerticesToPathSpace(path);

            float shortestDistance = float.MaxValue;
            Vector2 closestPoint = Vector2.Zero;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Vector2 lineStart = vertices[i];
                Vector2 lineEnd = vertices[i + 1];

                Vector2 lineDirection = lineEnd - lineStart;
                float lineLength = lineDirection.Length;
                lineDirection = Vector2.Normalize(lineDirection);

                Vector2 pointDirection = point - lineStart;
                float dotProduct = Vector2.Dot(pointDirection, lineDirection);

                Vector2 projectedPoint;
                if (dotProduct <= 0)
                {
                    projectedPoint = lineStart;
                }
                else if (dotProduct >= lineLength)
                {
                    projectedPoint = lineEnd;
                }
                else
                {
                    projectedPoint = lineStart + lineDirection * dotProduct;
                }

                float distance = Vector2.Distance(point, projectedPoint);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPoint = projectedPoint;
                }
            }

            return closestPoint;
        }

        private static List<Vector2> getPathVerticesToPathSpace(Path path)
        {
            var vertices = new List<Vector2>();

            foreach (Vector2 vertex in path.Vertices.ToList())
            {
                vertices.Add(vertex + path.Position);
            }

            return vertices;
        }
    }

    public partial class PlayerKeyBindingContainer : KeyBindingContainer<InputAction>
    {
        public PlayerKeyBindingContainer() : base(SimultaneousBindingMode.Unique)
        {
        }

        public override IEnumerable<KeyBinding> DefaultKeyBindings => new[]
        {
            new KeyBinding(new[] { InputKey.Left }, InputAction.Left),
            new KeyBinding(new[] { InputKey.Right }, InputAction.Right),
            new KeyBinding(new[] { InputKey.Space }, InputAction.Jump),
        };
    }

    public enum InputAction
    {
        Left,
        Right,
        Jump
    }
}
