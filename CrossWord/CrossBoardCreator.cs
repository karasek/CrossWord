using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CrossWord;

public static class CrossBoardCreator
{
    public static async Task<ICrossBoard> CreateFromFileAsync(string path)
    {
        await using var fs = File.OpenRead(path);
        return await CreateFromStreamAsync(fs);
    }

    public static async Task<ICrossBoard> CreateFromStreamAsync(Stream s)
    {
        var r = new StreamReader(s, Encoding.UTF8);
        var lines = new List<string>();
        while (true)
        {
            var line = await r.ReadLineAsync();
            if (string.IsNullOrEmpty(line)) break;
            lines.Add(line);
        }

        int lineLength = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lineLength == -1)
                lineLength = lines[i].Length;
            else if (lines[i].Length != lineLength)
                throw new ($"Line {i} has different length ({lines[i]}) then previous lines ({lineLength})");
        }

        ICrossBoard board = new CrossBoard(lineLength, lines.Count);
        for (int row = 0; row < lines.Count; row++)
        {
            var line = lines[row];
            for (int col = 0; col < lineLength; col++)
            {
                if (line[col] == '-')
                    board.AddStartWord(col, row);
            }
        }

        return board;
    }
}
