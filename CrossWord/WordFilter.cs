namespace CrossWord;

public class WordFilter : object
{
    readonly char[] _forbiddenChars;
    readonly int _maxLength;
    readonly int _minLength;

    public WordFilter(int aMinLength, int aMaxLength)
    {
        _minLength = aMinLength;
        _maxLength = aMaxLength;

        _forbiddenChars = new[] { '\'', ',' };
    }

    public bool Filter(string aWord)
    {
        if (aWord.Length < _minLength || aWord.Length > _maxLength) return false;
        return aWord.IndexOfAny(_forbiddenChars) == -1;
    }
}
