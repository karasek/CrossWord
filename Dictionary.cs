using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CrossWord
{
    public class Dictionary : object, ICrossDictionary
    {
        readonly WordFilter _filter;
        readonly IList<string>[] _words; //different array list for each word length
        readonly WordIndex[] _indexes;
        readonly int _maxWordLength;

        public Dictionary(int maxWordLength)
        {
            _maxWordLength = maxWordLength;
            _words = new List<string>[maxWordLength + 1];
            for (int i = 1; i <= maxWordLength; i++)
            {
                _words[i] = new List<string>();
            }
            _indexes = new WordIndex[maxWordLength + 1];
            for (int i = 1; i <= maxWordLength; i++)
            {
                _indexes[i] = new WordIndex(i);
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
            if (!_filter.Filter(aWord)) return;
            _indexes[aWord.Length].IndexWord(aWord, _words[aWord.Length].Count);
            _words[aWord.Length].Add(aWord);
        }

        public int GetWordOfLengthCount(int aLength)
        {
            return _words[aLength].Count;
        }

        bool IsEmptyPattern(char[] aPattern)
        {
            if (aPattern==null) return true;
            foreach (var c in aPattern)
            {
                if (c!='.') return false;
            }
            return true;
        }

        public int GetMatchCount(char[] aPattern)
        {
            if (IsEmptyPattern(aPattern))
                return _words[aPattern.Length].Count;
            var indexes = _indexes[aPattern.Length].GetMatchingIndexes(aPattern);
            return indexes != null ? indexes.Count : 0;
        }

        public void GetMatch(char[] aPattern, List<string> matched)
        {
            if (IsEmptyPattern(aPattern))
            {
                matched.AddRange(_words[aPattern.Length]);
                return;
            }
            var indexes = _indexes[aPattern.Length].GetMatchingIndexes(aPattern);
            if (indexes == null) return;
            foreach (var idx in indexes)
            {
                matched.Add(_words[aPattern.Length][idx]);
            }
        }
    }
}