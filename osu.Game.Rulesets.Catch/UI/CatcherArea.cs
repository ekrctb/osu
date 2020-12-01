// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        public const float CATCHER_SIZE = 106.75f;

        public Func<CatchHitObject, DrawableHitObject<CatchHitObject>> CreateDrawableRepresentation;

        public readonly Catcher MovableCatcher;
        private readonly CatchComboDisplay comboDisplay;

        internal ExplodingFruitContainer ExplodingFruitTarget
        {
            set => MovableCatcher.ExplodingFruitTarget = value;
        }

        private readonly DrawablePool<CaughtFruit> caughtFruitPool;
        private readonly DrawablePool<CaughtDroplet> caughtDropletPool;

        public CatcherArea(BeatmapDifficulty difficulty = null)
        {
            Size = new Vector2(CatchPlayfield.WIDTH, CATCHER_SIZE);
            Children = new Drawable[]
            {
                caughtFruitPool = new DrawablePool<CaughtFruit>(1),
                caughtDropletPool = new DrawablePool<CaughtDroplet>(1),
                comboDisplay = new CatchComboDisplay
                {
                    RelativeSizeAxes = Axes.None,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Bottom = 350f },
                    X = CatchPlayfield.CENTER_X
                },
                MovableCatcher = new Catcher(this, difficulty) { X = CatchPlayfield.CENTER_X },
            };
        }

        public void OnNewResult(DrawableCatchHitObject hitObject, JudgementResult result)
        {
            if (!result.Type.IsScorable())
                return;

            if (result.IsHit && hitObject is DrawablePalpableCatchHitObject fruit)
            {
                addCaughtObject(fruit);
            }

            if (hitObject.HitObject.LastInCombo)
            {
                if (result.Judgement is CatchJudgement catchJudgement && catchJudgement.ShouldExplodeFor(result))
                    MovableCatcher.Explode();
                else
                    MovableCatcher.Drop();
            }

            comboDisplay.OnNewResult(hitObject, result);
        }

        private void addCaughtObject(DrawablePalpableCatchHitObject fruit)
        {
            var caughtObject = getCaughtObject(fruit);
            if (caughtObject == null) return;

            MovableCatcher.PlaceOnPlate(caughtObject, fruit);

            if (!fruit.StaysOnPlate)
                MovableCatcher.Explode(caughtObject);
        }

        public void OnRevertResult(DrawableCatchHitObject fruit, JudgementResult result)
            => comboDisplay.OnRevertResult(fruit, result);

        public void OnReleased(CatchAction action)
        {
        }

        public bool AttemptCatch(CatchHitObject obj)
        {
            return MovableCatcher.AttemptCatch(obj);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var state = (GetContainingInputManager().CurrentState as RulesetInputManagerInputState<CatchAction>)?.LastReplayState as CatchFramedReplayInputHandler.CatchReplayState;

            if (state?.CatcherX != null)
                MovableCatcher.X = state.CatcherX.Value;

            comboDisplay.X = MovableCatcher.X;
        }

        [CanBeNull]
        private PoolableCaughtObject getCaughtObject(DrawablePalpableCatchHitObject drawableObject)
        {
            PoolableCaughtObject caughtObject;

            switch (drawableObject)
            {
                case DrawableFruit fruit:
                    CaughtFruit caughtFruit = caughtFruitPool.Get();

                    // Copying the value because the value will change when the DHO is reused for the next hit object.
                    caughtFruit.VisualRepresentation.Value = fruit.VisualRepresentation.Value;

                    caughtObject = caughtFruit;
                    break;

                case DrawableDroplet _:
                    caughtObject = caughtDropletPool.Get();
                    break;

                default:
                    return null;
            }

            caughtObject.AccentColour.Value = drawableObject.AccentColour.Value;
            caughtObject.Scale = drawableObject.ScaleContainer.Scale * 0.5f;
            caughtObject.Rotation = drawableObject.ScaleContainer.Rotation;

            return caughtObject;
        }
    }
}
