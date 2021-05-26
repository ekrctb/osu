// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Catch.MathUtils
{
    /// <summary>
    /// Implements a linear-time algorithm of sliding window maximum.
    /// </summary>
    public class SlidingMaxQueue<TKey, TValue>
        where TValue : IComparable<TValue>
    {
        // See e.g. https://stackoverflow.com/a/31061015.
        // The double ended queue is implemented by a list and an index to the start of the contents.
        // The invariant is that `list[startIndex..]` have a monotonically decreasing `Value`s.
        // In particular, `list[startIndex]` is the current window max.
        private readonly List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();

        private int startIndex;

        public bool IsEmpty => startIndex == list.Count;

        public KeyValuePair<TKey, TValue> Max
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException($"Cannot get maximum value of an empty {nameof(SlidingMaxQueue<TKey, TValue>)}");

                return list[startIndex];
            }
        }

        public void Enqueue(TKey key, TValue value)
        {
            while (startIndex < list.Count && list[^1].Value.CompareTo(value) <= 0)
                list.RemoveAt(list.Count - 1);

            list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void DequeueMax()
        {
            if (IsEmpty)
                throw new InvalidOperationException($"Cannot dequeue an empty {nameof(SlidingMaxQueue<TKey, TValue>)}");

            startIndex++;
        }
    }
}
