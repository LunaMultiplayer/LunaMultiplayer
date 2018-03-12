using System.Threading.Tasks;

namespace uhttpsharp.Handlers.Compression
{

    /// <summary>
    /// Represents an object that can compress <see cref="IHttpResponse"/>s.
    /// </summary>
    public interface ICompressor
    {

        /// <summary>
        /// The name of the compression algorithm
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Compresses the given <paramref name="response"/>
        /// </summary>
        /// <param name="response"></param>
        /// <returns>The compressed response</returns>
        Task<IHttpResponse> Compress(IHttpResponse response);

    }
}