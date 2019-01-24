﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class BananaShower : CatchHitObject, IHasEndTime
    {
        public override FruitVisualRepresentation VisualRepresentation => FruitVisualRepresentation.Banana;

        public override bool LastInCombo => true;

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();
            createBananas();
        }

        private void createBananas()
        {
            double spacing = Duration;
            while (spacing > 100)
                spacing /= 2;

            if (spacing <= 0)
                return;

            for (double time = StartTime; time <= EndTime; time += spacing)
                AddNested(new Banana
                {
                    Samples = Samples,
                    StartTime = Math.Floor(time)
                });
        }

        public double EndTime => StartTime + Duration;

        public double Duration { get; set; }
    }
}
