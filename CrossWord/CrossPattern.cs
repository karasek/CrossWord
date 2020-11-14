using System;
using System.Linq;

namespace CrossWord
{
    public class CrossPattern
    {
        readonly bool _isHorizontal;
        readonly int _length;
        readonly int _startX;
        readonly int _startY;

        int _instantiationCount;
        char[] _pattern;

        public CrossPattern(int startX, int startY, int length, bool isHorizontal)
        {
            _startX = startX;
            _startY = startY;
            _length = length;
            AdjacentPatterns = new CrossPattern[_length];
            _isHorizontal = isHorizontal;
            _pattern = Enumerable.Repeat('.', length).ToArray();
        }

        public static CrossPattern Empty { get; } = new CrossPattern(0, 0, 0, false);

        public int StartX => _startX;

        public int StartY => _startY;

        public int Length => _length;

        public char[] Pattern
        {
            get => _pattern;
            set => _pattern = value;
        }

        public int InstantiationCount
        {
            get => _instantiationCount;
            set => _instantiationCount = value;
        }

        public CrossPattern[] AdjacentPatterns { get; }

        public CrossTransformation? TryFillPuzzle(ReadOnlySpan<char> word, ICrossDictionary dict)
        {
            for (int i = 0; i < word.Length; i++)
                if (_pattern[i] != '.')
                    return null;
            return TryFill("", word, dict, true);
        }

        public CrossTransformation? TryFill(string dictWord, ReadOnlySpan<char> word, ICrossDictionary dict)
        {
            return TryFill(dictWord, word, dict, false);
        }

        CrossTransformation? TryFill(string dictWord, ReadOnlySpan<char> word, ICrossDictionary dict, bool puzzle)
        {
            var trans = new CrossTransformation(dictWord);
            int instSum = 0;
            for (int i = 0; i < word.Length; i++)
            {
                if (_pattern[i] == '.')
                {
                    if (AdjacentPatterns[i] != null)
                    {
                        int adjIndex;
                        if (_isHorizontal)
                            adjIndex = _startY - AdjacentPatterns[i].StartY;
                        else
                            adjIndex = _startX - AdjacentPatterns[i].StartX;
                        char c = AdjacentPatterns[i].Pattern[adjIndex];
                        if (c == '.')
                        {
                            char[] adjacent = AdjacentPatterns[i].Pattern;
                            adjacent[adjIndex] = word[i];
                            int newInstCount = dict.GetMatchCount(adjacent);
                            adjacent[adjIndex] = '.';
                            if (newInstCount == 0)
                                return null;
                            instSum += newInstCount;
                            trans.AddChangeInstantiation(i, AdjacentPatterns[i].InstantiationCount, newInstCount);
                            trans.AddChange(i, adjIndex, word[i]);
                        }
                        else if (puzzle || c != word[i])
                        {
                            return null;
                        }
                    }

                    trans.AddChange(-1, i, word[i]);
                }
            }

            trans.AddChangeInstantiation(-1, _instantiationCount, (int) Constants.Unbounded);
            trans.SumInst = instSum;
            return trans;
        }

        public override string ToString()
        {
            return (_isHorizontal ? "-" : "|") + $",{_startX},{_startY}," + new string(_pattern);
        }

        public object Clone()
        {
            var result = new CrossPattern(_startX, _startY, _length, _isHorizontal);
            result._instantiationCount = _instantiationCount;
            result._pattern = new char[_pattern.Length];
            Array.Copy(_pattern, result._pattern, _pattern.Length);
            return result;
        }

        public string GetWord()
        {
            return new string(_pattern);
        }
    }
}