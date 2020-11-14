using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossWord
{
    public class SkipList
    {
        readonly IList<int> _list;
        int[]? _array;

        public SkipList()
        {
            _list = new List<int>();
        }

        public int Count => _list.Count;

        public void Add(int val)
        {
            _list.Add(val);
            _array = null;
        }

        int[] GetSkipped()
        {
            return _array ??= _list.ToArray();
        }

        public class SkipListEnumerator
        {
            readonly int[] _values;
            readonly int _step;
            readonly int _count;
            int _index;
            int _current;

            public SkipListEnumerator(SkipList skipList)
            {
                _values = skipList.GetSkipped();
                _step = (int) Math.Sqrt(_values.Length);
                if (_step < 2) _step = 2;
                _count = _values.Length;
                _index = 0;
                _current = _values[0];
            }

            public bool MoveNext()
            {
                if (_index < _values.Length)
                {
                    _current = _values[_index];
                    _index++;
                    return true;
                }

                return false;
            }

            public bool MoveNextGreaterOrEqual(int value)
            {
                var pos = _index + _step;
                while (pos < _count && _values[pos] <= value)
                {
                    _index = pos;
                    pos += _step;
                }

                while (_index < _count)
                {
                    var val = _values[_index];
                    if (val >= value)
                    {
                        _current = val;
                        return true;
                    }

                    _index++;
                }

                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public int Current => _current;

        }

        public SkipListEnumerator GetSkipEnumerator()
        {
            return new SkipListEnumerator(this);
        }

        public ICollection<int> All()
        {
            return _list;
        }
    }

}
