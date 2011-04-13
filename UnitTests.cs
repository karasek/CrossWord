using System.Collections.Generic;
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
            cb.Preprocess(new Dictionary());
            Assert.AreEqual(10, cb.GetPatternCount());
        }

        [Test]
        public void DictionaryTest()
        {
            ICrossDictionary dict = new Dictionary();
            dict.AddWord("duty");
            dict.AddWord("ruty");
            dict.AddWord("suty");

            int count = dict.GetMatchCount("..ty".ToCharArray());
            Assert.AreEqual(3, count);

            var al = new List<string>();
            dict.GetMatch("s...".ToCharArray(), al);
            Assert.AreEqual(1, al.Count);
            Assert.AreEqual("suty", al[0]);
        }
    }
}