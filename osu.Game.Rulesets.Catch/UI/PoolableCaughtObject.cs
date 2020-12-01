// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Catch.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    public abstract class PoolableCaughtObject : PoolableDrawable
    {
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        protected PoolableCaughtObject()
        {
            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;
        }

        protected override void FreeAfterUse()
        {
            ClearTransforms();

            base.FreeAfterUse();
        }
    }
}
