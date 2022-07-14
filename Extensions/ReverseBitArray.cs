using System.Collections;

namespace Gnutella;

public class ReverseBitArrayEnumerator : IEnumerator {
    private readonly ReverseBitArray _bitArray;
    private int _index = -1;
    public ReverseBitArrayEnumerator(ReverseBitArray instance) {
        _bitArray = instance;
    }

    public bool MoveNext() {
        if (_index < _bitArray._bitArray.Length)
            _index++;

        return _index < _bitArray._bitArray.Length;
    }

    public void Reset() {
        _index = -1;
    }

    public object Current => _bitArray[_index];
}

public class ReverseBitArray : IEnumerable {
    internal readonly BitArray _bitArray;
    public ReverseBitArray(BitArray bitArray) {
        _bitArray = bitArray;
    }

    public ReverseBitArray(byte[] bytes) {
        _bitArray = new BitArray(bytes);
    }

    public ReverseBitArray(bool[] bits) {
        _bitArray = new BitArray(bits);
    }


    public int Toint() {
        var len = Math.Min(32, _bitArray.Count);
        int n = 0;
        for (int i = 0; i < len; i++) {
            if (this[i])
                n |= 1 << i;
        }
        return n;
    }

    public bool[] this[Range range] {
        get {
            if (range.Start.Value == range.End.Value)
                return new[] { this[range.Start.Value] };

            var result = new bool[range.End.Value - range.Start.Value];
            int currentIndex = range.Start.Value;
            do {
                result[currentIndex - range.Start.Value] = this[currentIndex];
                currentIndex++;
            } while (currentIndex != range.End.Value);

            return result;
        }
    }

    public bool this[int index] {
        get => _bitArray.Get(_bitArray.Length - (index + 1));
        set => _bitArray.Set(index, value);
    }

    public IEnumerator GetEnumerator() {
        return new ReverseBitArrayEnumerator(this);
    }

}