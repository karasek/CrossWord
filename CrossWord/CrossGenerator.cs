using System;
using System.Collections.Generic;
using System.Threading;

namespace CrossWord;

public class CrossGenerator
{
    readonly ICrossBoard _board;
    readonly ICrossDictionary _dict;

    public delegate void ProgressWatcher(CrossGenerator generator);

    public event ProgressWatcher? Watcher;


    public CrossGenerator(ICrossDictionary aDict, ICrossBoard aBoard)
    {
        _dict = aDict;
        _board = aBoard;
    }

    void DoCommands()
    {
        Watcher?.Invoke(this);
    }

    public ICrossBoard Board => _board;

    /*
      1. Choosing which pattern to fill (i.e. which variable to solve for).
      2. Picking a suitable word (i.e. which value to select).
      3. Choosing where to backtrack to when we reach an impasse.
     */

    public IEnumerable<ICrossBoard> Generate(CancellationToken cancellationToken)
    {
        var history = new List<int>();
        var historyTrans = new List<List<CrossTransformation>>();
        var usedWords = new HashSet<string>();

        var pattern = _board.GetMostConstrainedPattern(_dict);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DoCommands();
            if (pattern != null)
            {
                var succTrans = FindPossibleCrossTransformations(pattern, usedWords);

                if (succTrans.Count > 0)
                {
                    succTrans.Sort(new CrossTransformationComparer());
                    var trans = succTrans[0];
                    usedWords.Add(trans.Word);
                    trans.Transform(pattern);
                    historyTrans.Add(succTrans);
                    history.Add(0);
                    pattern = _board.GetMostConstrainedPattern(_dict);
                }
                else
                {
                    pattern = BackTrack(history, historyTrans, usedWords);
                    if (pattern == null)
                        yield break;
                }
            }
            else
            {
                yield return _board.Clone();
                pattern = BackTrack(history, historyTrans, usedWords);
                if (pattern == null)
                    yield break;
            }
        }
    }

    List<CrossTransformation> FindPossibleCrossTransformations(CrossPattern pattern, HashSet<string> usedWords)
    {
        var matchingWords = new List<string>();
        _dict.GetMatch(pattern.Pattern, matchingWords);
        var succTrans = new List<CrossTransformation>();
        foreach (string t in matchingWords)
        {
            if (usedWords.Contains(t)) continue;
            var trans = pattern.TryFill(t, t.AsSpan(), _dict);
            if (trans == null) continue;
            succTrans.Add(trans);
            trans.Pattern = pattern;
        }

        return succTrans;
    }

    CrossPattern? BackTrack(List<int> history, List<List<CrossTransformation>> historyTrans,
        HashSet<string> usedWords)
    {
        CrossPattern? crossPatternToContinueWith = null;
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
