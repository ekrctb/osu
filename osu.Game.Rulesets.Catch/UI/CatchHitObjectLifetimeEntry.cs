// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchHitObjectLifetimeEntry : HitObjectLifetimeEntry
    {
        public CaughtObjectEntry CaughtObjectEntry;

        public CatchHitObjectLifetimeEntry(CatchHitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
