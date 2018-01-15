using System;
using System.Collections.Generic;

namespace CrossWord
{
    public class CrossGenerator
    {
        readonly ICrossBoard _board;
        readonly ICrossDictionary _dict;

        public delegate void ProgressWatcher(CrossGenerator generator);
        public event ProgressWatcher Watcher;


        public CrossGenerator(ICrossDictionary aDict, ICrossBoard aBoard)
        {
            _dict = aDict;
            _board = aBoard;
        }

        void DoCommands()
        {
            if (Watcher != null)
            {
                Watcher(this);
            }
        }

        public ICrossBoard Board { get { return _board; } }

        /*
          1. Choosing which pattern to fill (i.e. which variable to solve for).
          2. Picking a suitable word (i.e. which value to select).
          3. Choosing where to backtrack to when we reach an impasse.
         */

        public IEnumerable<ICrossBoard> Generate()
        {
            var history = new List<int>();
            var historyTrans = new List<List<CrossTransformation>>();
            var matchingWords = new List<string>();
            var usedWords = new HashSet<string>();
            CrossPattern patt = _board.GetMostConstrainedPattern(_dict);
            while (true)
            {
                DoCommands();
                if (patt != null)
                {
                    matchingWords.Clear();
                    _dict.GetMatch(patt.Pattern, matchingWords);
                    var succTrans = new List<CrossTransformation>();
                    foreach (string t in matchingWords)
                    {
                        if (usedWords.Contains(t)) continue;
                        var trans = patt.TryFill(t, t.AsSpan(), _dict);
                        if (trans != null)
                        {
                            succTrans.Add(trans);
                            trans.Pattern = patt;
                        }
                    }
                    if (succTrans.Count > 0)
                    {
                        succTrans.Sort(new CrossTransformationComparer());
                        var trans = succTrans[0];
                        usedWords.Add(trans.Word);
                        trans.Transform(patt);
                        historyTrans.Add(succTrans);
                        history.Add(0);
                        patt = _board.GetMostConstrainedPattern(_dict);
                    }
                    else
                    {
                        patt = BackTrack(history, historyTrans, usedWords);
                        if (patt == null)
                            yield break;
                    }
                }
                else
                {
                    yield return _board.Clone();
                    patt = BackTrack(history, historyTrans, usedWords);
                    if (patt == null)
                        yield break;
                }
            }
        }

        CrossPattern BackTrack(List<int> history, List<List<CrossTransformation>> historyTrans, HashSet<string> usedWords)
        {
            CrossPattern crossPatternToContinueWith = null;
            while (history.Count > 0)
            {
                int last = history.Count - 1;
                int item = history[last];
                var succTrans = historyTrans[last];
                var trans = succTrans[item];
                trans.Undo(trans.Pattern);
                usedWords.Remove(trans.Word);
                item++;
                if (item < succTrans.Count)
                {
                    var nextTrans = succTrans[item];
                    usedWords.Add(nextTrans.Word);
                    nextTrans.Transform(nextTrans.Pattern);
                    history[last] = item;
                    crossPatternToContinueWith = _board.GetMostConstrainedPattern(_dict);
                    break;
                }
                history.RemoveAt(last);
                historyTrans.RemoveAt(last);
            }
            return crossPatternToContinueWith;
        }
    }
}