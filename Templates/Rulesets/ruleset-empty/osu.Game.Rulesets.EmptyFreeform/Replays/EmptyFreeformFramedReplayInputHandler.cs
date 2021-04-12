// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.EmptyFreeform.Replays
{
    public class EmptyFreeformFramedReplayInputHandler : FramedReplayInputHandler<EmptyFreeformReplayFrame>
    {
        public EmptyFreeformFramedReplayInputHandler(Replay replay)
            : base(replay)
        {
        }

        protected override bool IsImportant(EmptyFreeformReplayFrame frame) => frame.Actions.Any();

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            var interpolatedPosition = Interpolation.ValueAt(CurrentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);

            inputs.Add(new MousePositionAbsoluteInput
            {
                Position = GamefieldToScreenSpace(interpolatedPosition),
            });
            inputs.Add(new ReplayState<EmptyFreeformAction>
            {
                PressedActions = CurrentFrame?.Actions ?? new List<EmptyFreeformAction>(),
            });
        }
    }
}
