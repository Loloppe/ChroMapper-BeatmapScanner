#region Import

using BeatmapScanner.Algorithm.LackWiz;
using BeatmapScanner.Algorithm.Loloppe;
using System.Collections.Generic;
using System.Linq;
using System;
using Beatmap.Base;
using UnityEngine;

#endregion

namespace BeatmapScanner.Algorithm
{
    internal class BeatmapScanner
    {
        #region Algorithm value

        public static float MaxNerfMS = 500f;
        public static float MinNerfMS = 250f;
        public static float NormalizedMax = 5f;   
        public static float NormalizedMin = 0f;

        public static float MinNote = 80f;

        public static float Speed = 0.00125f;
        public static float Reset = 1.1f;

        #endregion

        #region Analyzer

        public static (double diff, double tech, double ebpm, double slider, double reset, double bomb, int crouch, double linear) Analyzer(List<BaseNote> notes, List<BaseNote> bombs, List<BaseObstacle> obstacles, float bpm)
        {
            #region Prep

            var pass = 0d;
            var tech = 0d;
            var ebpm = 0d;
            var reset = 0d;
            var bomb = 0d;
            var slider = 0d;
            var crouch = 0;
            var linear = 0d;

            List<Cube> cube = new List<Cube>();
            List<SwingData> data = new List<SwingData>();

            foreach(var note in notes)
            {
                cube.Add(new Cube(note));
            }

            cube.OrderBy(c => c.Beat);
            var red = cube.Where(c => c.Note.Type == 0).ToList();
            var blue = cube.Where(c => c.Note.Type == 1).ToList();

            #endregion

            #region Algorithm

            if (red.Count() > 0)
            {
                Helper.FindNoteDirection(red, bombs, bpm);
                Helper.FixPatternHead(red);
                Helper.FindReset(red);
                ebpm = GetIntensity(red, bpm);
                Helper.CalculateDistance(red);
            }

            if (blue.Count() > 0)
            {
                Helper.FindNoteDirection(blue, bombs, bpm);
                Helper.FixPatternHead(blue);
                Helper.FindReset(blue);
                ebpm = Math.Max(GetIntensity(blue, bpm), ebpm);
                Helper.CalculateDistance(blue);
            }

            (pass, tech, data) = Method.UseLackWizAlgorithm(red.Select(c => c.Note).ToList(), blue.Select(c => c.Note).ToList(), bpm);

            #endregion

            #region Calculator

            slider = Math.Round((double)cube.Where(c => c.Slider && (c.Head || !c.Pattern)).Count() / cube.Where(c => c.Head || !c.Pattern).Count() * 100, 2);
            linear = Math.Round((double)cube.Where(c => c.Linear && (c.Head || !c.Pattern)).Count() / cube.Where(c => c.Head || !c.Pattern).Count() * 100, 2);
            reset = Math.Round((double)cube.Where(c => c.Reset && !c.Bomb && (c.Head || !c.Pattern)).Count() / cube.Where(c => c.Head || !c.Pattern).Count() * 100, 2);
            bomb = Math.Round((double)cube.Where(c => c.Reset && c.Bomb && (c.Head || !c.Pattern)).Count() / cube.Where(c => c.Head || !c.Pattern).Count() * 100, 2);

            // Find group of walls and list them together
            List<List<BaseObstacle>> wallsGroup = new List<List<BaseObstacle>>()
            {
                new List<BaseObstacle>()
            };

            for (int i = 0; i < obstacles.Count(); i++)
            {
                wallsGroup.Last().Add(obstacles[i]);

                for (int j = i; j < obstacles.Count() - 1; j++)
                {
                    if (obstacles[j + 1].Time >= obstacles[j].Time && obstacles[j + 1].Time <= obstacles[j].Time + obstacles[j].Duration)
                    {
                        wallsGroup.Last().Add(obstacles[j + 1]);
                    }
                    else
                    {
                        i = j;
                        wallsGroup.Add(new List<BaseObstacle>());
                        break;
                    }
                }
            }

            // Find how many time the player has to crouch
            List<int> wallsFound = new List<int>();
            int count;

            foreach(var group in wallsGroup)
            {
                float found = 0f;
                count = 0;

                for (int j = 0; j < group.Count(); j++)
                {
                    var wall = group[j];

                    if (found != 0f && wall.Time - found < 1.5) // Skip too close
                    {
                        continue;
                    }
                    else
                    {
                        found = 0f;
                    }

                    // Individual
                    if(wall.PosY >= 2 && wall.Width >= 3)
                    {
                        count++;
                        found = wall.Time + wall.Duration;
                    }
                    else if (wall.PosY >= 2 && wall.Width >= 2 && wall.PosX == 1)
                    {
                        count++;
                        found = wall.Time + wall.Duration;
                    }
                    else if (group.Count() > 1) // Multiple
                    {
                        for (int k = j + 1; k < group.Count(); k++)
                        {
                            if(k == j + 100) // So it doesn't take forever on some maps :(
                            {
                                break;
                            }

                            var other = group[k];

                            if ((wall.PosY >= 2 || other.PosY >= 2) && wall.Width >= 2 && wall.PosX == 0 && other.PosX == 2)
                            {
                                count++;
                                found = wall.Time + wall.Duration;
                                break;
                            }
                            else if ((wall.PosY >= 2 || other.PosY >= 2) && other.Width >= 2 && wall.PosX == 2 && other.PosX == 0)
                            {
                                count++;
                                found = wall.Time + wall.Duration;
                                break;
                            }
                            else if ((wall.PosY >= 2 || other.PosY >= 2) && wall.PosX == 1 && other.PosX == 2)
                            {
                                count++;
                                found = wall.Time + wall.Duration;
                                break;
                            }
                        }
                    }
                }

                crouch += count;
            }

            #endregion

            return (Math.Round(pass, 3), Math.Round(tech, 3), Math.Round(ebpm, 3), Math.Round(slider, 3), Math.Round(reset, 3), Math.Round(bomb, 3), crouch, Math.Round(linear, 3));
        }

        #endregion

        #region Intensity

        public static float GetIntensity(List<Cube> cubes, float bpm)
        {
            #region Prep

            var intensity = 1f;
            var speed = (Speed * bpm);
            var previous = 0f;
            var ebpm = 0f;
            var pbpm = 0f;
            var count = 0;
            var prev = cubes[0].Beat;

            #endregion

            #region Algorithm

            for (int i = 1; i < cubes.Count(); i++)
            {
                if (cubes[i].Pattern && !cubes[i].Head)
                {
                    continue;
                }

                var time = (cubes[i].Beat - prev);

                if(time > 0)
                {
                    if (previous == (500 / time) && (500 / time) > ebpm)
                    {
                        count++;
                        ebpm = previous;
                    }
                    else
                    {
                        count = 0;
                    }

                    if ((500 / time) > pbpm)
                    {
                        pbpm = previous;
                    }

                    previous = (500 / time);
                }

                if (cubes[i].Reset || cubes[i].Head)
                {
                    if(time != 0f)
                    {
                        intensity += (speed / time) * Reset;
                    }
                }
                else
                {
                    if (time != 0f)
                    {
                        intensity += speed / time;
                    }
                }

                prev = cubes[i].Beat;
            }

            #endregion

            if(ebpm == 0)
            {
                ebpm = pbpm;
            }
            ebpm *= bpm / 1000;
            intensity /= cubes.Where(c => !c.Pattern || c.Head).Count();

            return ebpm;
        }

        #endregion

    }
}
