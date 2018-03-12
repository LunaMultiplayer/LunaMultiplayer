using System;
using System.Threading.Tasks;
using uhttpsharp.Handlers;

namespace uhttpsharp.Controllers
{
    /// <summary>
    /// Represents a series of functions and an entry point in which a function can be called at.
    /// </summary>

    public interface IPipeline
    {
        /// <summary>
        /// Starts the pipeline.
        /// An empty pipeline implementation will just return the task of the injected task function.
        /// </summary>
        /// <param name="injectedTask">The function to be called at the appropriate stage.</param>
        /// <param name="context">The context in which this task will run (in case you need to check something before the actual task runs).</param>
        /// <returns></returns>
        Task<IControllerResponse> Go(Func<Task<IControllerResponse>> injectedTask, IHttpContext context);
    }
}