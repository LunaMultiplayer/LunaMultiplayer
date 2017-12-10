//
// UnityExt.cs
//
// Author:
// 	Jim Borden  <jim.borden@couchbase.com>
//
// Copyright (c) 2015 Couchbase, Inc All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//#if UNITY
using System.Threading.Tasks;
using System.Threading;

namespace System
{
    internal static class UnityExt
    {
        public static T Await<T>(this Task<T> t)
        {
            using(var mre = new ManualResetEventSlim()) {
                t.ConfigureAwait(false).GetAwaiter().OnCompleted(mre.Set);
                mre.Wait();
                return t.Result;
            }
        }
            
        public static void Await(this Task t)
        {
            using(var mre = new ManualResetEventSlim()) {
                t.ConfigureAwait(false).GetAwaiter().OnCompleted(mre.Set);
                mre.Wait();
            }
        }
    }
}
//#endif