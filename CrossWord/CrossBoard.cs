using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrossWord;

public class XYStartWordComparer : IComparer<StartWord>
{
    int IComparer<StartWord>.Compare(StartWord sw1, StartWord sw2)
    {
        int result = sw1.StartX.CompareTo(sw2.StartX);
        if (result == 0)
            result = sw1.StartY.CompareTo(sw2.StartY);
        return result;
    }
}

public class YXStartWordComparer : IComparer<StartWord>
{
    int IComparer<StartWord>.Compare(StartWord sw1, StartWord sw2)
    {
        int result = sw1.StartY.CompareTo(sw2.StartY);
        if (result == 0)
            result = sw1.StartX.CompareTo(sw2.StartX);
        return result;
    }
}

public class CrossBoard : ICrossBoard
{
    readonly int _sizeX;
    readonly int _sizeY;
    readonly List<StartWord> _startWords; //StartWord

    List<CrossPattern> _horizontalPatterns;
    List<CrossPattern> _verticalPatterns;

    public CrossBoard(int aX, int aY)
    {
        _sizeX = aX;
        _sizeY = aY;
        _startWords = new List<StartWord>();

        _horizontalPatterns = new List<CrossPattern>();
        _verticalPatterns = new List<CrossPattern>();
    }

    public void AddStartWord(StartWord aStartWord)
    {
        _startWords.Add(aStartWord);
    }

    const int MinPatternLength = 2;

    public void Preprocess(ICrossDictionary aDict)
    {
        _horizontalPatterns.Clear();
        _startWords.Sort(new YXStartWordComparer()); //first create horizontal patterns

        int wordIdx = 0;
        for (int y = 0; y < _sizeY; y++)
        {
            int nextX = 0;
            while (wordIdx < _startWords.Count)
            {
                var sw = _startWords[wordIdx];
                if (sw.StartY == y)
                {
                    if (sw.StartX - nextX >= MinPatternLength)
                    {
                        var cp = new CrossPattern(nextX, y, sw.StartX - nextX, true);
                        _horizontalPatterns.Add(cp);
                    }

                    nextX = sw.StartX + 1;
                    wordIdx++;
                }
                else
                {
                    break;
                }
            }

            if (_sizeX - nextX >= MinPatternLength)
            {
                var cp = new CrossPattern(nextX, y, _sizeX - nextX, true);
                _horizontalPatterns.Add(cp);
            }
        }

        _verticalPatterns.Clear();
        _startWords.Sort(new XYStartWordComparer()); //first create horizontal patterns

        wordIdx = 0;
        for (int x = 0; x < _sizeX; x++)
        {
            int nextY = 0;
            while (wordIdx < _startWords.Count)
            {
                var sw = _startWords[wordIdx];
                if (sw.StartX == x)
                {
                    if (sw.StartY - nextY >= MinPatternLength)
                    {
                        var cp = new CrossPattern(x, nextY, sw.StartY - nextY, false);
                        _verticalPatterns.Add(cp);
                    }

                    nextY = sw.StartY + 1;
                    wordIdx++;
                }
                else
                {
                    break;
                }
            }

            if (_sizeY - nextY >= MinPatternLength)
            {
                var cp = new CrossPattern(x, nextY, _sizeY - nextY, false);
                _verticalPatterns.Add(cp);
            }
        }

        BindAdjacentPatterns();
        //set instantiation count
        int patternCount = GetPatternCount();
        for (int i = 0; i < patternCount; i++)
        {
            var pattern = GetCrossPattern(i);
            pattern.InstantiationCount = aDict.GetWordOfLengthCount(pattern.Length);

        }
    }

    void BindAdjacentPatterns()
    {
        foreach (var hor in _horizontalPatterns)
        {
            foreach (var ver in _verticalPatterns)
            {
                if (ver.StartX >= hor.StartX && ver.StartX < hor.StartX + hor.Length &&
                    hor.StartY >= ver.StartY && hor.StartY < ver.StartY + ver.Length)
                {
                    //adjacent
                    hor.AdjacentPatterns[ver.StartX - hor.StartX] = ver;
                    ver.AdjacentPatterns[hor.StartY - ver.StartY] = hor;
                }
            }
        }
    }

    public int MaxWordLength => Math.Max(_sizeX, _sizeY);

