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

            /// <summary>
            /// Add <paramref name="val"/> to the interval <paramref name="lo"/>..<paramref name="up"/>.
            /// </summary>
            public void Add(int lo, int up, int val)
            {
                clipInterval(ref lo, ref up);
                for (int i = lo; i < up; i++)
                    values[i] += val;
            }

            /// <summary>
            /// Assign <paramref name="val"/> to the interval <paramref name="lo"/>..<paramref name="up"/>.
            /// </summary>
            public void Set(int lo, int up, int val)
            {
                clipInterval(ref lo, ref up);
                for (int i = lo; i < up; i++)
                    values[i] = val;
            }

            /// <summary>
            /// Compute the maximum value in the interval <paramref name="lo"/>..<paramref name="up"/>.
            /// </summary>
            public int Max(int lo, int up)
            {
                clipInterval(ref lo, ref up);
                int max = values[lo];
                for (int i = lo + 1; i < up; i++)
                    max = Math.Max(max, values[i]);

                return max;
            }

            private void clipInterval(ref int lo, ref int up)
            {
                lo = Math.Max(0, lo);
                up = Math.Min(up, values.Length);
                Debug.Assert(lo < up);
            }
        }
    }
}
