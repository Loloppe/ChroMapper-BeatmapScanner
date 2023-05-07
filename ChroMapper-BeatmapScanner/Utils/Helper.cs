using BeatmapScanner.Algorithm.Loloppe;
using System.Collections.Generic;
using System.Linq;
using System;
using BeatmapScanner.Algorithm.LackWiz;
using Beatmap.Base;
using UnityEngine;

namespace BeatmapScanner.Algorithm
{
    internal class Helper
    {
        #region Array

        public static double[] DirectionToDegree = { 90, 270, 180, 0, 135, 45, 225, 315, 270 };

        #endregion

        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            (list[indexB], list[indexA]) = (list[indexA], list[indexB]);
        }

        public static (double x, double y) SimulateSwingPos(double x, double y, double direction)
        {
            return (x + 5 * Math.Cos(ConvertDegreesToRadians(direction)), y + 5 * Math.Sin(ConvertDegreesToRadians(direction)));
        }

        public static void HandlePattern(List<Cube> cubes)
        {
            var length = 0;
            for (int n = 0; n < cubes.Count - 2; n++)
            {
                if(length > 0)
                {
                    length--;
                    continue;
                }
                if (cubes[n].Time == cubes[n + 1].Time)
                {
                    length = cubes.Where(c => c.Time == cubes[n].Time).Count() - 1;
                    var arrow = cubes.Where(c => c.CutDirection != 8 && c.Time == cubes[n].Time);
                    double direction = 0;
                    if(arrow.Count() == 0)
                    {
                        var foundArrow = cubes.Where(c => c.CutDirection != 8 && c.Time > cubes[n].Time).ToList();
                        if(foundArrow.Count() > 0)
                        {
                            direction = ReverseCutDirection(Mod(DirectionToDegree[foundArrow[0].CutDirection] + foundArrow[0].AngleOffset, 360));
                            for (int i = cubes.IndexOf(foundArrow[0]) - 1; i > n; i--)
                            {
                                if (cubes[i + 1].Time - cubes[i].Time >= 0.25)
                                {
                                    direction = ReverseCutDirection(direction);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        direction = ReverseCutDirection(Mod(DirectionToDegree[arrow.Last().CutDirection] + arrow.Last().AngleOffset, 360));
                    }
                    (double x, double y) pos;
                    if(n > 0)
                    {
                        pos = SimulateSwingPos(cubes[n - 1].Line, cubes[n - 1].Layer, direction);
                    }
                    else
                    {
                        pos = SimulateSwingPos(cubes[0].Line, cubes[0].Layer, direction);
                    }
                    List<double> distance = new List<double>();
                    for (int i = n; i < n + length + 1; i++)
                    {
                        distance.Add(Math.Sqrt(Math.Pow(pos.y - cubes[i].Layer, 2) + Math.Pow(pos.x - cubes[i].Line, 2)));
                    }
                    for (int i = 0; i < distance.Count; i++)
                    {
                        for (int j = n; j < n + length; j++)
                        {
                            if (distance[j - n + 1] < distance[j - n])
                            {
                                Swap(cubes, j, j + 1);
                                Swap(distance, j - n + 1, j - n);
                            }
                        }
                    }
                }
            }
        }

        public static string DegreeToName(double direction)
        {
            switch (direction)
            {
                case double d when (d > 67.5 && d <= 112.5):
                    return "UP";
                case double d when (d > 247.5 && d <= 292.5):
                    return "DOWN";
                case double d when (d > 157.5 && d <= 202.5):
                    return "LEFT";
                case double d when ((d <= 22.5 && d >= 0) || (d > 337.5 && d < 360)):
                    return "RIGHT";
                case double d when (d > 112.5 && d <= 157.5):
                    return "UP-LEFT";
                case double d when (d > 22.5 && d <= 67.5):
                    return "UP-RIGHT";
                case double d when (d > 202.5 && d <= 247.5):
                    return "DOWN-LEFT";
                case double d when (d > 292.5 && d <= 337.5):
                    return "DOWN-RIGHT";
            }

            return "ERROR";
        }

        public static double Mod(double x, double m)
        {
            return (x % m + m) % m;
        }

        public static double FindAngleViaPosition(List<Cube> cubes, int index, int h, double guideAngle, bool pattern)
        {
            (double x, double y) previousPosition = SimulateSwingPos(cubes[h].Line, cubes[h].Layer, guideAngle);
            (double x, double y) currentPosition = (cubes[index].Line, cubes[index].Layer);
            
            if(pattern)
            {
                previousPosition = (cubes[h].Line, cubes[h].Layer);
            }

            var currentAngle = ReverseCutDirection(Mod(MathWiz.ConvertRadiansToDegrees(Math.Atan2(previousPosition.y - currentPosition.y, previousPosition.x - currentPosition.x)), 360));

            currentAngle = Math.Round(currentAngle / 45) * 45;

            if(pattern && !IsSameDirection(guideAngle, currentAngle))
            {
                currentAngle = ReverseCutDirection(currentAngle);
            }
            else if(!pattern && IsSameDirection(guideAngle, currentAngle))
            {
                currentAngle = ReverseCutDirection(currentAngle);
            }

            return currentAngle;
        }

        public static void FixPatternHead(List<Cube> cubes)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int i = 1; i < cubes.Count() - 1; i++)
                {
                    if (cubes[i].Time - cubes[i - 1].Time >= -0.01 && cubes[i].Time - cubes[i - 1].Time <= 0.01)
                    {
                        switch (cubes[i].Direction)
                        {
                            case double d when (d > 67.5 && d <= 112.5):
                                if (cubes[i - 1].Layer > cubes[i].Layer)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when (d > 247.5 && d <= 292.5):
                                if (cubes[i - 1].Layer < cubes[i].Layer)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when (d > 157.5 && d <= 202.5):
                                if (cubes[i - 1].Line < cubes[i].Line)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when ((d <= 22.5 && d >= 0) || (d > 337.5 && d < 360)):
                                if (cubes[i - 1].Line > cubes[i].Line)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when (d > 112.5 && d <= 157.5): 
                                if (cubes[i - 1].Line < cubes[i].Line)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                else if (cubes[i - 1].Layer > cubes[i].Layer)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when (d > 22.5 && d <= 67.5):
                                if (cubes[i - 1].Line > cubes[i].Line)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                else if (cubes[i - 1].Layer > cubes[i].Layer)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when (d > 202.5 && d <= 247.5):
                                if (cubes[i - 1].Line < cubes[i].Line)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                else if (cubes[i - 1].Layer < cubes[i].Layer)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                            case double d when (d > 292.5 && d <= 337.5):
                                if (cubes[i - 1].Line > cubes[i].Line)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                else if (cubes[i - 1].Layer < cubes[i].Layer)
                                {
                                    Swap(cubes, i - 1, i);
                                }
                                break;
                        }
                    }
                }
            }
        }

