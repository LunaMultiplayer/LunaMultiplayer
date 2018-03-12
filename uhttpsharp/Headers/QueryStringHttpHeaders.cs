using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace uhttpsharp.Headers
{
    [DebuggerDisplay("{Count} Query String Headers")]
    [DebuggerTypeProxy(typeof(HttpHeadersDebuggerProxy))]
    internal class QueryStringHttpHeaders : IHttpHeaders
    {
        private readonly HttpHeaders _child;
        private static readonly char[] Seperators = {'&', '='};

        private readonly int _count;

        public QueryStringHttpHeaders(string query)
        {
            var splittedKeyValues = query.Split(Seperators, StringSplitOptions.RemoveEmptyEntries);
            var values = new Dictionary<string, string>(splittedKeyValues.Length / 2, StringComparer.InvariantCultureIgnoreCase);

            for (int i = 0; i < splittedKeyValues.Length; i += 2)
            {
                var key = Uri.UnescapeDataString(splittedKeyValues[i]);
                string value = null;
                if (splittedKeyValues.Length > i + 1)
                {
                    value = Uri.UnescapeDataString(splittedKeyValues[i + 1]).Replace('+', ' ');    
                }
                
                values[key] = value;
            }

            _count = values.Count;
            _child = new HttpHeaders(values);
        }

        public string GetByName(string name)
        {
            return _child.GetByName(name);
        }
        public bool TryGetByName(string name, out string value)
        {
            return _child.TryGetByName(name, out value);
        }
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _child.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal int Count
        {
            get
            {
                return _count;
            }
        }
    }
}