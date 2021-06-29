// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModTimeSlice<THitObject> : Mod, IApplicableToBeatmap, IApplicableToDrawableRuleset<THitObject>
        where THitObject : HitObject
    {
        public override string Name => @"Time Slice";
        public override string Acronym => @"TS";
        public override string Description => @"";
        public override double ScoreMultiplier => 1.0;

        public override ModType Type => ModType.Conversion;

        public override IconUsage? Icon => FontAwesome.Solid.Clock;

        public override bool RequiresConfiguration => true;

        [SettingSource("Start Time", "")]
        public BindableDouble StartTime { get; } = new BindableDouble
        {
            Precision = 1,
            MinValue = 0,
            MaxValue = 100,
            Value = 10,
        };

        [SettingSource("End Time", "")]
        public BindableDouble EndTime { get; } = new BindableDouble
        {
            Precision = 1,
            MinValue = 0,
            MaxValue = 100,
            Value = 30,
        };

        public void ApplyToBeatmap(IBeatmap b)
        {
            var beatmap = (Beatmap<THitObject>)b;
            if (beatmap.HitObjects.Count == 0)
                return;

            double timeOffset = beatmap.HitObjects[0].StartTime;
            double startTime = timeOffset + StartTime.Value * 1e3;
            double endTime = timeOffset + EndTime.Value * 1e3;
            beatmap.HitObjects.RemoveAll(hitObject => hitObject.StartTime < startTime || endTime <= hitObject.GetEndTime());
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<THitObject> drawableRuleset)
        {
            drawableRuleset.DisplayTimeOffset = StartTime.Value * 1e3;
        }
    }
}
