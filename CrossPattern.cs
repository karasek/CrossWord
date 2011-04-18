using System;
using System.Collections.Generic;

namespace CrossWord
{
    public class CrossPattern : object
    {
        int _instantiationCount;
        bool _isHorizontal;
        int _length;
        char[] _pattern;

        int _startX;

        int _startY;

        public CrossPattern()
        {
            _startX = _startY = -1;
            _instantiationCount = 0;
            _length = 0;
        }

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
            set { _startX = value; }
        }

        public int StartY
        {
            get { return _startY; }
            set { _startY = value; }
        }

        public int Length
        {
            get { return _length; }
            set
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

        public CrossTransformation TryFill(string aWord, ICrossDictionary aDict)
        {
            var trans = new CrossTransformation();
            int instSum = 0;
            for (int i = 0; i < aWord.Length; i++)
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
                            adjacent[adjIndex] = aWord[i];
                            int newInstCount = aDict.GetMatchCount(adjacent);
                            adjacent[adjIndex] = '.';
                            if (newInstCount == 0)
                                return null;
                            instSum += newInstCount;
                            trans.AddChangeInst(i, AdjacentPatterns[i].InstantiationCount, newInstCount);
                            trans.AddChange(i, adjIndex, aWord[i]);
                        }
                        else if (c != aWord[i])
                        {
                            return null;
                        }
                    }
                    trans.AddChange(-1, i, aWord[i]);
                }
            }
            trans.AddChangeInst(-1, _instantiationCount, (int)Constants.Unbounded);
            trans.SumInst = instSum;
            return trans;
        }
    }
}