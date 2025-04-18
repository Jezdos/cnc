namespace Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the <see cref="BiDictionary{T1, T2}" />
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class BiDictionary<T1, T2> where T1 : notnull where T2 : notnull
    {
        private readonly Dictionary<T1, T2> _forward = [];
        private readonly Dictionary<T2, T1> _reverse = [];

        public void Add(T1 key1, T2 key2)
        {
            if (_forward.ContainsKey(key1) || _reverse.ContainsKey(key2))
                throw new ArgumentException("Duplicate key detected.");

            _forward.Add(key1, key2);
            _reverse.Add(key2, key1);
        }

        public bool TryGetByFirst(T1 key1, [NotNullWhen(true)] out T2? value2)
        {
            return _forward.TryGetValue(key1, out value2);
        }

        public bool TryGetBySecond(T2 key2, [NotNullWhen(true)] out T1? value1)
        {
            return _reverse.TryGetValue(key2, out value1);
        }

        public bool RemoveByFirst(T1 key1)
        {
            if (_forward.TryGetValue(key1, out var value2))
            {
                _forward.Remove(key1);
                _reverse.Remove(value2);
                return true;
            }
            return false;
        }

        public bool RemoveBySecond(T2 key2)
        {
            if (_reverse.TryGetValue(key2, out var value1))
            {
                _reverse.Remove(key2);
                _forward.Remove(value1);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _forward.Clear();
            _reverse.Clear();
        }
    }
}
