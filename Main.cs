using System;
using System.Threading;

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

        public static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;
            //create dictionary
            var dict = new Dictionary("../../words");
            //prepare cross board
            ICrossBoard cb = new CrossBoard();
            CreateCross(cb);

            cb.Preprocess(dict);

            CrossPattern cp = cb.GetCrossPattern(32);
            CrossTransformation trans = cp.TryFill("ADELAVOJTAHELA", dict); //length 14
            trans.Transform(cp);

            //command reader
            var commandStore = new CommandStore();
            var ri = new ReadInput(commandStore);

            ThreadStart readMethod = ri.Run;
            var readThread = new Thread(readMethod);
            readThread.Start();


            //create generator
            var generator = new CrossGenerator(dict, cb);
            generator.SetCommandStore(commandStore);
            generator.Generate();

            ri.ShouldStop = true;

            TimeSpan timeSpan = DateTime.Now - startTime;
            Console.WriteLine("Time elapsed: {0}", timeSpan);
        }
    }
}