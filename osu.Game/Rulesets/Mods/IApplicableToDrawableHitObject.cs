// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for <see cref="Mod"/>s that can be applied to <see cref="DrawableHitObject"/>s.
    /// </summary>
    public interface IApplicableToDrawableHitObject : IApplicableMod
    {
        /// <summary>
        /// Applies this <see cref="IApplicableToDrawableHitObject"/> to a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// This is invoked for nested <see cref="DrawableHitObject"/>s as well.
        /// If the effect should only be applied to top-level <see cref="DrawableHitObject"/>s, check for <c>null</c> <see cref="DrawableHitObject.ParentHitObject"/>.
        /// </remarks>
        void ApplyToDrawableHitObject(DrawableHitObject drawable);
    }
}
