using System.Collections.Generic;
using System.IO;
using CrossWord;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class CrossTest
    {
        [Test]
        public void CrossBoardPreprocessTest()
        {
            ICrossBoard cb = new CrossBoard();
            cb.SetBoardSize(5, 5);
            for (int i = 0; i < 5; i++)
            {
                var sw = new StartWord();
                sw.StartX = i;
                sw.StartY = 0;
                cb.AddStartWord(sw);
                if (i > 0)
                {
                    sw.StartX = 0;
                    sw.StartY = i;
                    cb.AddStartWord(sw);
                }
                else
                {
                    sw.StartX = 2;
                    sw.StartY = 2;
                    cb.AddStartWord(sw);
                }
            }
            cb.Preprocess(new Dictionary(cb.MaxWordLength));
            Assert.AreEqual(8, cb.GetPatternCount());
        }

        [Test]
        public void DictionaryTest()
        {
            ICrossDictionary dict = new Dictionary(4);
            dict.AddWord("duty");
            dict.AddWord("ruty");
            dict.AddWord("suty");
            dict.AddWord("ab");

            int count = dict.GetMatchCount("..ty".ToCharArray());
            Assert.AreEqual(3, count);

            var al = new List<string>();
            dict.GetMatch("s...".ToCharArray(), al);
            Assert.AreEqual(1, al.Count);
            Assert.AreEqual("suty", al[0]);
        }

        [Test]
        public void FileBoardCreatorTest()
        {
            using (var memoryStream = new MemoryStream())
            {
                var w = new StreamWriter(memoryStream);
                w.WriteLine("--   ");
                w.WriteLine("-    ");
                w.WriteLine("     ");
                w.WriteLine("    -");
                w.WriteLine("   --");
                w.Flush();
                memoryStream.Position = 0;
                var board = CrossBoardCreator.CreateFromStream(memoryStream);
                Assert.True(board != null);
            }
        }
    }
}