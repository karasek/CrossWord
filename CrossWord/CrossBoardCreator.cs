using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrossWord
{
    static public class CrossBoardCreator
    {
        public static ICrossBoard CreateFromFile(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return CreateFromStream(fs);
            }
        }

        public static ICrossBoard CreateFromStream(Stream s)
        {
            var r = new StreamReader(s, Encoding.UTF8);
            var lines = new List<string>();
            while (true)
            {
                var line = r.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                lines.Add(line);
            }
            int lineLength = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lineLength == -1)
                    lineLength = lines[i].Length;
                else if (lines[i].Length != lineLength)
                    throw new Exception(string.Format("Line {0} has different length ({1}) then previous lines ({2})",
                        i, lines[i], lineLength));
            }
            var board = new CrossBoard();
            board.SetBoardSize(lineLength, lines.Count);
            for (int row = 0; row < lines.Count; row++)
            {
                var line = lines[row];
                for (int col = 0; col < lineLength; col++)
                {
                    if (line[col]=='-')
                        board.AddStartWord(col, row);
                }
            }
            return board;
        }
    }
}
