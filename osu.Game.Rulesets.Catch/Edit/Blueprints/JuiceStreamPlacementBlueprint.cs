// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public class JuiceStreamPlacementBlueprint : CatchPlacementBlueprint<JuiceStream>
    {
        private readonly ScrollingPath scrollingPath;

        // Only shown during "waiting" state.
        private readonly FruitOutline startFruitOutline;

        private readonly NestedOutlineContainer nestedOutlineContainer;

        private bool isEmpty;

        public JuiceStreamPlacementBlueprint()
        {
            InternalChildren = new Drawable[]
            {
                scrollingPath = new ScrollingPath(),
                startFruitOutline = new FruitOutline(),
                nestedOutlineContainer = new NestedOutlineContainer(),
            };
        }

        protected override void Update()
        {
            base.Update();

            scrollingPath.UpdatePositionFrom(HitObjectContainer, HitObject);
            nestedOutlineContainer.UpdatePositionFrom(HitObjectContainer, HitObject);

            if (PlacementActive == PlacementState.Waiting)
            {
                startFruitOutline.UpdateFrom(HitObjectContainer, HitObject);
                return;
            }

            bool wasEmpty = isEmpty;
            isEmpty = HitObject.Duration == 0;
            if (wasEmpty == isEmpty) return;

            // When the duration is zero, "start" and "end" fruits are overlapped.
            // Use a half of desired alpha to compensate that.
            nestedOutlineContainer.FadeTo(isEmpty ? 0.25f : 1);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    if (e.Button != MouseButton.Left) break;

                    BeginPlacement(true);
                    HitObject.Path.ControlPoints.Add(new PathControlPoint());
                    addLinearSegment();

                    startFruitOutline.Hide();
                    return true;

                case PlacementState.Active:
                    switch (e.Button)
                    {
                        case MouseButton.Left:
                            addLinearSegment();
                            break;

                        case MouseButton.Right:
                            EndPlacement(HitObject.Duration > 0);
                            return true;
                    }

                    break;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            if (!(result.Time is double time)) return;

            float x = ToLocalSpace(result.ScreenSpacePosition).X;

            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    HitObject.X = x;
                    HitObject.StartTime = time;
                    break;

                case PlacementState.Active:
                    moveCurrentSegmentTowards(time, x);

                    ApplyDefaultsToHitObject();

                    scrollingPath.UpdatePathFrom(HitObjectContainer, HitObject);
                    nestedOutlineContainer.UpdateNestedObjectsFrom(HitObjectContainer, HitObject);
                    break;
            }
        }

        private void addLinearSegment()
        {
            var lastControlPoint = HitObject.Path.ControlPoints[^1];
            lastControlPoint.Type.Value = PathType.Linear;

            HitObject.Path.ControlPoints.Add(new PathControlPoint());
            ApplyDefaultsToHitObject();
        }

        private void moveCurrentSegmentTowards(double time, float x)
        {
            int startIndex = HitObject.Path.ControlPoints.Count - 2;
            Vector2 startPosition = HitObject.Path.ControlPoints[startIndex].Position.Value;
            double distance = (time - HitObject.StartTime) * HitObject.Velocity - HitObject.Path.DistanceAtControlPointIndex(startIndex);
            float xDiff = x - (HitObject.OriginalX + startPosition.X);
            Vector2 segment = fitLinearSegment(distance, xDiff);

            HitObject.Path.ControlPoints[^1].Position.Value = startPosition + segment;
        }

        private Vector2 fitLinearSegment(double distance, float xDiff)
        {
            // x^2 + y^2 = distance^2
            // recalculate `x` from `y` to make it prioritize `distance` over `deltaX`.
            double y = Math.Sqrt(Math.Max(0, distance * distance - (double)xDiff * xDiff));
            double x = Math.Sign(xDiff) * Math.Sqrt(Math.Max(0, distance * distance - y * y));
            return new Vector2((float)x, (float)y);
        }
    }
}
