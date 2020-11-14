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
                if (dict.TryGetValue(word[i], out var list))
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

        struct SkipListId
        {
            public SkipListId(int index, char c)
            {
                Index = index;
                Char = c;
            }

            public int Index;
            public char Char;
        }

        public int GetMatchingIndexCount(ReadOnlySpan<char> pattern)
        {
            unsafe
            {
                var toMerge = stackalloc SkipListId[pattern.Length];
                var toMergeCount = 0;
                for (int i = 0; i < pattern.Length; i++)
                {
                    char c = pattern[i];
                    if (c == '.') continue;
                    if (!_index[i].ContainsKey(c))
                        return 0;
                    toMerge[toMergeCount++] = new SkipListId(i, c);
                }

                return MergedCount(toMerge, toMergeCount);
            }
        }

        unsafe int MergedCount(SkipListId* toMerge, in int toMergeCount)
        {
            if (toMergeCount == 1)
                return _index[toMerge->Index][toMerge->Char].Count;
            var enumerators = new List<SkipList.SkipListEnumerator>();
            for (int i = 0; i < toMergeCount; i++)
            {
                var list = _index[toMerge[i].Index][toMerge[i].Char];
                var en = list.GetSkipEnumerator();
                enumerators.Add(en);
            }

            var result = 0;
            int max = -1;
            while (true)
            {
                again:
                foreach (var enumerator in enumerators)
                {
                    if (!enumerator.MoveNextGreaterOrEqual(max))
                    {
                        return result;
                    }

                    if (enumerator.Current > max)
                    {
                        max = enumerator.Current;
                        goto again;
                    }
                }

                result++;
                if (enumerators[0].MoveNext())
                    max = enumerators[0].Current;
                else
                    return result;
            }
        }


        public ICollection<int>? AddMatched(ReadOnlySpan<char> pattern)
        {
            unsafe
            {
                var toMerge = stackalloc SkipListId[pattern.Length];
                var toMergeCount = 0;
                for (int i = 0; i < pattern.Length; i++)
                {
                    char c = pattern[i];
                    if (c == '.') continue;
                    if (!_index[i].TryGetValue(c, out var list))
                        return null;
                    toMerge[toMergeCount++] = new SkipListId(i, c);
                }

                if (toMergeCount == 0) return null;
                return Merge(toMerge, toMergeCount);
            }
        }

        unsafe ICollection<int> Merge(SkipListId* toMerge, in int toMergeCount)
        {
            if (toMergeCount == 1)
                return _index[toMerge->Index][toMerge->Char].All();
            var enumerators = new List<SkipList.SkipListEnumerator>();
            for (int i = 0; i < toMergeCount; i++)
            {
                var list = _index[toMerge[i].Index][toMerge[i].Char];
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
                    if (!enumerator.MoveNextGreaterOrEqual(max))
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
