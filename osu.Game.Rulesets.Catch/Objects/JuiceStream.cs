// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class JuiceStream : CatchHitObject, IHasCurve
    {
        /// <summary>
        /// Positional distance that results in a duration of one second, before any speed adjustments.
        /// </summary>
        private const float base_scoring_distance = 100;

        public int RepeatCount { get; set; }

        public double Velocity;
        public double TickDistance;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate;
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();
            createTicks();
        }

        private void createTicks()
        {
            if (TickDistance == 0)
                return;

            var tickSamples = Samples.Select(s => new SampleInfo
            {
                Bank = s.Bank,
                Name = @"slidertick",
                Volume = s.Volume
            }).ToList();

            double length = Path.Distance;
            int spanCount = this.SpanCount();

            float positionAt(double time)
            {
                double progress = Math.Max(0.0, time - StartTime) * Velocity / length % 2;
                if (progress <= 1)
                    return Path.PositionAt(progress).X;
                else
                    return Path.PositionAt(2 - progress).X;
            }

            void add(double time, CatchHitObject hitObject)
            {
                hitObject.StartTime = time;
                hitObject.X = X + positionAt(time) / CatchPlayfield.BASE_WIDTH;
                AddNested(hitObject);
            }

            void addFruit(double time) => add(time, new Fruit { Samples = Samples });
            void addDroplet(double time) => add(time, new Droplet { Samples = tickSamples });
            void addTinyDroplet(double time) => add(time, new TinyDroplet { Samples = tickSamples });

            double getTime(double progressLength) => Math.Floor(StartTime + progressLength / Velocity);

            var tickTimes = new List<double>();
            addFruit(StartTime);

            for (var span = 0; span < spanCount; ++span)
            {
                for (var tickAt = TickDistance; tickAt < length - 1; tickAt += TickDistance)
                {
                    var time = span % 2 == 0 ? getTime(span * length + tickAt) : getTime((span + 1) * length - tickAt);
                    tickTimes.Add(time);
                    addDroplet(time);
                }

                {
                    var time = getTime((span + 1) * length);
                    tickTimes.Add(time);
                    addFruit(time);
                }
            }

            tickTimes.Sort();
            if (LegacyLastTickOffset != null)
            {
                var lo = Math.Floor(StartTime + length / Velocity * spanCount / 2);
                tickTimes[tickTimes.Count - 1] = Math.Max(lo, tickTimes[tickTimes.Count - 1] - LegacyLastTickOffset.Value);
            }

            var lastTime = StartTime;
            foreach (var time in tickTimes)
            {
                var interval = time - lastTime;
                if (interval > 80)
                {
                    while (interval > 100) interval /= 2;
                    for (var t = lastTime + interval; t < time; t += interval)
                        addTinyDroplet(Math.Floor(t));
                }

                lastTime = time;
            }
        }

        public double EndTime => StartTime + this.SpanCount() * Path.Distance / Velocity;

        public float EndX => X + this.CurvePositionAt(1).X / CatchPlayfield.BASE_WIDTH;

        public double Duration => EndTime - StartTime;

        private SliderPath path;

        public SliderPath Path
        {
            get => path;
            set => path = value;
        }

        public double Distance => Path.Distance;

        public List<List<SampleInfo>> NodeSamples { get; set; } = new List<List<SampleInfo>>();

        public double? LegacyLastTickOffset { get; set; }
    }
}
