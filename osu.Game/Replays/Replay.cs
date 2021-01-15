// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Replays
{
    public class Replay
    {
        /// <summary>
        /// Whether all frames for this replay have been received.
        /// If false, gameplay would be paused to wait for further data, for instance.
        /// </summary>
        public bool HasReceivedAllFrames = true;

        private readonly List<ReplayFrame> frames = new List<ReplayFrame>();

        // TODO: make it append-only
        public List<ReplayFrame> Frames
        {
            get => frames;
            set
            {
                if (frames.Count != 0)
                    throw new InvalidOperationException("May not set replay frames multiple times");

                frames.AddRange(value);
            }
        }

        public void AddFrame(ReplayFrame frame) => frames.Add(frame);
    }
}
