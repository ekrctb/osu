// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.EventArgs;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Graphics.Containers
{
    public class OsuScrollContainer : ScrollContainer
    {
        /// <summary>
        /// Allows controlling the scroll bar from any position in the container using the right mouse button.
        /// Uses the value of <see cref="DistanceDecayOnRightMouseScrollbar"/> to smoothly scroll to the dragged location.
        /// </summary>
        public bool RightMouseScrollbar = false;

        /// <summary>
        /// Controls the rate with which the target position is approached when performing a relative drag. Default is 0.02.
        /// </summary>
        public double DistanceDecayOnRightMouseScrollbar = 0.02;

        private bool shouldPerformRightMouseScroll(InputState state) => RightMouseScrollbar && state.Mouse.IsPressed(MouseButton.Right);

        private void scrollToRelative(float value) => ScrollTo(Clamp((value - Scrollbar.DrawSize[ScrollDim] / 2) / Scrollbar.Size[ScrollDim]), true, DistanceDecayOnRightMouseScrollbar);

        private bool mouseScrollBarDragging;

        protected override bool IsDragging => base.IsDragging || mouseScrollBarDragging;

        protected override bool OnMouseDown(MouseDownEventArgs args)
        {
            if (shouldPerformRightMouseScroll(args.State))
            {
                scrollToRelative(args.MousePosition[ScrollDim]);
                return true;
            }

            return base.OnMouseDown(args);
        }

        protected override bool OnDrag(DragEventArgs args)
        {
            if (mouseScrollBarDragging)
            {
                scrollToRelative(args.MousePosition[ScrollDim]);
                return true;
            }

            return base.OnDrag(args);
        }

        protected override bool OnDragStart(DragStartEventArgs args)
        {
            if (shouldPerformRightMouseScroll(args.State))
            {
                mouseScrollBarDragging = true;
                return true;
            }

            return base.OnDragStart(args);
        }

        protected override bool OnDragEnd(DragEndEventArgs args)
        {
            if (mouseScrollBarDragging)
            {
                mouseScrollBarDragging = false;
                return true;
            }

            return base.OnDragEnd(args);
        }
    }
}
