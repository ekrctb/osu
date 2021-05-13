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
                var func = new CatchStepFunction(0, size);

                for (int testStep = 0; testStep < 10; testStep++)
                {
                    switch (rng.Next(3))
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
                    }
                }

                (int, int) nextInterval()
                {
                    int lo = rng.Next(0, size + 1);
                    int up = rng.Next(0, size + 1);
                    return lo <= up ? (lo, up) : (up, lo);
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

            /// <summary>
            /// Add <paramref name="val"/> to the half-closed interval [<paramref name="lo"/>, <paramref name="up"/>).
            /// </summary>
            public void Add(int lo, int up, int val)
            {
                Debug.Assert(0 <= lo && lo <= up && up <= values.Length);
                for (int i = lo; i < up; i++)
                    values[i] += val;
            }

            /// <summary>
            /// Assign <paramref name="val"/> to the half-closed interval [<paramref name="lo"/>, <paramref name="up"/>).
            /// </summary>
            public void Set(int lo, int up, int val)
            {
                Debug.Assert(0 <= lo && lo <= up && up <= values.Length);
                for (int i = lo; i < up; i++)
                    values[i] = val;
            }

            /// <summary>
            /// Compute the maximum value in the half-closed interval [<paramref name="lo"/>, <paramref name="up"/>).
            /// </summary>
            /// <returns>The maximum value, or <c>0</c> if the given interval was empty.</returns>
            public int Max(int lo, int up)
            {
                lo = Math.Max(0, lo);
                up = Math.Min(up, values.Length);
                if (lo >= up) return 0;

                int max = values[lo];
                for (int i = lo + 1; i < up; i++)
                    max = Math.Max(max, values[i]);
                return max;
            }

            public float MaxRobustness(int lo, int up)
            {
                Debug.Assert(0 <= lo && lo <= up && up <= values.Length);
                int maxValue = Max(lo, up);
                int maxRobustness = -1;

                for (int i = lo; i < up; i++)
                {
                    if (values[i] == maxValue)
                        maxRobustness = Math.Max(maxRobustness, GetRobustness(i));
                }

                Debug.Assert(maxRobustness >= 0);
                return maxRobustness;
            }

            public int GetRobustness(int x)
            {
                int lo = x, up = x;

                while (lo >= 0 && values[lo] >= values[x]) lo--;
                while (up < values.Length && values[up] >= values[x]) up++;
                return up - lo;
            }
        }
    }
}
