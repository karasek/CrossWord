using System;

namespace CrossWord
{
    public class CrossPattern
    {
        int _instantiationCount;
        bool _isHorizontal;
        int _length;
        char[] _pattern;

        readonly int _startX;
        readonly int _startY;

        public CrossPattern(int startX, int startY, int length, bool isHorizontal)
        {
            _startX = startX;
            _startY = startY;
            Length = length;
            _isHorizontal = isHorizontal;
        }

        public bool IsHorizontal
        {
            get { return _isHorizontal; }
            set { _isHorizontal = value; }
        }

        public int StartX
        {
            get { return _startX; }
        }

        public int StartY
        {
            get { return _startY; }
        }

        public int Length
        {
            get { return _length; }
            private set
            {
                _length = value;
                AdjacentPatterns = new CrossPattern[_length];
            }
        }

        public char[] Pattern
        {
            get { return _pattern; }
            set { _pattern = value; }
        }

        public int InstantiationCount
        {
            get { return _instantiationCount; }
            set { _instantiationCount = value; }
        }

        public CrossPattern[] AdjacentPatterns { get; private set; }

        public CrossTransformation TryFillPuzzle(string word, ICrossDictionary dict)
        {
            for (int i = 0; i < word.Length; i++ )
                if (_pattern[i] != '.')
                    return null;
            return TryFill(word, dict, true);
        }

        public CrossTransformation TryFill(string word, ICrossDictionary dict)
        {
            return TryFill(word, dict, false);
        }

        CrossTransformation TryFill(string word, ICrossDictionary dict, bool puzzle)
        {
            var trans = new CrossTransformation(word);
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
                            trans.AddChangeInst(i, AdjacentPatterns[i].InstantiationCount, newInstCount);
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
            trans.AddChangeInst(-1, _instantiationCount, (int)Constants.Unbounded);
            trans.SumInst = instSum;
            return trans;
        }

        public override string ToString()
        {
            return (_isHorizontal ? "-" : "|") + string.Format(",{0},{1},", _startX, _startY) + new string(_pattern);
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