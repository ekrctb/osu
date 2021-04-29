// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osu.Game.Rulesets.Objects.Pooling;

namespace osu.Game.Rulesets.Catch.UI
{
    public class HitExplosionContainer : DrawablePoolWithLifetime<HitExplosionEntry, HitExplosion>
    {
        public HitExplosionContainer()
            : base(10)
        {
        }

        protected override void OnEntryCrossedBoundary(HitExplosionEntry entry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
        {
            if (kind == LifetimeBoundaryKind.Start && direction == LifetimeBoundaryCrossingDirection.Backward)
                Remove(entry);
        }
    }
}
