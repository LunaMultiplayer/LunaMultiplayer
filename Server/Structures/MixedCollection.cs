using System.Collections.Generic;
using System.Linq;

namespace Server.Structures
{
    public static class Extensions
    {
        public static IEnumerable<MutableKeyValue<K, V>> ToMutableKeyValue<K, V>(this Dictionary<K, V> dict)
        {
            var list = new List<MutableKeyValue<K, V>>();
            foreach (var v in dict)
            {
                list.Add(new MutableKeyValue<K, V>(v.Key, v.Value));
            }

            return list;
        }
    }

    public class MutableKeyValue<T1, T2>
    {
        public T1 Key { get; set; }
        public T2 Value { get; set; }

        public MutableKeyValue(T1 first, T2 second)
        {
            Key = first;
            Value = second;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() == typeof(MutableKeyValue<T1, T2>))
            {
                var castedObj = (MutableKeyValue<T1, T2>)obj;
                return Key.Equals(castedObj.Key) && Value.Equals(castedObj.Value);
            }

            return false;
        }

        protected bool Equals(MutableKeyValue<T1, T2> other)
        {
            return Key.Equals(other.Key) && Value.Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return (EqualityComparer<T1>.Default.GetHashCode(Key) * 397) ^ EqualityComparer<T2>.Default.GetHashCode(Value);
        }

        public static bool operator ==(MutableKeyValue<T1, T2> lhs, MutableKeyValue<T1, T2> rhs)
        {
            if (lhs == null)
            {
                return rhs == null;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(MutableKeyValue<T1, T2> lhs, MutableKeyValue<T1, T2> rhs) => !(lhs == rhs);
    }

    public class MixedCollection<K, V>
    {
        private readonly object _lock = new object();

        private bool _useDictionary = true;
        private Dictionary<K, V> _dictionary = new Dictionary<K, V>();
        private List<MutableKeyValue<K, V>> _list;

        public MixedCollection() { }

        public MixedCollection(IEnumerable<MutableKeyValue<K, V>> collection)
        {
            lock (_lock)
            {
                var keyValuePairs = collection as List<MutableKeyValue<K, V>> ?? collection.ToList();

                _useDictionary = !keyValuePairs.GroupBy(x => x.Key).Any(x => x.Count() > 1);
                if (_useDictionary)
                {
                    _dictionary = keyValuePairs.ToDictionary(k => k.Key, v => v.Value);
                }
                else
                {
                    _list = keyValuePairs.ToList();
                }
            }
        }

        public void Add(K key, V value)
        {
            lock (_lock)
            {
                if (_dictionary.ContainsKey(key))
                {
                    _useDictionary = false;
                    _list = new List<MutableKeyValue<K, V>>(_dictionary.ToMutableKeyValue()) { new MutableKeyValue<K, V>(key, value) };
                    _dictionary = null;
                }
                else
                {
                    _dictionary.Add(key, value);
                }
            }
        }

        public List<V> Get(K key)
        {
            lock (_lock)
            {
                if (_useDictionary)
                {
                    if (_dictionary.ContainsKey(key))
                    {
                        return new List<V> { _dictionary[key] };
                    }
                }
                else
                {
                    return _list.Where(k => k.Key.Equals(key)).Select(v => v.Value).ToList();
                }

                return new List<V>();
            }
        }

        public List<MutableKeyValue<K, V>> GetAll()
        {
            lock (_lock)
            {
                return _useDictionary ? _dictionary.ToMutableKeyValue().ToList() : _list;
            }
        }

        public void Update(K key, V value)
        {
            lock (_lock)
            {
                if (_useDictionary)
                {
                    if (_dictionary.ContainsKey(key))
                    {
                        _dictionary[key] = value;
                    }
                }
                else
                {
                    foreach (var keyVal in _list.Where(k=> k.Key.Equals(key)))
                    {
                        keyVal.Value = value;
                    }
                }
            }
        }

        public void Delete(K key)
        {
            lock (_lock)
            {
                if (_useDictionary)
                {
                    if (_dictionary.ContainsKey(key))
                    {
                        _dictionary.Remove(key);
                    }
                }
                else
                {
                    _list.RemoveAll(v => v.Key.Equals(key));
                }
            }
        }

        public bool Exists(K key)
        {
            lock (_lock)
            {
                return _useDictionary ? _dictionary.ContainsKey(key) : _list.Any(v => v.Key.Equals(key));
            }
        }
    }
}
