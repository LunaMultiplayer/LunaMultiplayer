/*
 * Copyright (C) 2011 uhttpsharp project - http://github.com/raistlinthewiz/uhttpsharp
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public class HttpRouter : IHttpRequestHandler
    {
        private readonly IDictionary<string, IHttpRequestHandler> _handlers = new Dictionary<string, IHttpRequestHandler>(StringComparer.InvariantCultureIgnoreCase);

        public HttpRouter With(string function, IHttpRequestHandler handler)
        {
            _handlers.Add(function, handler);

            return this;
        }

        public Task Handle(IHttpContext context, Func<Task> nextHandler)
        {
            string function = string.Empty;

            if (context.Request.RequestParameters.Length > 0)
            {
                function = context.Request.RequestParameters[0];
            }

            IHttpRequestHandler value;
            if (_handlers.TryGetValue(function, out value))
            {
                return value.Handle(context, nextHandler);
            }
            

            // Route not found, Call next.
            return nextHandler();
        }
    }
}