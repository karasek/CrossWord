using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CrossWord
{
    public class Dictionary : object, ICrossDictionary
    {
        readonly WordFilter _filter;
        readonly IList<string>[] _words; //different array list for each word length
        readonly int _maxWordLength;

        public Dictionary(int maxWordLength)
        {
            _maxWordLength = maxWordLength;
            _words = new IList<string>[maxWordLength+1];
            for (int i = 1; i <= maxWordLength; i++)
            {
                _words[i] = new List<string>();
            }

            _filter = new WordFilter(1, maxWordLength);
        }

        public Dictionary(string aFileName, int maxWordLength)
            : this(maxWordLength)
        {
            //read streams
            using (StreamReader reader = File.OpenText(aFileName))
            {
                string str = reader.ReadLine();
                TextInfo ti = new CultureInfo("en-US", false).TextInfo;
                while (str != null)
                {
                    //Console.WriteLine(str);
                    AddWord(ti.ToUpper(str));
                    str = reader.ReadLine();
                }
            }
        }

        public int MaxWordLength
        {
            get { return _maxWordLength; }
        }

        public void AddWord(string aWord)
        {
            if (_filter.Filter(aWord))
            {
                _words[aWord.Length].Add(aWord);
            }
        }

        public int GetWordOfLengthCount(int aLength)
        {
            return _words[aLength].Count;
        }

        public int GetMatchCount(char[] aPattern)
        {
            int count = 0;
            IList<string> words = _words[aPattern.Length];
            foreach(var w in words)
            {
                if (Match(aPattern, w))
                {
                    count++;
                }
            }
            return count;
        }

        public void GetMatch(char[] aPattern, IList<string> matched)
        {
            matched.Clear();
            IList<string> words = _words[aPattern.Length];
            foreach(var w in words)
            {
                if (Match(aPattern, w))
                {
                   matched.Add(w);
                }
            }
        }


        static bool Match(char[] aPattern, string aWord)
        {
            //the length must be correct
            int len = aPattern.Length;
            for (int i = 0; i < len; i++)
            {
                if (aPattern[i] != '.' && aPattern[i] != aWord[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}