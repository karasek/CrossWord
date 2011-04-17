using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrossWord
{
    internal static class MainClass
    {
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
            var dict = new Dictionary("../../dict/cz", cb.MaxWordLength);
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
            gen.SetCommandStore(commands);
            return gen;
        }

        public static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;

            var commands = new CommandStore();
            var generators = new List<CrossGenerator>
                {
                    CreateGenerator("../../templates/Template1.txt", "../../dict/cz", commands),
                    CreateGenerator("../../templates/Template2.txt", "../../dict/words", commands),
                    CreateGenerator("../../templates/Template3.txt", "../../dict/cz", commands),
                    CreateGenerator("../../templates/Template4.txt", "../../dict/cz", commands),
                   // CreateGenerator("../../templates/american.txt", "../../dict/words", commands),
                    CreateGenerator("../../templates/british.txt", "../../dict/words", commands),
                    CreateGenerator("../../templates/japanese.txt", "../../dict/cz", commands)
                };
            //command reader

            var ri = new ReadInput(commands);

            ThreadStart readMethod = ri.Run;
            var readThread = new Thread(readMethod);
            readThread.Start();

            var tasks = generators.Select(gen1 => Task.Factory.StartNew(gen1.Generate)).ToArray();
            Task.WaitAll(tasks);
            ri.ShouldStop = true;

            TimeSpan timeSpan = DateTime.Now - startTime;
            Console.WriteLine("Time elapsed: {0}", timeSpan);
        }
    }
}