using System;
using System.Collections.Generic;

namespace CrossWord
{
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

    public class CrossBoard : object, ICrossBoard
    {
        IList<CrossPattern> _horizontalPatterns;
        int _sizeX;
        int _sizeY;
        List<StartWord> _startWords; //StartWord
        IList<CrossPattern> _verticalPatterns;

        public void SetBoardSize(int aX, int aY)
        {
            _horizontalPatterns = null;
            _verticalPatterns = null;
            _startWords = new List<StartWord>();
            _sizeX = aX;
            _sizeY = aY;
        }

        public void AddStartWord(StartWord aStartWord)
        {
            _startWords.Add(aStartWord);
        }

        public void AddStartWord(int aX, int aY)
        {
            var sw = new StartWord { StartX = aX, StartY = aY };
            AddStartWord(sw);
        }

        public void Preprocess(ICrossDictionary aDict)
        {
            _horizontalPatterns = new List<CrossPattern>();
            _startWords.Sort(new YXStartWordComparer()); //first create horizontal patterns

            int wordIdx = 0;
            for (int y = 0; y < _sizeY; y++)
            {
                int nextX = 0;
                while (wordIdx < _startWords.Count)
                {
                    var sw = _startWords[wordIdx];
                    //Console.WriteLine("StartWord x:{0} y:{1} idx:{2}/cnt:{3}",sw.StartX,sw.StartY,wordIdx,_startWords.Count);
                    if (sw.StartY == y)
                    {
                        if (sw.StartX - nextX > 0)
                        {
                            var cp = new CrossPattern(nextX, y, sw.StartX - nextX, true);
                            //Console.WriteLine("SW pattern startX: {0} startY: {1} len: {2}",cp.StartX, cp.StartY, cp.Length);
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
                if (_sizeX - nextX > 0)
                {
                    var cp = new CrossPattern(nextX, y, _sizeX - nextX, true);
                    //Console.WriteLine("EL pattern startX: {0} startY: {1} len: {2}",cp.StartX, cp.StartY, cp.Length);
                    _horizontalPatterns.Add(cp);
                }
            }

            _verticalPatterns = new List<CrossPattern>();
            _startWords.Sort(new XYStartWordComparer()); //first create horizontal patterns

            wordIdx = 0;
            for (int x = 0; x < _sizeX; x++)
            {
                int nextY = 0;
                while (wordIdx < _startWords.Count)
                {
                    var sw = _startWords[wordIdx];
                    //Console.WriteLine("StartWord x:{0} y:{1} idx:{2}/cnt:{3}",sw.StartX,sw.StartY,wordIdx,_startWords.Count);
                    if (sw.StartX == x)
                    {
                        if (sw.StartY - nextY > 0)
                        {
                            var cp = new CrossPattern(x, nextY, sw.StartY - nextY, false);
                            //Console.WriteLine("SW patternY startX: {0} startY: {1} len: {2}",cp.StartX, cp.StartY, cp.Length);
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
                if (_sizeY - nextY > 0)
                {
                    var cp = new CrossPattern(x, nextY, _sizeY - nextY, false);
                    //Console.WriteLine("EL patternY startX: {0} startY: {1} len: {2}",cp.StartX, cp.StartY, cp.Length);
                    _verticalPatterns.Add(cp);
                }
            }
            //find adjacent patterns
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
                        //Console.WriteLine("New dep for startX: {0} startY: {1} and startX {2} start Y {3} ",hor.StartX, hor.StartY, ver.StartX, ver.StartY);
                    }
                }
            }
            //calculate instantiation count
            var wordLengthCount = new int[16];
            var emptyPatterns = new[]
                                    {
                                        "", ".", "..", "...", "....", ".....", "......",
                                        ".......", "........", ".........", "..........", "...........", //7-11
                                        "............", ".............", "..............", "..............."
                                    };
            for (int i = 1; i < 16; i++)
            {
                wordLengthCount[i] = aDict.GetWordOfLengthCount(i);
            }
            int patternCount = GetPatternCount();
            for (int i = 0; i < patternCount; i++)
            {
                CrossPattern pattern = GetCrossPattern(i);
                if (pattern.Pattern == null)
                {
                    //empty
                    pattern.InstantiationCount = wordLengthCount[pattern.Length];
                    pattern.Pattern = emptyPatterns[pattern.Length].ToCharArray();
                }
                else
                {
                    //already set some letters
                    pattern.InstantiationCount = aDict.GetMatchCount(pattern.Pattern);
                }
            }
        }

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

        //get pattern at given position
        public CrossPattern GetCrossPattern(int aStartX, int aStartY, Orientation aOrientation)
        {
            return _horizontalPatterns[0]; //todo getCrossPattern
        }

        public CrossPattern GetMostConstrainedPattern(ICrossDictionary aDict)
        {
            var min = (int)Constants.Unbounded;
            int cnt = GetPatternCount();
            CrossPattern result = null;
            for (int i = 0; i < cnt; i++)
            {
                if (GetCrossPattern(i).InstantiationCount < min)
                {
                    result = GetCrossPattern(i);
                    min = result.InstantiationCount;
                }
            }
            return result;
        }


        public void OutputToConsole()
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

            for (int y = 0; y < _sizeY; y++)
            {
                string row = "";
                for (int x = 0; x < _sizeX; x++)
                    row += board[x, y] + " ";
                Console.WriteLine("{0:00}: {1}", y, row);
            }
            Console.WriteLine("");

            foreach (var p in _verticalPatterns)
            {
                for (int y = p.StartY; y < p.StartY + p.Length; y++)
                {
                    if (p.Pattern != null && board[p.StartX, y] != p.Pattern[y - p.StartY])
                        throw new Exception("X/Y patterns inconsistency");
                }
            }

        }

        public void OutputPatternsToConsole()
        {
            Console.WriteLine("Patterns: ");
            int cnt = GetPatternCount();
            for (int i = 0; i < cnt; i++)
            {
                CrossPattern p = GetCrossPattern(i);
                int ic = p.InstantiationCount;
                if (ic == (int)Constants.Unbounded)
                {
                    ic = 1;
                }
                Console.WriteLine("{0} : {1}", new string(p.Pattern), ic);
            }
            Console.WriteLine();
        }

    }
}