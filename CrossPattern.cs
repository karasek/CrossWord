using System;
using System.Collections.Generic;

namespace CrossWord
{
    //descending sort to find patterns which does not
    //constrain so much
    public class CrossTransformationComparer : IComparer<CrossTransformation>
    {
        int IComparer<CrossTransformation>.Compare(CrossTransformation t1, CrossTransformation t2)
        {
            return t2.SumInst.CompareTo(t1.SumInst);
        }
    }

    public class CrossTransformation : object
    {
        readonly IList<int> _changes;
        readonly IList<int> _instChanges;

        public CrossTransformation()
        {
            _changes = new List<int>();
            _instChanges = new List<int>();
        }

        public int SumInst { get; set; }

        public CrossPattern Pattern { get; set; }

        public void Transform(CrossPattern aCrossPattern)
        {
            for (int i = 0; i < _changes.Count; i += 3)
            {
                var adjIdx = _changes[i];
                var pos = _changes[i + 1];
                var newChar = (char)_changes[i + 2];
                char[] pattern;
                if (adjIdx == -1)
                    pattern = aCrossPattern.Pattern;
                else
                    pattern = aCrossPattern.AdjacentPatterns[adjIdx].Pattern;
                pattern[pos] = newChar;
            }
            for (int i = 0; i < _instChanges.Count; i += 3)
            {
                var adjIdx = _instChanges[i];
                var newInst = _instChanges[i + 2];
                if (adjIdx != -1)
                {
                    aCrossPattern.AdjacentPatterns[adjIdx].InstantiationCount = newInst;
                }
                else
                {
                    aCrossPattern.InstantiationCount = newInst;
                }
            }
            //Console.WriteLine("Transform: {0}", new String(aCrossPattern.Pattern));
        }

        public void Undo(CrossPattern aCrossPattern)
        {
            //Console.WriteLine("Undo: {0}", new String(aCrossPattern.Pattern));
            for (int i = 0; i < _changes.Count; i += 3)
            {
                var adjIdx = _changes[i];
                var pos = _changes[i + 1];
                char[] pattern;
                if (adjIdx == -1)
                    pattern = aCrossPattern.Pattern;
                else
                    pattern = aCrossPattern.AdjacentPatterns[adjIdx].Pattern;
                pattern[pos] = '.';
            }
            for (int i = 0; i < _instChanges.Count; i += 3)
            {
                var adjIdx = _instChanges[i];
                var old = _instChanges[i + 1];
                if (adjIdx != -1)
                    aCrossPattern.AdjacentPatterns[adjIdx].InstantiationCount = old;
                else
                    aCrossPattern.InstantiationCount = old;
            }
        }

        //create
        public void AddChange(int aAdjancedIdx, int aPosition, char aNewChar)
        {
            if (aPosition < 0) throw new Exception("Bad index.");
            _changes.Add(aAdjancedIdx);
            _changes.Add(aPosition);
            _changes.Add(aNewChar);
        }

        public void AddChangeInst(int aAdjancedIdx, int aInst, int aNewInst)
        {
            _instChanges.Add(aAdjancedIdx);
            _instChanges.Add(aInst);
            _instChanges.Add(aNewInst);
        }
    }

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
                if (_pattern[i] == '.' && AdjacentPatterns[i] != null)
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
                    if (_pattern[i] == '.')
                    {
                        trans.AddChange(-1, i, aWord[i]);
                    }
                }
            }
            trans.AddChangeInst(-1, _instantiationCount, (int)Constants.Unbounded);
            trans.SumInst = instSum;
            return trans;
        }
    }
}