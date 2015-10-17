using System.Collections.Generic;

namespace CrossWord
{
    public interface ICrossDictionary
    {
        void AddWord(string word);
        int GetWordOfLengthCount(int length);
        int GetMatchCount(char[] pattern);
        void GetMatch(char[] pattern, List<string> matched);
        int MaxWordLength { get; }
        void AddDescription(string word, string description);
        bool TryGetDescription(string word, out string description);
    }
}