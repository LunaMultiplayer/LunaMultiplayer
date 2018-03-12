using System;
using System.Collections.Generic;
using System.Linq;

namespace uhttpsharp.Headers
{
    /// <summary>
    /// A trivial implementation of <see cref="IHttpHeaders"/>
    /// that is composed from multiple <see cref="IHttpHeaders"/>.
    /// 
    /// If value is found in more then one header,
    /// Gets the first available value from by the order of the headers
    /// given in the c'tor.
    /// </summary>
    public class CompositeHttpHeaders : IHttpHeaders
    {
        private static readonly IEqualityComparer<KeyValuePair<string, string>> HeaderComparer = 
            new KeyValueComparer<string, string, string>(k => k.Key, StringComparer.InvariantCultureIgnoreCase);
        
        private readonly IEnumerable<IHttpHeaders> _children;

        public CompositeHttpHeaders(IEnumerable<IHttpHeaders> children)
        {
            _children = children;
        }

        public CompositeHttpHeaders(params IHttpHeaders[] children) 
        {
            _children = children;
        }

        public string GetByName(string name)
        {
            foreach (var child in _children)
            {
                string value;
                if (child.TryGetByName(name, out value))
                {
                    return value;
                }
            }

            throw new KeyNotFoundException(string.Format("Header {0} was not found in any of the children headers.", name));
        }

        public bool TryGetByName(string name, out string value)
        {
            foreach (var child in _children)
            {
                if (child.TryGetByName(name, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _children.SelectMany(c => c).Distinct(HeaderComparer).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class KeyValueComparer<TKey, TValue, TOutput> : IEqualityComparer<KeyValuePair<TKey, TValue>>
    {
        private readonly Func<KeyValuePair<TKey, TValue>, TOutput> _outputFunc;
        private readonly IEqualityComparer<TOutput> _outputComparer;
        public KeyValueComparer(Func<KeyValuePair<TKey, TValue>, TOutput> outputFunc, IEqualityComparer<TOutput> outputComparer)
        {
            _outputFunc = outputFunc;
            _outputComparer = outputComparer;
        }

        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return _outputComparer.Equals(_outputFunc(x), _outputFunc(y));
        }

        public int GetHashCode(KeyValuePair<TKey, TValue> obj)
        {
            return _outputComparer.GetHashCode(_outputFunc(obj));
        }

    }
}
