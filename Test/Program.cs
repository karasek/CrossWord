using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrossWord;

namespace CrossWordTest
{
    static class Program
    {
        static CommandStore _commandStore;

        static void CreateCross(ICrossBoard cb)
        {
            const int sizex = 11;
            const int sizey = 15;
            cb.SetBoardSize(sizex, sizey);
            for (int i = 0; i < sizey; i++)
            {
                cb.AddStartWord(0, i);
            }
            for (int i = 1; i < sizex; i++)
            {
                cb.AddStartWord(i, 0);
            }
            cb.AddStartWord(7, 1);
            cb.AddStartWord(4, 3);
            cb.AddStartWord(3, 4);
            cb.AddStartWord(6, 4);
            cb.AddStartWord(7, 5);
            cb.AddStartWord(4, 6);
            cb.AddStartWord(1, 7);
            cb.AddStartWord(5, 7);
            cb.AddStartWord(9, 7);
            cb.AddStartWord(10, 7);
            cb.AddStartWord(8, 8);
            cb.AddStartWord(3, 9);
            cb.AddStartWord(6, 9);
            cb.AddStartWord(4, 10);
            cb.AddStartWord(7, 10);
            cb.AddStartWord(8, 11);
            cb.AddStartWord(5, 12);
            cb.AddStartWord(6, 13);
            cb.AddStartWord(6, 14);
            cb.AddStartWord(10, 14);
        }

        static void oldTest()
        {
            //prepare cross board
            ICrossBoard cb = new CrossBoard();
            CreateCross(cb);
            var dict = new Dictionary("../../../dict/cz", cb.MaxWordLength);
            cb.Preprocess(dict);

            CrossPattern cp = cb.GetCrossPattern(32);
            CrossTransformation trans = cp.TryFill("ADELAVOJTAHELA", dict); //length 14
            trans.Transform(cp);
        }

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
            var w = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            return w;
        }

        static void GeneratorWatcher(CrossGenerator generator)
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
                    using (var w = OpenConsoleWriter())
                        generator.Board.WriteTo(w);
                }
                else if (command.Equals("p"))
                {
                    using (var w = OpenConsoleWriter())
                        generator.Board.WritePatternsTo(w, null);
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
                    break;
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
                    CreateGenerator("../templates/Template1.txt", "../dict/cz", _commandStore),
                    CreateGenerator("../templates/Template2.txt", "../dict/words", _commandStore),
                    CreateGenerator("../templates/Template3.txt", "../dict/cz", _commandStore),
                    CreateGenerator("../templates/Template4.txt", "../dict/cz", _commandStore),
                    CreateGenerator("../templates/american.txt", "../dict/words", _commandStore),
                    CreateGenerator("../templates/british.txt", "../dict/words", _commandStore),
                    CreateGenerator("../templates/japanese.txt", "../dict/cz", _commandStore)
                };
            //command reader
            const int maxSolutionsCount = 3;
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