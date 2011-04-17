using System;
using System.Collections;
using System.Collections.Generic;

namespace CrossWord
{
    public class WordIndex
    {
        readonly IDictionary<char, ICollection<int>>[] _index;
        readonly int _wordLength;

        public WordIndex(int wordLength)
        {
            _wordLength = wordLength;
            _index = new IDictionary<char, ICollection<int>>[wordLength + 1];
            for (int i = 0; i <= wordLength; i++)
            {
                _index[i] = new Dictionary<char, ICollection<int>>();
            }
        }

        public void IndexWord(string word, int index)
        {
            if (word.Length != _wordLength)
                throw new Exception("Invalid word length");
            for (int i = 0; i < word.Length; i++)
            {
                var dict = _index[i];
                ICollection<int> list;
                if (dict.TryGetValue(word[i], out list))
                {
                    list.Add(index);
                }
                else
                {
                    dict.Add(word[i], new List<int> { index });
                }
            }
        }

        public ICollection<int> GetMatchingIndexes(char[] pattern)
        {
            IList<ICollection<int>> toMerge = new List<ICollection<int>>();
            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (c == '.') continue;
                ICollection<int> list;
                if (!_index[i].TryGetValue(c, out list))
                    return null;
                toMerge.Add(list);
            }
            return Merge(toMerge);
        }

        static ICollection<int> Merge(IList<ICollection<int>> lists)
        {
            if (lists.Count == 1)
                return lists[0];
            var enumerators = new List<IEnumerator<int>>();
            foreach (var list in lists)
            {
                var en = list.GetEnumerator();
                if (!en.MoveNext()) return null;
                enumerators.Add(en);
            }
            var result = new List<int>();
            int max = enumerators[0].Current;
            while (true)
            {
            again:
                foreach (var enumerator in enumerators)
                {
                    while (enumerator.Current < max)
                    {
                        if (!enumerator.MoveNext())
                            return result;
                    }
                    if (enumerator.Current > max)
                    {
                        max = enumerator.Current;
                        goto again;
                    }
                }
                result.Add(max);
                foreach (var enumerator in enumerators)
                {
                    if (!enumerator.MoveNext())
                        return result;
                    if (enumerator.Current > max)
                        max = enumerator.Current;
                }
            }
        }
    }
}
