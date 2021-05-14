// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using NUnit.Framework;
using osu.Game.Rulesets.Catch.MathUtils;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchStepFunctionTest
    {
        [Test]
        public void TestFuzz()
        {
            for (int testCase = 0; testCase < 100000; testCase++)
            {
                var rng = new Random(testCase);

                int size = rng.Next(3, 10);
                var naive = new DiscreteStepFunction(size);
                var func = new CatchStepFunction();

                for (int testStep = 0; testStep < 10; testStep++)
                {
                    switch (rng.Next(5))
                    {
                        case 0:
                        {
                            var (lo, up) = nextInterval();
                            int val = rng.Next(1, 5);
                            naive.Add(lo, up, val);
                            func.Add(lo, up, val);
                            break;
                        }

                        case 1:
                        {
                            var (lo, up) = nextInterval();
                            int val = rng.Next(1, 5);
                            naive.Set(lo, up, val);
                            func.Set(lo, up, val);
                            break;
                        }

                        case 2:
                        {
                            var (lo, up) = nextInterval();
                            int expected = naive.Max(lo, up);
                            int actual = func.Max(lo, up);
                            Assert.AreEqual(expected, actual);
                            break;
                        }

                        case 3:
                        {
                            float x = rng.Next(size * 2 + 1) / 2f;
                            int value = naive.Get(rng.Next(0, size));
                            float expected = naive.DistanceToSmaller(x, value);
                            float actual = func.DistanceToSmaller(x, value);
                            Assert.AreEqual(expected, actual);
                            break;
                        }

                        case 4:
                        {
                            var (lo, up) = nextInterval();
                            float expectedDistance = naive.GetOptimalDistance(lo, up);
                            float actualPoint = func.GetOptimalPoint(lo, up);
                            float actualDistance = naive.DistanceToSmaller(actualPoint, naive.Max(lo, up));
                            Assert.That(actualPoint, Is.InRange(lo, up));
                            Assert.AreEqual(expectedDistance, actualDistance);
                            break;
                        }
                    }
                }

                (int, int) nextInterval()
                {
                    int lo = rng.Next(0, size);
                    int up = rng.Next(0, size);
                    return lo <= up ? (lo, up + 1) : (up, lo + 1);
                }
            }
        }

        /// <summary>
        /// A step function of discrete domain represented by an array.
        /// Used as a canonical correct implementation and compared to the testing implementation.
        /// </summary>
        public class DiscreteStepFunction
        {
            private readonly int[] values;

            public DiscreteStepFunction(int size)
            {
                values = new int[size];
            }

            public int Get(int i) => i < 0 || i >= values.Length ? 0 : values[i];

            /// <summary>
            /// Add <paramref name="val"/> to the interval <paramref name="lo"/>..<paramref name="up"/>.
            /// </summary>
            public void Add(int lo, int up, int val)
            {
                if (lo >= up) return;

                for (int i = lo; i < up; i++)
                    values[i] += val;
            }

            /// <summary>
            /// Assign <paramref name="val"/> to the interval <paramref name="lo"/>..<paramref name="up"/>.
            /// </summary>
            public void Set(int lo, int up, int val)
            {
                if (lo >= up) return;

                for (int i = lo; i < up; i++)
                    values[i] = val;
            }

            /// <summary>
            /// Compute the maximum value in the interval <paramref name="lo"/>..<paramref name="up"/>.
            /// </summary>
            public int Max(int lo, int up)
            {
                Debug.Assert(lo < up);
                lo = Math.Max(0, lo);
                up = Math.Min(up, values.Length);

                int max = 0;
                for (int i = lo; i < up; i++)
                    max = Math.Max(max, values[i]);

                return max;
            }

            /// <summary>
            /// Get the distance from <paramref name="x"/> to a point with a value smaller than <paramref name="value"/>.
            /// </summary>
            public float DistanceToSmaller(float x, int value)
            {
                float distance = float.PositiveInfinity;

                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] < value)
                        distance = Math.Min(distance, i <= x && x < i + 1 ? 0 : Math.Min(Math.Abs(i - x), Math.Abs(x - (i + 1))));
                }

                if (value > 0)
                    distance = Math.Min(distance, Math.Min(Math.Abs(x), Math.Abs(values.Length - x)));

                return distance;
            }

            public float GetOptimalDistance(int lo, int up)
            {
                int max = Max(lo, up);
                float res = 0;

                for (int i = lo; i <= up; i++)
                    res = Math.Max(res, DistanceToSmaller(i, max));

                for (int i = lo; i < up; i++)
                    res = Math.Max(res, DistanceToSmaller(i + 0.5f, max));

                return res;
            }
        }
    }
}
