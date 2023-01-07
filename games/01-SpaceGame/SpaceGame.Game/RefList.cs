using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace SpaceGame.Game;

public class RefList<T> : IEnumerable
{
    private T[] _array = null;
    private int _index = 0;
    private int _capacity = 4;

    public RefList(int capacity)
    {
        this._capacity = capacity;
        _array = new T[capacity];
    }

    public RefList()
    {
        _array = new T[_capacity];
    }

    public int Count()
    {
        return _array.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        if (_index >= _array.Length)
        {
            Expand();
        }

        _array[_index++] = value;
    }

    public T Get(int index)
    {
        return _array[index];
    }

    public void Set(int index, T value)
    {
        _array[index] = value;
    }

    public void Expand()
    {
        var newCapacity = _array.Length * 2;

        var newArray = new T[newCapacity];
        Array.Copy(_array, newArray, _array.Length);
        _array = newArray;

        _capacity = newCapacity;
    }

    public T this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    public RefEnumerator GetEnumerator() => new RefEnumerator(_array, _capacity);
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    public struct RefEnumerator
    {
        private readonly T[] _array;
        private readonly int _capacity;
        private int _index;

        public RefEnumerator(T[] target, int capacity)
        {
            _array = target;
            _index = -1;
            _capacity = capacity;
        }

        public ref T Current
        {
            get
            {
                if (_array is null || _index < 0 || _index > _capacity)
                {
                    throw new InvalidOperationException();
                }

                return ref _array[_index];
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext() => ++_index < _capacity;

        public void Reset() => _index = -1;
    }
}