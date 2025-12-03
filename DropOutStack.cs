using System;
using System.Collections;
using System.Collections.Generic;

namespace InGameUIDesigner
{
    public class DropOutStack<T> : ICollection, IEnumerable<T> , IEnumerable, ICloneable
    {
        public DropOutStack(int initialCapacity)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException("initialCapacity", "Capacity must be non-negative");
            }
            _array = new T[initialCapacity];
            _actualSize = 0;
            _topIndex = 0;
            _bottomIndex = 0;
        }
        public DropOutStack(ICollection<T> col) : this((col == null) ? 32 : col.Count)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }
            foreach (T obj in col)
            {
                Push(obj);
            }
        }

        public virtual int Count
        {
            get
            {
                return _actualSize;
            }
        }

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public virtual void Clear()
        {
            Array.Clear(_array, 0, _array.Length);
            _actualSize = 0;
            _topIndex = 0;
            _bottomIndex = 0;
        }

        public virtual object Clone()
        {
            DropOutStack<T> stack = new DropOutStack<T>(_array.Length);
            stack._topIndex = _topIndex;
            stack._bottomIndex = _bottomIndex;
            stack._actualSize = _actualSize;
            Array.Copy(_array, stack._array, _topIndex);
            return stack;
        }

        public virtual bool Contains(T obj)
        {
            int size = _actualSize;
            while (size-- > 0)
            {
                if (obj == null)
                {
                    if (_array[size] == null)
                    {
                        return true;
                    }
                }
                else if (_array[size] != null && _array[size].Equals(obj))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException("Multi-dimensional arrays not supported", "array");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "Index must be non-negative");
            }
            if (array.Length - index < _actualSize)
            {
                throw new ArgumentException("Offset if larger than DropOutStack size");
            }
            int i = 0;
            T[] array2 = array as T[];
            if (array2 != null)
            {
                while (i < _actualSize)
                {
                    array2[i + index] = _array[_actualSize - i - 1];
                    i++;
                }
                return;
            }
            while (i < _actualSize)
            {
                array.SetValue(_array[_actualSize - i - 1], i + index);
                i++;
            }
        }

        public virtual T Peek()
        {
            if (_actualSize == 0)
            {
                throw new InvalidOperationException("DropOutStack is empty");
            }
            var peekIndex = _topIndex - 1;
            if (peekIndex < 0) peekIndex = _array.Length - 1;
            return _array[peekIndex];
        }
        public virtual T PeekBottom()
        {
            if (_actualSize == 0)
            {
                throw new InvalidOperationException("DropOutStack is empty");
            }
            return _array[_bottomIndex];
        }

        public virtual T Pop()
        {
            if (_actualSize == 0)
            {
                throw new InvalidOperationException("DropOutStack is empty");
            }
            _topIndex--;
            if (_topIndex < 0) _topIndex = _array.Length - 1;
            var result = _array[_topIndex];
            _array[_topIndex] = default(T); 
            _actualSize--; 
            return result;
        }

        public virtual T Push(T obj)
        {
            var oldItem = _array[_topIndex];
            _array[_topIndex] = obj;
            if (_topIndex == _bottomIndex && _actualSize != 0) _bottomIndex++;
            if (_bottomIndex >= _array.Length) _bottomIndex = 0;
            _topIndex++;
            if (_topIndex >= _array.Length) _topIndex = 0;
            _actualSize++;
            if (_actualSize > _array.Length) _actualSize = _array.Length;
            return oldItem;
        }

        public virtual T[] ToArray()
        {
            if (_actualSize == 0)
            {
                return Array.Empty<T>();
            }
            T[] array = new T[_actualSize];
            for (int i = 0; i < _actualSize; i++)
            {
                array[i] = _array[_actualSize - i - 1];
            }
            return array;
        }

        public DropOutStackEnumerator GetEnumerator()
        {
            return new DropOutStackEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DropOutStackEnumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new DropOutStackEnumerator(this);
        }

        private T[] _array;
        private int _topIndex;
        private int _bottomIndex;
        private int _actualSize;

        public struct DropOutStackEnumerator : IEnumerator<T>, ICloneable, IDisposable, IEnumerator
        {
            // Token: 0x06000122 RID: 290 RVA: 0x000068D8 File Offset: 0x000054D8
            internal DropOutStackEnumerator(DropOutStack<T> stack)
            {
                _stack = stack;
                _index = -2;
                _currentElement = default(T);
            }
            public object Clone()
            {
                return MemberwiseClone();
            }

            public bool MoveNext()
            {
                if (_index == -2)
                {
                    _index = _stack._actualSize - 1;
                    if (_index >= 0)
                    {
                        _currentElement = _stack._array[_index];
                    }
                    return _index >= 0;
                }
                if (_index == -1)
                {
                    return false;
                }
                _index--;
                if (_index >= 0)
                {
                    _currentElement = _stack._array[_index];
                }
                else
                {
                    _currentElement = default(T);
                }
                return _index >= 0;
            }

            public T Current
            {
                get
                {
                    if (_index == -2)
                    {
                        throw new InvalidOperationException("Enumeration not yet started");
                    }
                    if (_index == -1)
                    {
                        throw new InvalidOperationException("Enumeration ended");
                    }
                    return _currentElement;
                }
            }

            object IEnumerator.Current => Current;

            public void Reset()
            {
                _index = -2;
                _currentElement = default(T);
            }

            private readonly DropOutStack<T> _stack;
            private int _index;
            private T _currentElement;
            public void Dispose()
            {
                _index = -1;
            }
        }
    }
}
