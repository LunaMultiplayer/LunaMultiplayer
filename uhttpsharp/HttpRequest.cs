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
using System.Diagnostics;
using uhttpsharp.Headers;

namespace uhttpsharp
{
    [DebuggerDisplay("{Method} {OriginalUri,nq}")]
    internal class HttpRequest : IHttpRequest
    {
        private readonly IHttpHeaders _headers;
        private readonly HttpMethods _method;
        private readonly string _protocol;
        private readonly Uri _uri;
        private readonly string[] _requestParameters;
        private readonly IHttpHeaders _queryString;
        private readonly IHttpPost _post;

        public HttpRequest(IHttpHeaders headers, HttpMethods method, string protocol, Uri uri, string[] requestParameters, IHttpHeaders queryString, IHttpPost post)
        {
            _headers = headers;
            _method = method;
            _protocol = protocol;
            _uri = uri;
            _requestParameters = requestParameters;
            _queryString = queryString;
            _post = post;
        }

        public IHttpHeaders Headers
        {
            get { return _headers; }
        }

        public HttpMethods Method
        {
            get { return _method; }
        }

        public string Protocol
        {
            get { return _protocol; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public string[] RequestParameters
        {
            get { return _requestParameters; }
        }

        public IHttpPost Post
        {
            get { return _post; }
        }

        public IHttpHeaders QueryString
        {
            get { return _queryString; }
        }

        internal string OriginalUri
        {
            get
            {
                if (QueryString == null)
                {
                    return Uri.OriginalString;    
                }

                return Uri.OriginalString + "?" + QueryString.ToUriData();

            }
        }
    }

    public interface IHttpRequest
    {
        IHttpHeaders Headers { get; }

        HttpMethods Method { get; }

        string Protocol { get; }

        Uri Uri { get; }

        string[] RequestParameters { get; }

        IHttpPost Post {get;}

        IHttpHeaders QueryString { get; }

    }

    public interface IHttpPost
    {

        byte[] Raw {get;}

        IHttpHeaders Parsed {get;}

    }

    public sealed class HttpRequestParameters
    {
        private readonly string[] _params;

        private static readonly char[] Separators = { '/' };

        public HttpRequestParameters(Uri uri)
        {
            var url = uri.OriginalString;
            _params = url.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public IList<string> Params
        {
            get { return _params; }
        }
    }
}