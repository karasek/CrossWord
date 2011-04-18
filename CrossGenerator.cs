using System;
using System.Collections.Generic;

namespace CrossWord
{
    public class CrossGenerator
    {
        readonly ICrossBoard _board;
        readonly ICrossDictionary _dict;
        CommandStore _commandStore;

        public CrossGenerator(ICrossDictionary aDict, ICrossBoard aBoard)
        {
            _dict = aDict;
            _board = aBoard;
            _commandStore = null;
        }

        public void SetCommandStore(CommandStore aCommandStore)
        {
            _commandStore = aCommandStore;
        }

        void DoCommands()
        {
            //_board.CheckPatternValidity();
            if (_commandStore != null && _commandStore.Count > 0)
            {
                while (_commandStore.Count > 0)
                {
                    string command = _commandStore.PopCommand();
                    if (command == null) break;
                    if (command.Equals("h"))
                    {
                        //write help
                        Console.WriteLine("Commands help: ");
                        Console.WriteLine("h - show this help");
                        Console.WriteLine("d - display cross");
                        Console.WriteLine("p - display patterns");
                        Console.WriteLine("c - check");
                    }
                    else if (command.Equals("d"))
                    {
                        _board.OutputToConsole();
                    }
                    else if (command.Equals("p"))
                    {
                        _board.OutputPatternsToConsole();
                    }
                    else if (command.Equals("c"))
                    {
                        _board.CheckPatternValidity();
                    }
                    else
                    {
                        Console.WriteLine("unknown command: {0}", command);
                    }
                }
            }
        }

        /*
          1. Choosing which pattern to fill (i.e. which variable to solve for).
          2. Picking a suitable word (i.e. which value to select).
          3. Choosing where to backtrack to when we reach an impasse.
         */

        public void Generate()
        {
            var history = new List<int>();
            var historyTrans = new List<List<CrossTransformation>>();
            var matchingWords = new List<string>();
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
                        CrossTransformation trans = patt.TryFill(t, _dict);
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
                        trans.Transform(patt);
                        historyTrans.Add(succTrans);
                        history.Add(0);
                        patt = _board.GetMostConstrainedPattern(_dict);
                    }
                    else
                    {
                        //backtrack
                        while (history.Count > 0)
                        {
                            int last = history.Count - 1;
                            int item = history[last];
                            succTrans = historyTrans[last];
                            var trans =  succTrans[item];
                            trans.Undo(trans.Pattern);
                            item++;
                            if (item < succTrans.Count)
                            {
                                var nextTrans =  succTrans[item];
                                nextTrans.Transform(nextTrans.Pattern);
                                history[last] = item;
                                patt = _board.GetMostConstrainedPattern(_dict);
                                break;
                            }
                            history.RemoveAt(last);
                            historyTrans.RemoveAt(last);
                        }
                        if (history.Count == 0)
                        {
                            Console.WriteLine("No solution!");
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Solution has been found.");
                    _board.OutputToConsole();
                    return;
                }
            }
        }
    }
}