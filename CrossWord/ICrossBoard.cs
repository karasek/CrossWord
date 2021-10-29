using System.IO;

namespace CrossWord;

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
    public int StartX { get; set; }
    public int StartY { get; set; }
}

public interface ICrossBoard
{
    //initialization phase
    void AddStartWord(StartWord aStartWord);

    void AddStartWord(int aX, int aY)
    {
        var sw = new StartWord { StartX = aX, StartY = aY };
        AddStartWord(sw);
    }

    void Preprocess(ICrossDictionary aDict);

    int MaxWordLength { get; }

    //enumerate patterns
    int GetPatternCount();
    CrossPattern GetCrossPattern(int aIndex);

    CrossPattern? GetMostConstrainedPattern(ICrossDictionary aDict);

    void WriteTo(StreamWriter writer);
    void WritePatternsTo(StreamWriter writer);
    void WritePatternsTo(StreamWriter writer, ICrossDictionary dictionary);
    void CheckPatternValidity();

    ICrossBoard Clone();
}
