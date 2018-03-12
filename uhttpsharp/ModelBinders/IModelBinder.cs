using uhttpsharp.Headers;

namespace uhttpsharp.ModelBinders
{
    public interface IModelBinder
    {
        /// <summary>
        /// Gets the object from the unparsed body
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="raw"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        T Get<T>(byte[] raw, string prefix);

        /// <summary>
        /// Gets the object from the body of the given headers
        /// </summary>
        /// <param name="headers"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(IHttpHeaders headers);

        /// <summary>
        /// Gets the object using the prefix from the given headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="headers"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        T Get<T>(IHttpHeaders headers, string prefix);

    }
}
