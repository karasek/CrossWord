using System;
using System.Collections;
using System.Collections.Generic;

namespace CrossWord
{
    public class SkipList : IEnumerable<int>
    {
        readonly IList<int> _list;

        public SkipList()
        {
            _list = new List<int>();
        }

        public void Add(int val)
        {
            _list.Add(val);
        }

        public class SkipListEnumerator : IEnumerator<int>
        {
            readonly IList<int> _list;
            readonly int _step;
            readonly int _count;
            readonly int _maxValue;
            int _index;
            int _current;

            public SkipListEnumerator(SkipList skipList)
            {
                _list = skipList._list;
                _step = (int) Math.Sqrt(_list.Count);
                if (_step < 2) _step = 2;
                _count = _list.Count;
                _maxValue = _list[^1];
            }

            public bool MoveNext()
            {
                if (_index < _list.Count)
                {
                    _current = _list[_index];
                    _index++;
                    return true;
                }

                return false;
            }

            public bool MoveNextGreaterOrEqual(int value)
            {
                if (value > _maxValue || _index >= _count) return false;
                while (_index + _step < _count && _list[_index + _step] <= value)
                    _index += _step;
                while (_index < _count)
                {
                    if (_list[_index] >= value)
                    {
                        _current = _list[_index];
                        return true;
                    }

                    _index++;
                }

                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default(int);
            }

            public int Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
            }
        }

        public IEnumerator<int> GetEnumerator()
        {
            return GetSkipEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
