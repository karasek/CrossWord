using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrossWord.TestApp
{
    static class Program
    {
        static CommandStore _commandStore;

        static CrossGenerator CreateGenerator(string file, string dictFile, CommandStore commands)
        {
            DateTime startTime = DateTime.Now;
            var cb = CrossBoardCreator.CreateFromFile(file);
            var dict = new Dictionary(dictFile, cb.MaxWordLength);
            cb.Preprocess(dict);
            var gen = new CrossGenerator(dict, cb);
            gen.Watcher += GeneratorWatcher;
            return gen;
        }

        static StreamWriter OpenConsoleWriter()
        {
            var w = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
            return w;
        }

        static void GeneratorWatcher(CrossGenerator generator)
        {
            while (_commandStore.Count > 0)
            {
                var command = _commandStore.PopCommand();
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
                    using (var w = OpenConsoleWriter())
                        generator.Board.WriteTo(w);
                }
                else if (command.Equals("p"))
                {
                    using (var w = OpenConsoleWriter())
                        generator.Board.WritePatternsTo(w);
                }
                else if (command.Equals("c"))
                {
                    generator.Board.CheckPatternValidity();
                }
                else
                {
                    Console.WriteLine("unknown command: {0}", command);
                }
            }
        }

        public static void GenerateAndOutput(CrossGenerator generator, CommandStore commands, int maxSolutionsCount)
        {
            int solutionsCount = 0;
            foreach (var solution in generator.Generate())
            {
                lock (commands.Lock)
                {
                    Console.WriteLine($"Solution {solutionsCount} found:");
                    using (var w = OpenConsoleWriter())
                        solution.WriteTo(w);
                }

                if (++solutionsCount == maxSolutionsCount)
                {
                    Console.WriteLine($"{solutionsCount} solutions found.");
                    break;
                }
            }

            if (solutionsCount == 0)
                Console.WriteLine("Solution not found:");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting");
            DateTime startTime = DateTime.Now;

            _commandStore = new CommandStore();
            var generators = new List<CrossGenerator>
            {
                CreateGenerator("../templates/template1.txt", "../dict/cz", _commandStore),
                CreateGenerator("../templates/template2.txt", "../dict/words", _commandStore),
                CreateGenerator("../templates/template3.txt", "../dict/words", _commandStore),
                CreateGenerator("../templates/template4.txt", "../dict/cz", _commandStore),
                CreateGenerator("../templates/american.txt", "../dict/words", _commandStore),
                CreateGenerator("../templates/british.txt", "../dict/words", _commandStore),
                CreateGenerator("../templates/japanese.txt", "../dict/words", _commandStore)
            };
            //command reader
            const int maxSolutionsCount = 100;
            var ri = new ReadInput(_commandStore);
            Task.Run(() => ri.Run());

            var tasks =
                generators.Select(gen1 => Task.Factory.StartNew(() =>
                    GenerateAndOutput(gen1, _commandStore, maxSolutionsCount))).ToArray();
            Task.WaitAll(tasks);
            ri.ShouldStop = true;

            TimeSpan timeSpan = DateTime.Now - startTime;
            Console.WriteLine("Time elapsed: {0}", timeSpan);
        }
    }
}