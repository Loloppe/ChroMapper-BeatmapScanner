using Beatmap.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChroMapper_BeatmapScanner
{
    public class Plugins
    {
        [Plugin("BeatmapScanner")]
        public class SongNameDuringEditing
        {
            static public BeatSaberSongContainer _beatSaberSongContainer;
            private NoteGridContainer _noteGridContainer;
            private ObstacleGridContainer _obstacleGridContainer;

            private TextMeshProUGUI Label;

            private Scene currentScene;

            public static double Diff { get; set; } = 0;
            public static double Tech { get; set; } = 0;
            public static double EBPM { get; set; } = 0;
            public static double Slider { get; set; } = 0;
            public static double Crouch { get; set; } = 0;
            public static double Reset { get; set; } = 0;
            public static double Bomb { get; set; } = 0;
            public static double Linear { get; set; } = 0;

            [Init]
            private void Init()
            {
                SceneManager.sceneLoaded += SceneLoaded;
                BeatmapActionContainer.ActionCreatedEvent += (_) => Tick();
                BeatmapActionContainer.ActionUndoEvent += (_) => Tick();
                BeatmapActionContainer.ActionRedoEvent += (_) => Tick();
            }

            private void Tick()
            {
                try
                {
                    if(currentScene.buildIndex == 3)
                    {
                        if (_noteGridContainer.LoadedObjects.Any())
                        {
                            List<BaseNote> notes = _noteGridContainer.LoadedObjects.Cast<BaseNote>().Where(n => n.Type != 3).ToList();
                            notes = notes.OrderBy(o => o.JsonTime).ToList();

                            if(notes.Count > 0)
                            {
                                List<BaseNote> bombs = _noteGridContainer.LoadedObjects.Cast<BaseNote>().Where(n => n.Type == 3).ToList();
                                bombs = bombs.OrderBy(b => b.JsonTime).ToList();

                                List<BaseObstacle> obstacles = _obstacleGridContainer.LoadedObjects.Cast<BaseObstacle>().ToList();
                                obstacles = obstacles.OrderBy(o => o.JsonTime).ToList();

                                (Diff, Tech, EBPM, Slider, Reset, Bomb, Crouch, Linear) = BeatmapScanner.Algorithm.BeatmapScanner.Analyzer(notes, bombs, obstacles, BeatSaberSongContainer.Instance.Song.BeatsPerMinute);

                                Label.text = "Diff: " + Diff.ToString() + "   Tech: " + Tech.ToString() + "   EBPM: " + EBPM.ToString() + "   Crouch: " + Crouch.ToString() + "   Linear: " + Linear.ToString() + "%"
                                    + "   Slider: " + Slider.ToString() + "%" + "   Bomb: " + Bomb.ToString() + "%" + "   Reset: " + Reset.ToString() + "%";
                            }
                            else
                            {
                                Label.text = "";
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.LogWarning(e.Message.ToString());
                }
            }

            private void ApplyUI()
            {
                SongTimelineController controller = UnityEngine.Object.FindObjectOfType<SongTimelineController>();
                TextMeshProUGUI songTimeText = controller.transform.Find("Song Time").GetComponent<TextMeshProUGUI>();

                Label = UnityEngine.Object.Instantiate(songTimeText, songTimeText.transform.parent);
                Label.rectTransform.localPosition = new Vector2(-342f, 42f);
                Label.alignment = TextAlignmentOptions.BottomLeft;
                Label.fontSize = 17;
                Label.text = "";
            }

            private void SceneLoaded(Scene scene, LoadSceneMode mode)
            {
                currentScene = scene;

                if (scene.buildIndex == 3)  //Only in the scene where you actually edit the map
                {
                    _noteGridContainer = UnityEngine.Object.FindObjectOfType<NoteGridContainer>();
                    _obstacleGridContainer = UnityEngine.Object.FindObjectOfType<ObstacleGridContainer>();
                    _beatSaberSongContainer = UnityEngine.Object.FindObjectOfType<BeatSaberSongContainer>();

                    if (Label == null)
                    {
                        ApplyUI();
                    }
                }
            }

            [Exit]
            private void Exit()
            {

            }
        }
    }
}
