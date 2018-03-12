using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace uhttpsharp.Headers
{
    internal class HttpHeadersDebuggerProxy
    {
        private readonly IHttpHeaders _real;
        
        [DebuggerDisplay("{Value,nq}", Name = "{Key,nq}")]
        internal class HttpHeader
        {
            private readonly KeyValuePair<string, string> _header;
            public HttpHeader(KeyValuePair<string, string> header)
            {
                _header = header;
            }

            public string Value
            {
                get
                {
                    return _header.Value;
                }
            }

            public string Key
            {
                get
                {
                    return _header.Key;
                }
            }
        }

        public HttpHeadersDebuggerProxy(IHttpHeaders real)
        {
            _real = real;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public HttpHeader[] Headers
        {
            get
            {
                return _real.Select(kvp => new HttpHeader(kvp)).ToArray();
            }
        }

    }
}