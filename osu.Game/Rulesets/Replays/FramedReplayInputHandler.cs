// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using osu.Game.Input.Handlers;
using osu.Game.Replays;

namespace osu.Game.Rulesets.Replays
{
    /// <summary>
    /// The ReplayHandler will take a replay and handle the propagation of updates to the input stack.
    /// It handles logic of any frames which *must* be executed.
    /// </summary>
    public abstract class FramedReplayInputHandler<TFrame> : ReplayInputHandler
        where TFrame : ReplayFrame
    {
        public override bool ShouldPause => !replay.HasReceivedAllFrames && !HasNextFrame;

        // TODO: it is assuming there are no multiple frames with the same time

        private readonly Replay replay;
        private readonly TFrame defaultFrame;

        private int currentFrameIndex;

        // is current frame played at the start time?
        private bool currentFrameStartPlayed;

        public double CurrentTime { get; private set; }

        public TFrame CurrentFrame => getFrame(currentFrameIndex);

        public TFrame NextFrame => getFrame(currentFrameIndex + 1);

        public bool HasFrames => replay.Frames.Count != 0;

        public bool HasNextFrame => currentFrameIndex + 1 < replay.Frames.Count;

        protected FramedReplayInputHandler(Replay replay, TFrame defaultFrame)
        {
            // TODO
            replay.Frames.Sort((a, b) => a.Time.CompareTo(b.Time));

            this.replay = replay;
            this.defaultFrame = defaultFrame;

            currentFrameIndex = -1;
            CurrentTime = defaultFrame.Time;
        }

        /// <inheritdoc cref="ReplayInputHandler"/>
        public sealed override double SetFrameFromTime(double proposedTime)
        {
            if (!HasFrames)
            {
                Debug.Assert(currentFrameIndex == -1);
                CurrentTime = proposedTime;
                currentFrameStartPlayed = false;
                return CurrentTime;
            }

            var previousFrameTime = getFrameTime(currentFrameIndex - 1);
            var currentFrameTime = getFrameTime(currentFrameIndex);
            var nextFrameTime = getFrameTime(currentFrameIndex + 1);

            if (nextFrameTime <= proposedTime)
            {
                CurrentTime = nextFrameTime;
                currentFrameIndex++;
            }
            else if (currentFrameTime > proposedTime)
            {
                if (currentFrameStartPlayed)
                {
                    CurrentTime = Math.Max(previousFrameTime, proposedTime);
                    currentFrameIndex--;
                }
                else
                {
                    CurrentTime = currentFrameTime;
                }
            }
            else
            {
                CurrentTime = proposedTime;
            }

            currentFrameStartPlayed = CurrentTime == getFrameTime(currentFrameIndex);

            if (currentFrameIndex < 0)
                Debug.Assert(CurrentTime < NextFrame.Time);
            else if (currentFrameIndex + 1 >= replay.Frames.Count)
                Debug.Assert(CurrentTime >= CurrentFrame.Time);
            else
                Debug.Assert(CurrentFrame.Time <= CurrentTime && CurrentTime <= NextFrame.Time);

            return CurrentTime;
        }

        private TFrame getFrame(int index)
        {
            if (replay.Frames.Count == 0)
                return defaultFrame;

            if (index < 0)
                return (TFrame)replay.Frames[0];

            if (index >= replay.Frames.Count)
                return (TFrame)replay.Frames[^1];

            return (TFrame)replay.Frames[index];
        }

        private double getFrameTime(int index)
        {
            if (index < 0 || replay.Frames.Count == 0)
                return double.NegativeInfinity;

            if (index >= replay.Frames.Count)
                return double.PositiveInfinity;

            return replay.Frames[index].Time;
        }
    }
}
