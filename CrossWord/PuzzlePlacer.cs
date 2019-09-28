using System;
using System.Collections.Generic;
using System.Text;

namespace CrossWord
{
    class PuzzlePlacer
    {
        readonly string _puzzle;
        readonly ICrossBoard _board;

        public PuzzlePlacer(ICrossBoard board, string puzzle)
        {
            _board = board;
            _puzzle = puzzle;
        }

        public IEnumerable<ICrossBoard> GetAllPossiblePlacements(ICrossDictionary dictionary)
        {
            var puzzle = NormalizePuzzle(_puzzle).AsMemory();
            var board = _board.Clone();
            board.Preprocess(dictionary);
            var patterns = new List<CrossPattern>();
            for (int i = 0; i < board.GetPatternCount(); i++)
                patterns.Add(board.GetCrossPattern(i));
            patterns.Sort((a, b) => -1 * a.Length.CompareTo(b.Length));
            if (patterns.Count == 0)
                yield break;

            var restPuzzleLength = puzzle.Length;
            var stack = new List<int>();
            var appliedTransformations = new List<CrossTransformation>();
            int idx = 0;
            while (true)
            {
                continueOuterLoop:
                for (; idx < patterns.Count; idx++)
                {
                    var pattern = patterns[idx];
                    if (restPuzzleLength < pattern.Length) continue;
                    if (restPuzzleLength - pattern.Length == 1) continue;
                    var trans = pattern.TryFillPuzzle(puzzle.Slice(puzzle.Length - restPuzzleLength,
                        pattern.Length).Span, dictionary);
                    if (trans != null)
                    {
                        trans.Transform(pattern);
                        if (restPuzzleLength == pattern.Length)
                        {
                            var cloned = board.Clone();
                            trans.Undo(pattern);
                            yield return cloned;
                            continue;
                        }

                        stack.Add(idx + 1);
                        trans.Pattern = pattern;
                        appliedTransformations.Add(trans);
                        restPuzzleLength -= pattern.Length;
                        idx = 0;
                        goto continueOuterLoop;
                    }
                }

                if (stack.Count == 0)
                    break;
                idx = stack.Back();
                stack.Pop();
                var appTr = appliedTransformations.Back();
                appliedTransformations.Pop();
                appTr.Undo(appTr.Pattern);
                restPuzzleLength += appTr.Pattern.Length;
            }
        }

        static string NormalizePuzzle(string puzzle)
        {
            var builder = new StringBuilder(puzzle.Length);
            foreach (var c in puzzle)
            {
                if (Char.IsLetter(c))
                    builder.Append(Char.ToUpper(c));
            }

            return builder.ToString();
        }
    }
}
