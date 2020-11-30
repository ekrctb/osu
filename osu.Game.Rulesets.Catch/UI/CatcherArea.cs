// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;
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

        public Container ExplodingFruitTarget
        {
            set => MovableCatcher.ExplodingFruitTarget = value;
        }

        private Drawable lastPlateableFruit;

        public CatcherArea(BeatmapDifficulty difficulty = null)
        {
            Size = new Vector2(CatchPlayfield.WIDTH, CATCHER_SIZE);
            Children = new Drawable[]
            {
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

            caughtObject.Anchor = Anchor.TopCentre;
            caughtObject.Origin = Anchor.Centre;
            caughtObject.RelativePositionAxes = Axes.None;
            caughtObject.Position = new Vector2(MovableCatcher.ToLocalSpace(fruit.ScreenSpaceDrawQuad.Centre).X - MovableCatcher.DrawSize.X / 2, 0);

            MovableCatcher.PlaceOnPlate(caughtObject, fruit);
            lastPlateableFruit = caughtObject;

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
        private Drawable getCaughtObject(DrawablePalpableCatchHitObject drawableObject)
        {
            return drawableObject.HitObject switch
            {
                Fruit _ => new FruitPiece
                {
                    VisualRepresentation = { BindTarget = ((DrawableFruit)drawableObject).VisualRepresentation },
                    HyperDash = { BindTarget = drawableObject.HyperDash },
                    AccentColour = { BindTarget = drawableObject.AccentColour },
                    RelativeSizeAxes = Axes.None,
                    // TODO TODO
                    Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2 * drawableObject.ScaleBindable.Value),
                },
                // TinyDroplet tinyDroplet => throw new NotImplementedException(),
                // Droplet droplet => throw new NotImplementedException(),
                _ => null,
            };
        }
    }
}