        public static bool IsSameDirection(double before, double after)
        {
            Mod(before, 360);
            Mod(after, 360);

            if (Math.Abs(before - after) <= 180)
            {
                if (Math.Abs(before - after) < 67.5)
                {
                    return true;
                }
            }
            else if (Math.Abs(before - after) > 180)
            {
                if (360 - Math.Abs(before - after) < 67.5)
                {
                    return true;
                }
            }

            return false;
        }

        public static double ReverseCutDirection(double direction)
        {
            if (direction >= 180)
            {
                return direction - 180;
            }
            else
            {
                return direction + 180;
            }
        }

        public static bool IsSlider(Cube prev, Cube next, double direction, bool dot)
        {
            if(dot && prev.Line == next.Line && prev.Layer == next.Layer)
            {
                return true;
            }

            switch (direction)
            {
                case double d when (d > 67.5 && d <= 112.5):
                    if(prev.Layer < next.Layer)
                    {
                        return true;
                    }
                    break;
                case double d when (d > 247.5 && d <= 292.5):
                    if (prev.Layer > next.Layer)
                    {
                        return true;
                    }
                    break;
                case double d when (d > 157.5 && d <= 202.5):
                    if (prev.Line > next.Line)
                    {
                        return true;
                    }
                    break;
                case double d when ((d <= 22.5 && d >= 0) || (d > 337.5 && d < 360)):
                    if (prev.Line < next.Line)
                    {
                        return true;
                    }
                    break;
                case double d when (d > 112.5 && d <= 157.5):
                    if (prev.Layer < next.Layer)
                    {
                        return true;
                    }
                    if (prev.Line > next.Line)
                    {
                        return true;
                    }
                    break;
                case double d when (d > 22.5 && d <= 67.5):
                    if (prev.Layer < next.Layer)
                    {
                        return true;
                    }
                    if (prev.Line < next.Line)
                    {
                        return true;
                    }
                    break;
                case double d when (d > 202.5 && d <= 247.5):
                    if (prev.Layer > next.Layer)
                    {
                        return true;
                    }
                    if (prev.Line > next.Line)
                    {
                        return true;
                    }
                    break;
                case double d when (d > 292.5 && d <= 337.5):
                    if (prev.Layer > next.Layer)
                    {
                        return true;
                    }
                    if (prev.Line < next.Line)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        public static void FindNoteDirection(List<Cube> cubes, List<BaseNote> bombs)
        {
            if (cubes[0].CutDirection == 8)
            {
                var c = cubes.Where(ca => ca.CutDirection != 8).FirstOrDefault();
                if (c != null)
                {
                    cubes[0].Direction = DirectionToDegree[c.CutDirection] + c.AngleOffset;
                    for (int i = cubes.IndexOf(c); i > 1; i--)
                    {
                        if (cubes[i].Time - cubes[i - 1].Time >= 0.25)
                        {
                            cubes[0].Direction = Helper.ReverseCutDirection(cubes[0].Direction);
                        }
                    }
                }
                else
                {
                    if (cubes[0].Layer == 2)
                    {
                        cubes[0].Direction = 90;
                    }
                    else
                    {
                        cubes[0].Direction = 270;
                    }
                }
            }
            else
            {
                cubes[0].Direction = DirectionToDegree[cubes[0].CutDirection] + cubes[0].AngleOffset;
            }

            bool pattern = false;

            FixPatternHead(cubes);

            BaseNote bo = null;

            for (int i = 1; i < cubes.Count(); i++)
            {
                if (cubes[i].Time - cubes[i - 1].Time < 0.25 && (cubes[i].CutDirection == cubes[i - 1].CutDirection ||
                    cubes[i].Assumed || cubes[i - 1].Assumed || IsSameDirection(cubes[i - 1].Direction, DirectionToDegree[cubes[i].CutDirection] + cubes[i].AngleOffset)))
                {
                    if (!pattern)
                    {
                        cubes[i - 1].Head = true;
                        if (cubes[i].Time - cubes[i - 1].Time < 0.26 && cubes[i].Time - cubes[i - 1].Time >= 0.01)
                        {
                            cubes[i - 1].Slider = true;
                        }
                    }

                    cubes[i - 1].Pattern = true;
                    cubes[i].Pattern = true;
                    pattern = true;
                }
                else
                {
                    pattern = false;
                }

                bo = bombs.LastOrDefault(b => cubes[i - 1].Time < b.JsonTime && cubes[i].Time >= b.JsonTime && cubes[i].Line == b.PosX);

                if (bo != null)
                {
                    cubes[i].Bomb = true;
                }

                if (cubes[i].Pattern && cubes[i - 1].Bomb)
                {
                    cubes[i].Bomb = cubes[i - 1].Bomb;
                }

                if (cubes[i].Assumed && !cubes[i].Pattern && !cubes[i].Bomb)
                {
                    cubes[i].Direction = ReverseCutDirection(cubes[i - 1].Direction);
                }
                else if (cubes[i].Assumed && cubes[i].Pattern)
                {
                    cubes[i].Direction = cubes[i - 1].Direction;
                }
                else if (cubes[i].Assumed && cubes[i].Bomb)
                {
                    if (bo.PosY == 0)
                    {
                        cubes[i].Direction = 270;
                    }
                    else if (bo.PosY == 1)
                    {
                        if (cubes[i].Layer == 0)
                        {
                            cubes[i].Direction = 90;
                        }
                        else
                        {
                            cubes[i].Direction = 270;
                        }
                    }
                    else if (bo.PosY == 2)
                    {
                        cubes[i].Direction = 90;
                    }
                }
                else
                {
                    cubes[i].Direction = DirectionToDegree[cubes[i].CutDirection] + cubes[i].AngleOffset;
                }
            }
        }

        public static void FindReset(List<Cube> cubes)
        {
            for (int i = 1; i < cubes.Count(); i++)
            {
                if (cubes[i].Pattern && !cubes[i].Head)
                {
                    continue;
                }

                if (IsSameDirection(cubes[i - 1].Direction, cubes[i].Direction))
                {
                    cubes[i].Reset = true;
                    continue;
                }
            }
        }

        public static double ConvertDegreesToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180f);
            return (radians);
        }

