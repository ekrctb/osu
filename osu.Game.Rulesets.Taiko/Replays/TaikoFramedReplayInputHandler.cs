// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Replays;
using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Game.Replays;

namespace osu.Game.Rulesets.Taiko.Replays
{
    internal class TaikoFramedReplayInputHandler : FramedReplayInputHandler<TaikoReplayFrame>
    {
        public TaikoFramedReplayInputHandler(Replay replay)
            : base(replay, new TaikoReplayFrame())
        {
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            inputs.Add(new ReplayState<TaikoAction> { PressedActions = CurrentFrame.Actions });
        }
    }
}
