using System;
using System.Collections.Generic;

namespace CrossWord
{
    public class WordIndex
    {
        readonly IDictionary<char, SkipList>[] _index;
        readonly int _wordLength;

        public WordIndex(int wordLength)
        {
            _wordLength = wordLength;
            _index = new IDictionary<char, SkipList>[wordLength + 1];
            for (int i = 0; i <= wordLength; i++)
            {
                _index[i] = new Dictionary<char, SkipList>();
            }
        }

        public void IndexWord(string word, int index)
        {
            if (word.Length != _wordLength)
                throw new Exception("Invalid word length");
            for (int i = 0; i < word.Length; i++)
            {
                var dict = _index[i];
                SkipList list;
                if (dict.TryGetValue(word[i], out list))
                {
                    list.Add(index);
                }
                else
                {
                    list = new SkipList();
                    dict.Add(word[i], list);
                    list.Add(index);
                }
            }
        }

        public ICollection<int> GetMatchingIndexes(char[] pattern)
        {
            IList<SkipList> toMerge = new List<SkipList>();
            for (int i = 0; i < pattern.Length; i++)
            {
                char c = pattern[i];
                if (c == '.') continue;
                SkipList list;
                if (!_index[i].TryGetValue(c, out list))
                    return null;
                toMerge.Add(list);
            }
            if (toMerge.Count == 0) return null;
            return Merge(toMerge);
        }

        static ICollection<int> Merge(IList<SkipList> lists)
        {
            if (lists.Count == 1)
                return lists[0].All();
            var enumerators = new List<SkipList.SkipListEnumerator>();
            foreach (var list in lists)
            {
                var en = list.GetSkipEnumerator();
                enumerators.Add(en);
            }
            var result = new List<int>();
            int max = -1;
            while (true)
            {
            again:
                foreach (var enumerator in enumerators)
                {
                    if (! enumerator.MoveNextGreaterOrEqual(max))
                    {
                        return result;
                    }
                    if (enumerator.Current > max)
                    {
                        max = enumerator.Current;
                        goto again;
                    }
                }
                result.Add(max);
                if (enumerators[0].MoveNext())
                    max = enumerators[0].Current;
                else
                    return result;
            }
        }
    }
}
