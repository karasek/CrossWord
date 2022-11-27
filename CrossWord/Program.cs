using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Runtime.CompilerServices;

namespace CrossWord;

static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var fileBoardOption = new Option<FileInfo>(name: "--input", description: "Input file with board template",
            isDefault: true, parseArgument: ParseExistingFile);
        var fileDictionaryOption = new Option<FileInfo>(name: "--dictionary", description: "Dictionary file",
            isDefault: true, parseArgument: ParseExistingFile);
        var fileOutputOption = new Option<FileInfo>(name: "--output", description: "Output file");
        var puzzleOption = new Option<string?>(name: "--puzzle", description: "Puzzle");

        var rootCommand = new RootCommand("Puzzle generator app");
        rootCommand.AddOption(fileBoardOption);
        rootCommand.AddOption(fileDictionaryOption);
        rootCommand.AddOption(fileOutputOption);
        rootCommand.AddOption(puzzleOption);

        int returnCode = 0;

        rootCommand.SetHandler(async (boardFile, dictionaryFile, outputFile, puzzle) =>
        {
            returnCode = await Generate(boardFile, dictionaryFile, outputFile, puzzle);
        }, fileBoardOption, fileDictionaryOption, fileOutputOption, puzzleOption);

        await rootCommand.InvokeAsync(args);
        return returnCode;
    }

    static FileInfo ParseExistingFile(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            result.ErrorMessage = $"Missing argument for {result.Argument.Name}";
            return new ("error");
        }
        string? filePath = result.Tokens.Single().Value;
        if (!File.Exists(filePath))
        {
            result.ErrorMessage = "File does not exist";
            return new ("error");
        }
        return new (filePath);
    }

    static async Task<int> Generate(FileInfo boardFile, FileInfo dictionaryFile, FileInfo outputFile, string? puzzle)
    {
        ICrossBoard board;
        try
        {
            board = await CrossBoardCreator.CreateFromFileAsync(boardFile.FullName);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Cannot load crossword layout from file {boardFile}.", e);
            return 2;
        }

        Dictionary dictionary;
        try
        {
            dictionary = new Dictionary(dictionaryFile.FullName, board.MaxWordLength);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Cannot load dictionary from file {dictionaryFile}.", e);
            return 3;
        }

        ICrossBoard? resultBoard;
        try
        {
            resultBoard = puzzle != null
                ? GenerateFirstCrossWord(board, dictionary, puzzle)
                : GenerateFirstCrossWord(board, dictionary);
        }
        catch (Exception e)
        {
            Console.WriteLine("Generating crossword has failed.", e);
            return 4;
        }

        if (resultBoard == null)
        {
            Console.WriteLine("No solution has been found.");
            return 5;
        }

        try
        {
            SaveResultToFile(outputFile.FullName, resultBoard, dictionary);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Saving result crossword to file {outputFile} has failed: {e.Message}.");
            return 6;
        }

        return 0;
    }

    static ICrossBoard? GenerateFirstCrossWord(ICrossBoard board, ICrossDictionary dictionary)
    {
        var gen = new CrossGenerator(dictionary, board);
        board.Preprocess(dictionary);
        return gen.Generate().FirstOrDefault();
    }

    static ICrossBoard GenerateFirstCrossWord(ICrossBoard board, ICrossDictionary dictionary, string puzzle)
    {
        var placer = new PuzzlePlacer(board, puzzle);
        var cts = new CancellationTokenSource();
        var mre = new ManualResetEvent(false);
        ICrossBoard? successFullBoard = null;
        foreach (var boardWithPuzzle in placer.GetAllPossiblePlacements(dictionary))
        {
            //boardWithPuzzle.WriteTo(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });
            var gen = new CrossGenerator(dictionary, boardWithPuzzle);
            var t = Task.Factory.StartNew(() =>
            {
                foreach (var solution in gen.Generate())
                {
                    successFullBoard = solution;
                    cts.Cancel();
                    mre.Set();
                    break; //interested in the first one
                }
            }, cts.Token);
            if (cts.IsCancellationRequested)
                break;
        }

        mre.WaitOne();
        return successFullBoard!;
    }

    static void SaveResultToFile(string outputFile, ICrossBoard resultBoard, ICrossDictionary dictionary)
    {
        Console.WriteLine($"Solution has been writen to file {outputFile}.");
        using var writer = new StreamWriter(new FileStream(outputFile, FileMode.Create));
        resultBoard.WriteTo(writer);
        resultBoard.WritePatternsTo(writer, dictionary);
    }
}
