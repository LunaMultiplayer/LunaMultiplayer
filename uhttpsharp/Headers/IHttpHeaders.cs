using System.Collections.Generic;

namespace uhttpsharp.Headers
{
    public interface IHttpHeaders : IEnumerable<KeyValuePair<string, string>>
    {

        string GetByName(string name);

        bool TryGetByName(string name, out string value);

    }
}