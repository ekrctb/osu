// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Created for a <see cref="DrawableHitObject"/> when only <see cref="HitObject"/> is given
    /// to make sure a <see cref="DrawableHitObject"/> is always associated with a <see cref="HitObjectLifetimeEntry"/>.
    /// </summary>
    internal class SyntheticHitObjectEntry : HitObjectLifetimeEntry
    {
        public readonly DrawableHitObject? DrawableHitObject;

        protected sealed override double InitialLifetimeOffset => DrawableHitObject?.InitialLifetimeOffset ?? base.InitialLifetimeOffset;

        public SyntheticHitObjectEntry(DrawableHitObject drawableHitObject, HitObject hitObject)
            : base(hitObject)
        {
            DrawableHitObject = drawableHitObject;
            // Initial lifetime is set in base constructor before DrawableHitObject is set.
            // Recompute the initial lifetime with the DHO's InitialLifetimeOffset.
            SetInitialLifetime();
        }
    }
}
