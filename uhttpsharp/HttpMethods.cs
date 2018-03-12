namespace uhttpsharp
{
    /// <summary>
    /// Specifies the methods of an http request.
    /// </summary>
    public enum HttpMethods
    {
        /// <summary>
        /// Requests a representation of the specified resource.
        /// </summary>
        Get,

        /// <summary>
        /// Asks for the response identical to the one that would correspond to a GET request, but without the response body
        /// </summary>
        Head,
        
        /// <summary>
        /// Requests that the server accept the entity enclosed in the request as a new subordinate of the web resource identified by the URI
        /// </summary>
        Post,

        /// <summary>
        /// Requests that the enclosed entity be stored under the supplied URI
        /// </summary>
        Put,

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        Delete,

        /// <summary>
        /// Echoes back the received request so that a client can see what (if any) changes or additions have been made by intermediate servers
        /// </summary>
        Trace,

        /// <summary>
        /// Returns the HTTP methods that the server supports for the specified URL
        /// </summary>
        Options,
        
        /// <summary>
        /// Converts the request connection to a transparent TCP/IP tunnel
        /// </summary>
        Connect,

        /// <summary>
        /// Is used to apply partial modifications to a resource
        /// </summary>
        Patch
    }
}