    public int GetPatternCount()
    {
        return _horizontalPatterns.Count + _verticalPatterns.Count;
    }

    public CrossPattern GetCrossPattern(int aIndex)
    {
        if (aIndex < _horizontalPatterns.Count)
            return _horizontalPatterns[aIndex];
        return _verticalPatterns[aIndex - _horizontalPatterns.Count];
    }

    public CrossPattern? GetMostConstrainedPattern(ICrossDictionary aDict)
    {
        var min = (int)Constants.Unbounded;
        CrossPattern? result = null;
        foreach (var p in _horizontalPatterns)
        {
            if (p.InstantiationCount >= min)
                continue;
            result = p;
            min = p.InstantiationCount;
        }

        foreach (var p in _verticalPatterns)
        {
            if (p.InstantiationCount >= min)
                continue;
            result = p;
            min = p.InstantiationCount;
        }

        return result;
    }


    public void WriteTo(StreamWriter writer)
    {
        var board = new char[_sizeX, _sizeY];

        foreach (var sw in _startWords)
        {
            board[sw.StartX, sw.StartY] = '-';
        }

        foreach (var p in _horizontalPatterns)
        {
            for (int x = p.StartX; x < p.StartX + p.Length; x++)
            {
                if (p.Pattern != null)
                    board[x, p.StartY] = p.Pattern[x - p.StartX];
                else
                    board[x, p.StartY] = '.';
            }
        }

        foreach (var p in _verticalPatterns)
        {
            for (int y = p.StartY; y < p.StartY + p.Length; y++)
            {
                if (p.Pattern != null)
                {
                    var c = p.Pattern[y - p.StartY];
                    if (c != '.')
                        board[p.StartX, y] = c;
                }
            }
        }

        StringBuilder sb = new();
        for (int y = 0; y < _sizeY; y++)
        {
            for (int x = 0; x < _sizeX; x++)
            {
                sb.Append(board[x, y]);
                sb.Append(' ');
            }
            writer.WriteLine("{0:00}: {1}", y, sb);
            sb.Clear();
        }

        writer.WriteLine();

    }

    public void WritePatternsTo(StreamWriter writer)
    {
        writer.WriteLine("Patterns: ");
        int cnt = GetPatternCount();
        for (int i = 0; i < cnt; i++)
        {
            writer.WriteLine(GetCrossPattern(i));
        }
    }

    public void WritePatternsTo(StreamWriter writer, ICrossDictionary dictionary)
    {
        writer.WriteLine("Patterns: ");
        int cnt = GetPatternCount();
        for (int i = 0; i < cnt; i++)
        {
            var pattern = GetCrossPattern(i);
            var word = pattern.GetWord();
            if (!dictionary.TryGetDescription(word, out var description))
                description = "[PUZZLE]";
            writer.WriteLine($"{pattern},{description}");
        }
    }

    public void CheckPatternValidity()
    {
        foreach (var p in _horizontalPatterns)
        {
            for (int i = 0; i < p.AdjacentPatterns.Length; i++)
            {
                var ap = p.AdjacentPatterns[i];
                if (ap == null) continue;
                if (ap.Pattern[p.StartY - ap.StartY] != p.Pattern[i])
                    throw new Exception("X/Y inconsistency");
            }
        }

        foreach (var p in _verticalPatterns)
        {
            for (int i = 0; i < p.AdjacentPatterns.Length; i++)
            {
                var ap = p.AdjacentPatterns[i];
                if (ap == null) continue;
                if (ap.Pattern[p.StartX - ap.StartX] != p.Pattern[i])
                    throw new Exception("Y/X inconsistency");
            }
        }
    }

    public ICrossBoard Clone()
    {
        var result = new CrossBoard(_sizeX, _sizeY);
        result._startWords.AddRange(_startWords);
        result._horizontalPatterns = new List<CrossPattern>();
        foreach (var patt in _horizontalPatterns)
        {
            result._horizontalPatterns.Add((CrossPattern)patt.Clone());
        }

        result._verticalPatterns = new List<CrossPattern>();
        foreach (var patt in _verticalPatterns)
        {
            result._verticalPatterns.Add((CrossPattern)patt.Clone());
        }

        result.BindAdjacentPatterns();

        return result;
    }
}
