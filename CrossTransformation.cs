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
}