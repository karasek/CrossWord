namespace CrossWord
{
    public enum Constants
    {
        Unbounded = 999999999,
    }

    public enum Orientation : byte
    {
        Horizontal = 0,
        Vertical = 1,
    }

    public struct StartWord
    {
        string[] _label; //one for each Orientation
        public int StartX { get; set; }

        public int StartY { get; set; }

        public string[] Label
        {
            get { return _label ?? (_label = new string[2]); }
        }
    }

    /// <summary>
    /// Description of ICrossBoard.
    /// </summary>
    public interface ICrossBoard
    {
        //initialization phase
        void SetBoardSize(int aX, int aY);
        void AddStartWord(StartWord aStartWord);
        void AddStartWord(int aX, int aY);
        void Preprocess(ICrossDictionary aDict);

        int MaxWordLength { get; }

        //enumerate patterns
        int GetPatternCount();
        CrossPattern GetCrossPattern(int aIndex);

        CrossPattern GetMostConstrainedPattern(ICrossDictionary aDict);

        //get pattern at given position
        CrossPattern GetCrossPattern(int aStartX, int aStartY, Orientation aOrientation);

        void OutputToConsole();
        void OutputPatternsToConsole();
        void CheckPatternValidity();
    }
}