        public static ((double x, double y) entry, (double x, double y) exit) CalculateBaseEntryExit((double x, double y) position, double angle)
        {
            (double, double) entry = (position.x * 0.333333 - Math.Cos(ConvertDegreesToRadians(angle)) * 0.166667 + 0.166667,
                position.y * 0.333333 - Math.Sin(ConvertDegreesToRadians(angle)) * 0.166667 + 0.166667);

            (double, double) exit = (position.x * 0.333333 + Math.Cos(ConvertDegreesToRadians(angle)) * 0.166667f + 0.166667,
                position.y * 0.333333 + Math.Sin(ConvertDegreesToRadians(angle)) * 0.166667 + 0.166667);

            return (entry, exit);
        }

        public static bool IsInLinearPath(Cube previous, Cube current, Cube next)
        {
            var prev = CalculateBaseEntryExit((previous.Line, previous.Layer), previous.Direction);
            var curr = CalculateBaseEntryExit((current.Line, current.Layer), current.Direction);
            var nxt = CalculateBaseEntryExit((next.Line, next.Layer), next.Direction);

            var dxc = nxt.entry.x - prev.entry.x;
            var dyc = nxt.entry.y - prev.entry.y;

            var dxl = curr.exit.x - prev.entry.x;
            var dyl = curr.exit.y - prev.entry.y;

            var cross = dxc * dyl - dyc * dxl;
            if (cross != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void CalculateDistance(List<Cube> cubes)
        {
            Cube pre = cubes[1];
            Cube pre2 = cubes[0];

            cubes[0].Linear = true;
            cubes[1].Linear = true;

            for (int i = 1; i < cubes.Count(); i++)
            {
                if (!cubes[i].Pattern || cubes[i].Head)
                {
                    if (IsInLinearPath(pre2, pre, cubes[i]))
                    {
                        cubes[i].Linear = true;
                    }

                    pre2 = pre;
                    pre = cubes[i];
                }
            }
        }
    }
}
