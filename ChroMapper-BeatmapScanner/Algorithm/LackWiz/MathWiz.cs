﻿using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace BeatmapScanner.Algorithm.LackWiz
{
    internal class MathWiz
    {
        public static List<Vector2> PointList2(List<Vector2> controlPoints, double interval)
        {
            int N = controlPoints.Count() - 1;
            if (N > 16)
            {
                controlPoints.RemoveRange(16, controlPoints.Count - 16);
            }

            List<Vector2> p = new List<Vector2>();

            for (double t = 0.0; t <= 1.0; t += interval)
            {
                Vector2 point = new Vector2();
                for (int i = 0; i < controlPoints.Count; ++i)
                {
                    float bn = Bernstein(N, i, t);
                    point.x += bn * controlPoints[i].x;
                    point.y += bn * controlPoints[i].y;
                }
                p.Add(point);
            }

            return p;
        }

        public static float Bernstein(int n, int i, double t)
        {
            double t_i = Math.Pow(t, i);
            double t_n_minus_i = Math.Pow((1 - t), (n - i));

            double basis = Binomial(n, i) * t_i * t_n_minus_i;
            return (float)basis;
        }

        public static double Binomial(int n, int i)
        {
            double ni;
            double a1 = Factorial[n];
            double a2 = Factorial[i];
            double a3 = Factorial[n - i];
            ni = a1 / (a2 * a3);
            return ni;
        }

        public static readonly double[] Factorial = new double[]
        {
                1.0d,
                1.0d,
                2.0d,
                6.0d,
                24.0d,
                120.0d,
                720.0d,
                5040.0d,
                40320.0d,
                362880.0d,
                3628800.0d,
                39916800.0d,
                479001600.0d,
                6227020800.0d,
                87178291200.0d,
                1307674368000.0d,
                20922789888000.0d,
        };

        public static double ConvertDegreesToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180f);
            return (radians);
        }

        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = radians * (180f / Math.PI);
            return (degrees);
        }

        public static ((double, double), (double, double)) CalculateBaseEntryExit((double x, double y) position, double angle)
        {
            (double, double) entry = (position.x * 0.333333 - Math.Cos(ConvertDegreesToRadians(angle)) * 0.166667 + 0.166667,
                position.y * 0.333333 - Math.Sin(ConvertDegreesToRadians(angle)) * 0.166667 + 0.166667);

            (double, double) exit = (position.x * 0.333333 + Math.Cos(ConvertDegreesToRadians(angle)) * 0.166667f + 0.166667,
                position.y * 0.333333 + Math.Sin(ConvertDegreesToRadians(angle)) * 0.166667 + 0.166667);

            return (entry, exit);
        }

        public static double SwingAngleStrainCalc(List<SwingData> swingData, bool leftOrRight)
        {
            var strainAmount = 0d;

            for (int i = 0; i < swingData.Count(); i++)
            {
                if (swingData[i].Forehand)
                {
                    if (leftOrRight)
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(247.5 - swingData[i].Angle) - 180)) / 180, 2);
                    }
                    else
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(292.5 - swingData[i].Angle) - 180)) / 180, 2);
                    }
                }
                else
                {
                    if (leftOrRight)
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(247.5 - 180 - swingData[i].Angle) - 180)) / 180, 2);
                    }
                    else
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(292.5 - 180 - swingData[i].Angle) - 180)) / 180, 2);
                    }
                }
            }

            return strainAmount;
        }

        public static double BezierAngleStrainCalc(List<double> angleData, bool forehand, bool leftOrRight)
        {
            var strainAmount = 0d;

            for (int i = 0; i < angleData.Count(); i++)
            {
                if (forehand)
                {
                    if (leftOrRight)
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(247.5 - angleData[i]) - 180)) / 180, 2);
                    }
                    else
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(292.5 - angleData[i]) - 180)) / 180, 2);
                    }
                }
                else
                {
                    if (leftOrRight)
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(247.5 - 180 - angleData[i]) - 180)) / 180, 2);
                    }
                    else
                    {
                        strainAmount += 2 * Math.Pow((180 - Math.Abs(Math.Abs(292.5 - 180 - angleData[i]) - 180)) / 180, 2);
                    }
                }
            }

            return strainAmount;
        }
    }
}
