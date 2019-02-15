/////////////////////////////////////////////////////////////////////////////////////////////////
//
// CSharpMonadic
// Copyright (c) 2017 Kouji Matsui (@kekyo2)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace CenterCLR
{
    public static class OptionalExtension
    {
        /// <summary>
        /// Support LINQ query expressions at Option.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="option"></param>
        /// <param name="collectionSelector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public static Optional<V> SelectMany<T, U, V>(
            this Optional<T> option,
            Func<T, Optional<U>> collectionSelector,
            Func<T, U, V> resultSelector)
        {
            return option.Bind(value1 =>
                collectionSelector(value1).Bind(value2 =>
                    Optional.Return(resultSelector(value1, value2))));
        }
    }
}
