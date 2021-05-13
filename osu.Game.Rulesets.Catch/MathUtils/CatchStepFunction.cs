// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Rulesets.Catch.MathUtils
{
    /// <summary>
    /// Represent a step function (piecewise constant function).
    /// The domain is the given interval, and the output is an integer.
    /// </summary>
    public class CatchStepFunction
    {
        ///<summary>
        /// The domain of the function is partitioned into pieces so the function is constant within each piece.
        /// Each piece <c>i</c> is the interval [<see cref="partition"/><c>[i]</c>, <see cref="partition"/><c>[i+1]</c>].
        ///</summary>
        private readonly List<float> partition = new List<float>();

        ///<summary>
        /// Values that the function takes.
        /// <see cref="values"/><c>[i]</c> is the value of the function on the interval [<see cref="partition"/><c>[i]</c>, <see cref="partition"/><c>[i+1]</c>].
        ///</summary>
        private readonly List<int> values = new List<int>();

        public float DomainLo => partition[0];
        public float DomainUp => partition[^1];

        ///<summary>
        /// Construct the constant zero function on the specified domain [<paramref name="lower"/>, <paramref name="upper"/>].
        ///</summary>
        public CatchStepFunction(float lower, float upper)
        {
            if (!(lower < upper))
                throw new ArgumentException($"The domain of a {nameof(CatchStepFunction)} must be non-empty.");

            partition.Add(lower);
            partition.Add(upper);
            values.Add(0);
        }

        ///<summary>
        /// Construct the step function as the rolling window max function of the <paramref name="input"/> using the window size <paramref name="halfWindowWidth"/>.
        /// The rolling window max function is defined as: <c>g(x) = max { f(x+d) | d ∈ [-w,+w] }</c>.
        /// The domain of the resulting function is same as the domain of <paramref name="input"/>.
        ///</summary>
        public CatchStepFunction(CatchStepFunction input, float halfWindowWidth)
        {
            Trace.Assert(halfWindowWidth > 0);

            // windowsLeft is the index of the first input partition that is strictly greater than the left of the window
            // windowsRight is the index of the first input partition that is strictly greater than the right of the window
            int windowLeft = 0, windowRight;
            Queue<int> window = new Queue<int>();

            float domainLo = input.DomainLo;
            float domainUp = input.DomainUp;

            // Extend the input function left and right, to simplify things
            input.partition.Add(domainUp + halfWindowWidth);
            input.values.Add(0);
            input.partition.Insert(0, domainLo - halfWindowWidth);
            input.values.Insert(0, 0);

            // Initialising the window.
            for (windowRight = windowLeft; input.partition[windowRight] <= halfWindowWidth; ++windowRight)
                window.Enqueue(input.values[windowRight]);
            var windowMax = window.Max();
            ++windowLeft;

            // At each iteration we slide the windows one step to the right,
            // adding a new value and partition each time, until the end.
            partition.Add(domainLo);

            while (true)
            {
                values.Add(windowMax);
                // This distance is used to know if it is the left side or the right side of the windows
                // that will meet with the next partition first. (or both at the same time if the distance is 0)
                float distance = input.partition[windowRight] - input.partition[windowLeft] - 2 * halfWindowWidth;

                if (distance <= 0)
                {
                    //if we reach the end, stop. The last partition (1) is added after the loop.
                    if (windowRight == input.partition.Count - 1)
                        break;

                    windowMax = Math.Max(windowMax, input.values[windowRight]);
                    window.Enqueue(input.values[windowRight]);
                    partition.Add(input.partition[windowRight] - halfWindowWidth);
                    ++windowRight;
                }

                if (distance >= 0)
                {
                    if (window.Dequeue() == windowMax)
                        windowMax = window.Max();
                    partition.Add(input.partition[windowLeft] + halfWindowWidth);
                    ++windowLeft;
                }

                //if we added the same partition twice (moving two steps in a iteration)
                if (distance == 0)
                    partition.RemoveAt(partition.Count - 1);
            }

            partition.Add(domainUp);

            // Revert the extension
            input.partition.RemoveAt(0);
            input.partition.RemoveAt(input.partition.Count - 1);
            input.values.RemoveAt(0);
            input.values.RemoveAt(input.values.Count - 1);

            normalize();
        }

        ///<summary>
        /// Minimize the partition representation of the function.
        /// Adjacent intervals with the same value are melded, and empty intervals are removed.
        ///</summary>
        private void normalize()
        {
            for (int i = values.Count - 1; i > 1; --i)
            {
                if (values[i] == values[i - 1])
                {
                    values.RemoveAt(i);
                    partition.RemoveAt(i);
                }
            }

            for (int i = partition.Count - 1; i > 1; --i)
            {
                if (partition[i] == partition[i - 1])
                {
                    values.RemoveAt(i - 1);
                    partition.RemoveAt(i);
                }
            }
        }

        ///<summary>
        /// Modify the function to make <paramref name="value"/> is added to the value on the interval [<paramref name="from"/>, <paramref name="to"/>].
        /// Function values outside the interval are unchanged.
        ///</summary>
        /// <exception cref="ArgumentException">The given interval is not contained in the domain.</exception>
        public void Add(float from, float to, int value)
        {
            if (!(DomainLo <= from && to <= DomainUp))
                throw new ArgumentException($"The given interval [{from}, {to}] is not contained in the domain [{DomainLo}, {DomainUp}].");

            if (!(from < to)) return;

            int indexStart, indexEnd;

            for (indexStart = 0; partition[indexStart] <= from; ++indexStart)
            {
            }

            partition.Insert(indexStart, from);
            values.Insert(indexStart, values[indexStart - 1]);

            for (indexEnd = indexStart; partition[indexEnd] < to; ++indexEnd)
            {
            }

            partition.Insert(indexEnd, to);
            values.Insert(indexEnd - 1, values[indexEnd - 1]);
            for (int i = indexStart; i < indexEnd; ++i)
                values[i] += value;
            normalize();
        }

        /// <summary>
        /// Modify the function to make the function takes <paramref name="value"/> constantly on the interval [<paramref name="from"/>, <paramref name="to"/>].
        /// Function values outside the interval are unchanged.
        /// </summary>
        /// <exception cref="ArgumentException">The given interval is not contained in the domain.</exception>
        public void Set(float from, float to, int value)
        {
            if (!(DomainLo <= from && to <= DomainUp))
                throw new ArgumentException($"The given interval [{from}, {to}] is not contained in the domain [{DomainLo}, {DomainUp}].");

            if (!(from < to)) return;

            int indexStart, indexEnd;

            for (indexStart = 0; partition[indexStart] <= from; ++indexStart)
            {
            }

            partition.Insert(indexStart, from);
            values.Insert(indexStart, values[indexStart - 1]);

            for (indexEnd = indexStart; partition[indexEnd] < to; ++indexEnd)
            {
            }

            partition.Insert(indexEnd, to);
            values.Insert(indexEnd - 1, values[indexEnd - 1]);
            for (int i = indexStart; i < indexEnd; ++i)
                values[i] = value;
            normalize();
        }

        ///<summary>
        /// Compute the maximum function value on the *open* interval (<param name="from"></param>, <param name="to"></param>).
        ///</summary>
        public int Max(float from, float to)
        {
            if (!(from < to)) return 0;

            int max = 0;

            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] > max && partition[i] < to && partition[i + 1] > from)
                    max = values[i];
            }

            return max;
        }

        // TODO: endpoints (open vs closed)
        ///<summary>
        /// Returns a point in the interval [<paramref name="from"></paramref>, <paramref name="to"></paramref>]
        /// where the function value is maximum in the given interval [<paramref name="from"></paramref>, <paramref name="to"></paramref>].
        /// Returns <paramref name="target"></paramref> if it works.
        /// Otherwise, a point furthest away from the suboptimal points is returned.
        ///</summary>
        public float OptimalPath(float from, float to, float target)
        {
            if (!(from <= target && target <= to))
                throw new ArgumentOutOfRangeException(nameof(target), $"The target {target} is not in the given interval [{from}, {to}].");

            int max = Max(from, to);
            float ret = target, value = -1;

            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] == max && partition[i] <= to && partition[i + 1] >= from)
                {
                    if (target >= partition[i] && target <= partition[i + 1])
                        return target;

                    // TODO: this is wrong when the mid point is not in the given interval
                    // TODO: also, what if adjacent interval outside the given interval has a larger value?
                    float newValue = partition[i + 1] - partition[i];

                    if (newValue > value)
                    {
                        value = newValue;
                        ret = Math.Clamp((partition[i + 1] + partition[i]) / 2, from, to);
                    }
                }
            }

            return ret;
        }
    }
}
