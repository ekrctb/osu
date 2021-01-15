﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Mania.Replays
{
    internal class ManiaFramedReplayInputHandler : FramedReplayInputHandler<ManiaReplayFrame>
    {
        public ManiaFramedReplayInputHandler(Replay replay)
            : base(replay, new ManiaReplayFrame())
        {
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            inputs.Add(new ReplayState<ManiaAction> { PressedActions = CurrentFrame.Actions });
        }
    }
}
