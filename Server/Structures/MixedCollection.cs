using System.Collections.Generic;
using System.Linq;

namespace Server.Structures
{
    public class MixedCollection<K, V>
    {
        private readonly object _lock = new object();

        private bool _useDictionary = true;
        private Dictionary<K, V> _dictionary = new Dictionary<K, V>();
        private List<KeyValuePair<K, V>> _list;

        public MixedCollection()
        {

        }

        public MixedCollection(IEnumerable<KeyValuePair<K,V>> collection)
        {
            lock (_lock)
            {
                var keyValuePairs = collection as List<KeyValuePair<K, V>> ?? collection.ToList();

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
                    _list = new List<KeyValuePair<K, V>>(_dictionary) { new KeyValuePair<K, V>(key, value) };
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
                    for (var i = 0; i < _list.Count; i++)
                    {
                        if (_list[i].Key.Equals(key))
                        {
                            _list[i] = new KeyValuePair<K, V>(_list[i].Key, value);
                        }
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
