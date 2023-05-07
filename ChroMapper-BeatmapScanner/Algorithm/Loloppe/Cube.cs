using Beatmap.Base;

namespace BeatmapScanner.Algorithm.Loloppe
{
    internal class Cube
    {
        public float Time { get; set; } = 0;
        public int Line { get; set; } = 0;
        public int Layer { get; set; } = 0;
        public int Type { get; set; } = 0;
        public int CutDirection { get; set; } = 0;
        public double AngleOffset { get; set; } = 0;
        public double Direction { get; set; } = 8;
        public bool Reset { get; set; } = false;
        public bool Bomb { get; set; } = false;
        public bool Head { get; set; } = false;
        public bool Pattern { get; set; } = false;
        public bool Slider { get; set; } = false;
        public bool Linear { get; set; } = false;
        public bool Assumed { get; set; } = false;

        public Cube(Cube cube)
        {
            AngleOffset = cube.AngleOffset;
            CutDirection = cube.CutDirection;
            Type = cube.Type;
            Time = cube.Time;
            Line = cube.Line;
            Layer = cube.Layer;
            Direction = cube.Direction;
            Assumed = cube.Assumed;
        }

        public Cube(BaseNote note)
        {
            AngleOffset = note.AngleOffset;
            CutDirection = note.CutDirection;
            Type = note.Type;
            Time = note.JsonTime;
            Line = note.PosX;
            Layer = note.PosY;
            Direction = note.CutDirection;
            if(note.CutDirection == 8)
            {
                Assumed = true;
            }
        }
    }
}
