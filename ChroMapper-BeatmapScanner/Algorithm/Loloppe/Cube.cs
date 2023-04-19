using Beatmap.Base;

namespace BeatmapScanner.Algorithm.Loloppe
{
    internal class Cube
    {
        public BaseNote Note { get; set; }
        public float Beat { get; set; } = 0;
        public int Line { get; set; } = 0;
        public int Layer { get; set; } = 0;
        public double Direction { get; set; } = 8;
        public bool Assumed { get; set; } = false;
        public bool Reset { get; set; } = false;
        public bool Bomb { get; set; } = false;
        public bool Head { get; set; } = false;
        public bool Pattern { get; set; } = false;
        public bool Slider { get; set; } = false;
        public bool Linear { get; set; } = false;


        public Cube(BaseNote note)
        {
            Note = note;
            Beat = note.Time;
            Line = note.PosX;
            Layer = note.PosY;
            Direction = note.CutDirection;
            if(Direction == 8)
            {
                Assumed = true;
            }
        }
    }
}
