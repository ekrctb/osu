﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuFramedReplayInputHandler : FramedReplayInputHandler<OsuReplayFrame>
    {
        public OsuFramedReplayInputHandler(Replay replay)
            : base(replay, new OsuReplayFrame { Position = OsuPlayfield.BASE_SIZE / 2 })
        {
        }

        public override void CollectPendingInputs(List<IInput> inputs)
        {
            // There is an edge case of the frame positions are on a slider but an interpolated position is not.
            // TODO: autoplay is noticeably non-smooth.
            bool canInterpolate = CurrentFrame.Actions.Count == 0;
            var position = canInterpolate
                ? Interpolation.ValueAt(CurrentTime, CurrentFrame.Position, NextFrame.Position, CurrentFrame.Time, NextFrame.Time)
                : CurrentFrame.Position;
            inputs.Add(new MousePositionAbsoluteInput { Position = GamefieldToScreenSpace(position) });
            inputs.Add(new ReplayState<OsuAction> { PressedActions = CurrentFrame.Actions });
        }
    }
}
