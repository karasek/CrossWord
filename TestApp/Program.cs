using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrossWord.TestApp;

class Program
{
    readonly CommandStore _commandStore;

    public Program()
    {
        _commandStore = new CommandStore();
    }

    async Task<CrossGenerator> CreateGeneratorAsync(string file, string dictFile, CommandStore commands)
    {
        DateTime startTime = DateTime.Now;
        var cb = await CrossBoardCreator.CreateFromFileAsync(file);
        var dict = new Dictionary(dictFile, cb.MaxWordLength);
        cb.Preprocess(dict);
        var gen = new CrossGenerator(dict, cb);
        gen.Watcher += GeneratorWatcher;
        return gen;
    }

    StreamWriter OpenConsoleWriter()
    {
        var w = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        return w;
    }

    void GeneratorWatcher(CrossGenerator generator)
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

    void GenerateAndOutput(CrossGenerator generator, CommandStore commands, int maxSolutionsCount)
    {
        int solutionsCount = 0;
        foreach (var solution in generator.Generate())
        {
            // lock (commands.Lock)
            // {
            //     Console.WriteLine($"Solution {solutionsCount} found:");
            //     using (var w = OpenConsoleWriter())
            //         solution.WriteTo(w);
            // }

            if (++solutionsCount == maxSolutionsCount)
            {
                Console.WriteLine($"{solutionsCount} solutions found.");
                break;
            }
        }

        if (solutionsCount == 0)
            Console.WriteLine("Solution not found:");
    }

    async Task RunAsync()
    {
        Console.WriteLine("Starting");
        DateTime startTime = DateTime.Now;

        var generators = new List<CrossGenerator>
            {
                await CreateGeneratorAsync("../templates/template1.txt", "../dict/cz", _commandStore),
                await CreateGeneratorAsync("../templates/template2.txt", "../dict/words", _commandStore),
                await CreateGeneratorAsync("../templates/template3.txt", "../dict/words", _commandStore),
                await CreateGeneratorAsync("../templates/template4.txt", "../dict/cz", _commandStore),
                await CreateGeneratorAsync("../templates/american.txt", "../dict/words", _commandStore),
                await CreateGeneratorAsync("../templates/british.txt", "../dict/words", _commandStore),
                await CreateGeneratorAsync("../templates/japanese.txt", "../dict/words", _commandStore)
            };
        //command reader
        const int maxSolutionsCount = 10000;
        var ri = new ReadInput(_commandStore);
        var t = Task.Run(() => ri.Run());

        var tasks =
            generators.Select(gen1 => Task.Factory.StartNew(() =>
                GenerateAndOutput(gen1, _commandStore, maxSolutionsCount))).ToArray();
        Task.WaitAll(tasks);
        ri.ShouldStop = true;

        TimeSpan timeSpan = DateTime.Now - startTime;
        Console.WriteLine("Time elapsed: {0}", timeSpan);
    }


    public static async Task Main(string[] args)
    {
        await new Program().RunAsync();
    }
}
