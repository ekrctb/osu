// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Catch.Replays
{
    public class CatchFramedReplayInputHandler : FramedReplayInputHandler<CatchReplayFrame>
    {
        public CatchFramedReplayInputHandler(Replay replay)
            : base(replay, new CatchReplayFrame { Position = CatchPlayfield.CENTER_X })
        {
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            float position = Interpolation.ValueAt(CurrentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time);

            inputs.Add(new CatchReplayState
            {
                PressedActions = CurrentFrame.Actions,
                CatcherX = position
            });
        }

        public class CatchReplayState : ReplayState<CatchAction>
        {
            public float CatcherX { get; set; }
        }
    }
}
