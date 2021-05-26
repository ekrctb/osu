// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Rulesets.Catch.MathUtils
{
    /// <summary>
    /// Represent a step function (piecewise constant function with finitely many pieces).
    ///
    /// Each piece is an open interval.
    /// The disjoint union of all pieces partitions the real line excluding finitely many singularities between pieces.
    /// </summary>
    public class CatchStepFunction
    {
        ///<summary>
        /// The domain of the function is partitioned into pieces so the function is constant within each piece.
        /// Each piece <c>i</c> is the interval (<see cref="partition"/><c>[i]</c>, <see cref="partition"/><c>[i+1]</c>).
        ///</summary>
        private readonly List<float> partition = new List<float>();

        ///<summary>
        /// Values that the function takes.
        /// <see cref="values"/><c>[i]</c> is the value of the function on the interval (<see cref="partition"/><c>[i]</c>, <see cref="partition"/><c>[i+1]</c>).
        ///</summary>
        private readonly List<int> values = new List<int>();

        ///<summary>
        /// Construct the constant zero function.
        ///</summary>
        public CatchStepFunction()
        {
            partition.Add(float.NegativeInfinity);
            partition.Add(float.PositiveInfinity);
            values.Add(0);
        }

        private CatchStepFunction(List<float> partition, List<int> values)
        {
            Debug.Assert(partition.Count == values.Count + 1, "partition.Count == values.Count + 1");
            Debug.Assert(partition[0] == float.NegativeInfinity && partition[^1] == float.PositiveInfinity, "range is (-infinity, infinity)");

            this.partition = partition;
            this.values = values;
        }

        /// <summary>
        /// Create a new step function representing the sliding window maximum of this function.
        /// The sliding window max function is defined as: <c>g(x) = max { f(x+d) | d ∈ -w..+w }</c>.
        /// </summary>
        public CatchStepFunction SlidingWindowMax(float halfWindowSize)
        {
            if (!(0 < halfWindowSize && halfWindowSize < float.PositiveInfinity))
                throw new ArgumentOutOfRangeException(nameof(halfWindowSize), "The window size must be positive.");

            var resultPartition = new List<float>();
            var resultValues = new List<int>();
            var queue = new SlidingMaxQueue<float, int>();

            resultPartition.Add(float.NegativeInfinity);

            for (int i = 0;; i++)
            {
                while (!queue.IsEmpty && queue.Max.Key <= partition[i] - halfWindowSize)
                {
                    resultValues.Add(queue.Max.Value);
                    resultPartition.Add(queue.Max.Key);

                    queue.DequeueMax();
                }

                if (i == partition.Count - 1) break;

                if (!queue.IsEmpty && queue.Max.Value < values[i])
                {
                    resultValues.Add(queue.Max.Value);
                    resultPartition.Add(partition[i] - halfWindowSize);
                }

                queue.Enqueue(partition[i + 1] + halfWindowSize, values[i]);
            }

            return new CatchStepFunction(resultPartition, resultValues);
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

            // TODO: removing an empty interval can introduce adjacent intervals with the same value
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
        /// Modify the function to make <paramref name="value"/> is added to the value on the interval (<paramref name="from"/>, <paramref name="to"/>).
        /// Function values outside the interval are unchanged.
        ///</summary>
        public void Add(float from, float to, int value)
        {
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
        /// Modify the function to make the function takes <paramref name="value"/> constantly on the interval (<paramref name="from"/>, <paramref name="to"/>).
        /// Function values outside the interval are unchanged.
        /// </summary>
        public void Set(float from, float to, int value)
        {
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
        /// Compute the maximum function value on the interval (<param name="from"></param>, <param name="to"></param>).
        ///</summary>
        /// <returns>The maximum value in the given interval.</returns>
        /// <exception cref="ArgumentException">The given interval is empty.</exception>
        public int Max(float from, float to)
        {
            if (!(from < to))
                throw new ArgumentException($"The given interval ({from}, {to}) must be non-empty.");

            int? max = null;

            for (int i = 0; i < values.Count; ++i)
            {
                if ((max == null || values[i] > max) && partition[i] < to && partition[i + 1] > from)
                    max = values[i];
            }

            Debug.Assert(max != null);
            return max.Value;
        }

        /// <summary>
        /// Get the distance from <paramref name="x"/> to a point with a function value smaller than <paramref name="value"/>.
        /// </summary>
        public float DistanceToSmaller(float x, int value)
        {
            if (!float.IsFinite(x))
                throw new ArgumentOutOfRangeException(nameof(x), "The input point must be finite.");

            int i = partition.BinarySearch(x);
            if (i < 0) i = ~i - 1;

            int indexLo = i;
            while (indexLo >= 0 && values[indexLo] >= value) indexLo--;

            int indexUp = i;
            while (indexUp < values.Count && values[indexUp] >= value) indexUp++;

            return indexLo == indexUp ? 0 : Math.Min(x - partition[indexLo + 1], partition[indexUp] - x);
        }

        ///<summary>
        /// Returns a point in the interval (<paramref name="from"></paramref>, <paramref name="to"></paramref>) taking the maximum value.
        /// Among all such points, a point furthest away from the suboptimal points is returned (i.e. maximizing <see cref="DistanceToSmaller"/>).
        /// </summary>
        /// <returns>An optimal point furthest away from the suboptimal points.</returns>
        /// <exception cref="ArgumentException">The given interval is empty.</exception>
        public float GetOptimalPoint(float from, float to)
        {
            if (!(from < to))
                throw new ArgumentException($"The given interval ({from}, {to}) must be non-empty.");

            int max = Max(from, to);

            float ret = -1;
            float value = -1;

            float currentLo = partition[0];

            for (int i = 0; i < partition.Count - 1; ++i)
            {
                if (values[i] < max)
                    currentLo = partition[i + 1];

                float currentUp = partition[i + 1];

                if (currentLo < to && currentUp > from)
                {
                    float mid = float.IsFinite(currentLo) && float.IsFinite(currentUp) ? (currentLo + currentUp) / 2 :
                        float.IsFinite(currentLo) ? currentLo :
                        float.IsFinite(currentUp) ? currentUp : 0;
                    float x = mid <= from ? from : to <= mid ? to : mid;
                    float dist = Math.Min(x - currentLo, currentUp - x);

                    if (value < dist)
                    {
                        value = dist;
                        ret = x;
                    }
                }
            }

            Debug.Assert(value != -1);
            return ret;
        }
    }
}
