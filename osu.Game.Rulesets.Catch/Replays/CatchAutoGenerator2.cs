// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchAutoGenerator2 : AutoGenerator<CatchReplayFrame>
    {
        public CatchAutoGenerator2(Beatmap<CatchHitObject> beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            float halfCatcherWidth = Catcher.CalculateCatchWidth(Beatmap.BeatmapInfo.BaseDifficulty) / 2;
            Debug.Assert(halfCatcherWidth > 0, "Catcher width must be positive");

            const double dash_speed = Catcher.BASE_SPEED;

            var objects = new List<PalpableCatchHitObject>();

            foreach (var obj in Beatmap.HitObjects)
            {
                if (obj is Fruit fruit)
                    objects.Add(fruit);

                if (obj is BananaShower || obj is JuiceStream)
                {
                    foreach (var nested in obj.NestedHitObjects.OfType<PalpableCatchHitObject>())
                        objects.Add(nested);
                }
            }

            objects.Sort((h1, h2) => h1.StartTime.CompareTo(h2.StartTime) +
                                     (h1.StartTime.CompareTo(h2.StartTime) == 0 ? h1.HyperDash.CompareTo(h2.HyperDash) : 0));

            // Removing droplets/banana that are during an hyperdash from the list
            bool skipping = false;

            for (int i = 0; i < objects.Count; ++i)
            {
                if (skipping && (objects[i] is Banana || objects[i] is TinyDroplet))
                    objects.RemoveAt(i--);
                else
                    skipping = objects[i].HyperDash;
            }

            //Building the score
            List<CatchStepFunction> scores = new List<CatchStepFunction>();
            List<double> times = new List<double>();
            scores.Insert(0, new CatchStepFunction()); // After the last object, there is no more score to be made
            times.Insert(0, 1 + objects[^1].StartTime); //some time after the last object

            for (int i = objects.Count - 1; i >= 0; --i)
            {
                var obj = objects[i];
                int value = obj is Banana || obj is TinyDroplet ? 1 : obj is Fruit ? 300 : 20;

                if (obj.StartTime != times[0])
                {
                    // TODO: efficiency is neglected
                    scores.Insert(0, scores[0].SlidingWindowMax((float)(dash_speed * (times[0] - obj.StartTime))));
                    scores[0].Set(float.NegativeInfinity, 0, 0);
                    scores[0].Set(CatchPlayfield.WIDTH, float.PositiveInfinity, 0);
                    times.Insert(0, obj.StartTime);
                }

                if (obj.HyperDash)
                {
                    float distance = Math.Abs(obj.HyperDashTarget.EffectiveX - obj.EffectiveX);
                    int maxScore = scores[1].Max(obj.EffectiveX - distance, obj.EffectiveX + distance);
                    scores[0].Set(obj.EffectiveX - halfCatcherWidth, obj.EffectiveX + halfCatcherWidth, maxScore);
                }

                scores[0].Add(Math.Max(0, obj.EffectiveX - halfCatcherWidth), Math.Min(CatchPlayfield.WIDTH, obj.EffectiveX + halfCatcherWidth), value);
            }

            float lastPosition = CatchPlayfield.CENTER_X;
            double lastTime = -10000;

            void moveToNext(float target, double time)
            {
                const double movement_speed = dash_speed / 2;
                float positionChange = Math.Abs(lastPosition - target);
                double timeAvailable = time - lastTime;
                //So we can either make it there without a dash or not.
                double speedRequired = positionChange / timeAvailable;
                bool dashRequired = speedRequired > movement_speed && time != 0;

                if (dashRequired)
                {
                    //we do a movement in two parts - the dash part then the normal part...
                    double timeAtDashSpeed = (positionChange - movement_speed * timeAvailable) / (dash_speed - movement_speed);

                    if (timeAtDashSpeed <= timeAvailable)
                    {
                        float midPosition = lastPosition + Math.Sign(target - lastPosition) * (float)(timeAtDashSpeed * dash_speed);
                        //dash movement
                        addFrame(lastTime + timeAtDashSpeed, midPosition, true);
                        addFrame(time, target);
                    }
                    else
                        addFrame(time, target, true);
                }
                else
                {
                    double timeBefore = positionChange / movement_speed;

                    addFrame(lastTime + timeBefore, target);
                    addFrame(time, target);
                }

                lastTime = time;
                lastPosition = target;
            }

            moveToNext(CatchPlayfield.CENTER_X, -1000);
            float hyperDashDistance = 0;
            int j = 0;

            foreach (var obj in objects)
            {
                if (obj.StartTime != lastTime)
                {
                    float movementRange = hyperDashDistance == 0 ? (float)(dash_speed * (times[j] - lastTime)) : hyperDashDistance;
                    float lo = lastPosition - movementRange, up = lastPosition + movementRange;
                    int max = scores[j].Max(lo, up);

                    // If it is possible to get the maximum score by standing still, don't move.
                    // Otherwise, move to a position that allows maximum error while getting the maximum score.
                    var nextPosition = scores[j].DistanceToSmaller(lastPosition, max) > 0 ? lastPosition : scores[j].GetOptimalPoint(lo, up);

                    moveToNext(nextPosition, times[j++]);
                }

                hyperDashDistance = obj.HyperDash && lastPosition >= obj.EffectiveX - halfCatcherWidth && lastPosition <= obj.EffectiveX + halfCatcherWidth
                    ? Math.Abs(obj.HyperDashTarget.EffectiveX - lastPosition)
                    : 0;
            }
        }

        private void addFrame(double time, float? position = null, bool dashing = false)
        {
            Frames.Add(new CatchReplayFrame(time, position, dashing, LastFrame));
        }
    }
}
