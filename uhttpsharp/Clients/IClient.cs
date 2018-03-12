using System.IO;
using System.Net;

namespace uhttpsharp.Clients
{
    public interface IClient
    {

        Stream Stream { get; }

        bool Connected { get; }

        void Close();

        EndPoint RemoteEndPoint { get; }



    }
}
