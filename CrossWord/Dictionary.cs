using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace CrossWord;

public class Dictionary : ICrossDictionary
{
    readonly WordFilter _filter;
    readonly List<string>[] _words; //different array list for each word length
    readonly WordIndex[] _indexes;
    readonly Dictionary<string, string> _description;

    public Dictionary(int maxWordLength)
    {
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
        _description = new Dictionary<string, string>();
    }

    public Dictionary(string aFileName, int maxWordLength)
        : this(maxWordLength)
    {
        //read streams
        using var reader = File.OpenText(aFileName);
        var str = reader.ReadLine();
        var ti = new CultureInfo("en-US").TextInfo;
        while (str != null)
        {
            int pos = str.IndexOf('|');
            if (pos == -1)
            {
                AddWord(ti.ToUpper(str));
            }
            else
            {
                var word = str.Substring(0, pos);
                AddWord(word);
                AddDescription(word, str.Substring(pos + 1));
            }

            str = reader.ReadLine();
        }
    }

    public void AddDescription(string word, string description)
    {
        _description[word] = description;
    }

    public bool TryGetDescription(string word, out string? description)
    {
        return _description.TryGetValue(word, out description);
    }

    public void AddWord(string word)
    {
        if (!_filter.Filter(word)) return;
        _indexes[word.Length].IndexWord(word, _words[word.Length].Count);
        _words[word.Length].Add(word);
    }

    public int GetWordOfLengthCount(int length)
    {
        return _words[length].Count;
    }

    static bool IsEmptyPattern(ReadOnlySpan<char> pattern)
    {
        for (var i = 0; i < pattern.Length; i++)
            if (pattern[i] != '.')
                return false;
        return true;
    }

    public int GetMatchCount(ReadOnlySpan<char> pattern)
    {
        if (IsEmptyPattern(pattern))
            return _words[pattern.Length].Count;
        return _indexes[pattern.Length].GetMatchingIndexCount(pattern);
    }

    public void GetMatch(ReadOnlySpan<char> pattern, List<string> matched)
    {
        if (IsEmptyPattern(pattern))
        {
            matched.AddRange(_words[pattern.Length]);
            return;
        }

        var indexes = _indexes[pattern.Length].AddMatched(pattern);
        foreach (var idx in indexes)
        {
            matched.Add(_words[pattern.Length][idx]);
        }
    }
}
