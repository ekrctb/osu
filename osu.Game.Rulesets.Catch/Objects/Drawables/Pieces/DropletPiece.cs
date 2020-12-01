// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class DropletPiece : CompositeDrawable
    {
        public readonly Bindable<bool> HyperDash = new Bindable<bool>();
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        public DropletPiece()
        {
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS / 2);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Pulp
            {
                RelativeSizeAxes = Axes.Both,
                AccentColour = { BindTarget = AccentColour }
            };

            if (HyperDash.Value)
            {
                AddInternal(new HyperDropletBorderPiece());
            }
        }
    }
}
