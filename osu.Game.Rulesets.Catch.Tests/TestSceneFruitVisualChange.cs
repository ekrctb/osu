// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneFruitVisualChange : TestSceneFruitObjects
    {
        private readonly Bindable<int> indexInBeatmap = new Bindable<int>();

        protected override void LoadComplete()
        {
            AddStep("visual change via Bindable", () => SetContents(() => new TestDrawableCatchHitObjectSpecimen(new DrawableFruit(new Fruit
            {
                IndexInBeatmapBindable = { BindTarget = indexInBeatmap },
            }))));

            Scheduler.AddDelayed(() =>
            {
                indexInBeatmap.Value++;
            }, 250, true);
        }

        [Test]
        public void TestHitObjectChangesVisual()
        {
            AddStep("show grapes", () => SetContents(() => new TestDrawableCatchHitObjectSpecimen(new DrawableFruit(new Fruit { IndexInBeatmap = 1 }))));
            checkVisual(FruitVisualRepresentation.Grape);
            AddStep("apply pear", () => applyHitObject(new Fruit { IndexInBeatmap = 0 }));
            checkVisual(FruitVisualRepresentation.Pear);
        }

        private void applyHitObject(CatchHitObject hitObject)
        {
            TestDrawableCatchHitObjectSpecimen.PrepareHitObject(hitObject);

            foreach (var drawableFruit in this.ChildrenOfType<DrawableFruit>())
                drawableFruit.Apply(hitObject, new HitObjectLifetimeEntry(hitObject));
        }

        private void checkVisual(FruitVisualRepresentation visual) =>
            AddAssert($"visual is {visual}", () =>
            {
                return Content.ChildrenOfType<FruitPulpFormation>().Count(p => p.VisualRepresentation.Value == visual) == 1 &&
                       Content.ChildrenOfType<LegacyFruitPiece>().Count(p => p.VisualRepresentation.Value == visual) == 4;
            });
    }
}
