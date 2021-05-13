// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.MathUtils
{
    /// <summary>
    /// Represent a step function (piecewise constant function) mapping from the interval [0, <see cref="CatchStepFunction.WIDTH"/>] to an integer.
    /// </summary>
    /// <remarks>
    /// Values on the discontinuities are confused, and which of the adjacent interval is used is unspecified.
    /// </remarks>
    public class CatchStepFunction
    {
        /// <summary>
        /// The upper bound of the domain.
        /// </summary>
        public const float WIDTH = CatchPlayfield.WIDTH;

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

        ///<summary>
        /// Construct the constant zero function.
        ///</summary>
        public CatchStepFunction()
        {
            partition.Add(0);
            partition.Add(WIDTH);
            values.Add(0);
        }

        ///<summary>
        /// Construct the step function as the rolling window max function of the <paramref name="input"/> using the window size <paramref name="halfWindowWidth"/>.
        /// The rolling window max function is defined as: <c>g(x) = max { f(x+d) | d ∈ [-w,+w] }</c>.
        ///</summary>
        public CatchStepFunction(CatchStepFunction input, float halfWindowWidth)
        {
            Assert.GreaterOrEqual(halfWindowWidth, 0);

            // windowsLeft is the index of the first input partition that is strictly greater than the left of the window
            // windowsRight is the index of the first input partition that is strictly greater than the right of the window
            int windowLeft = 0, windowRight;
            Queue<int> window = new Queue<int>();

            // Extend the input function left and right, to simplify things
            input.partition.Add(WIDTH + halfWindowWidth);
            input.values.Add(0);
            input.partition.Insert(0, -halfWindowWidth);
            input.values.Insert(0, 0);

            // Initialising the window.
            for (windowRight = windowLeft; input.partition[windowRight] <= halfWindowWidth; ++windowRight)
                window.Enqueue(input.values[windowRight]);
            var windowMax = window.Max();
            ++windowLeft;

            // At each iteration we slide the windows one step to the right,
            // adding a new value and partition each time, until the end.
            partition.Add(0);

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

            partition.Add(WIDTH);

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
        public void Add(float from, float to, int value)
        {
            Assert.GreaterOrEqual(from, 0);
            Assert.GreaterOrEqual(to, from);
            Assert.GreaterOrEqual(WIDTH, to);

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
        public void Set(float from, float to, int value)
        {
            Assert.GreaterOrEqual(from, 0);
            Assert.GreaterOrEqual(to, from);
            Assert.GreaterOrEqual(WIDTH, to);

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
        /// Compute the maximum function value on the interval [<param name="from"></param>, <param name="to"></param>].
        ///</summary>
        public int Max(float from, float to)
        {
            int max = 0;

            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i] > max && partition[i] < to && partition[i + 1] > from)
                    max = values[i];
            }

            return max;
        }

        ///<summary>
        /// Returns a point in the interval [<paramref name="from"></paramref>, <paramref name="to"></paramref>]
        /// where the function value is maximum in the given interval [<paramref name="from"></paramref>, <paramref name="to"></paramref>].
        /// Returns <paramref name="target"></paramref> if it works.
        /// Otherwise, a point furthest away from the suboptimal points is returned.
        ///</summary>
        public float OptimalPath(float from, float to, float target)
        {
            Assert.GreaterOrEqual(to, target);
            Assert.GreaterOrEqual(target, from);

            int max = Max(from, to);
            float ret = -1, value = -1;

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
