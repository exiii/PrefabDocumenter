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
    /// <summary>
    /// Option monad type manipulators.
    /// </summary>
    public sealed class Optional
    {
        /// <summary>
        /// Anonymous nothing identity.
        /// </summary>
        public static readonly Optional None = new Optional();

        private Optional()
        {
        }

        /// <summary>
        /// Construct Option.
        /// </summary>
        /// <typeparam name="T">Real type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Option value</returns>
        public static Optional<T> Return<T>(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// Construct Option from nullable value.
        /// </summary>
        /// <typeparam name="T">Real type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Option value</returns>
        public static Optional<T> Return<T>(T? value)
            where T : struct
        {
            return value.HasValue ? new Optional<T>(value.Value) : Optional<T>.None;
        }
    }

    /// <summary>
    /// Option monad type.
    /// </summary>
    /// <typeparam name="T">Real type</typeparam>
    public struct Optional<T>
    {
        /// <summary>
        /// Typed nothing identity.
        /// </summary>
        public static readonly Optional<T> None = new Optional<T>();

        private readonly T rawValue;

        internal Optional(T value)
        {
            rawValue = value;
        }

        public Optional<U> Bind<U>(Func<T, Optional<U>> binder)
        {
            return OptionalImpl.HasValue(rawValue) ? binder(rawValue) : Optional<U>.None;
        }

        public void Match(Action<T> some, Action none)
        {
            if (OptionalImpl.HasValue(rawValue))
            {
                some(rawValue);
            }
            else
            {
                none();
            }
        }

        public bool Equals(Optional<T> option)
        {
            var hasValue = OptionalImpl.HasValue(rawValue);
            return hasValue == OptionalImpl.HasValue(option.rawValue) && rawValue.Equals(option.rawValue);
        }

        public override bool Equals(object obj)
        {
            return obj is Optional<T> option && this.Equals(option);
        }

        public override int GetHashCode()
        {
            return OptionalImpl.HasValue(rawValue) ? rawValue.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return OptionalImpl.HasValue(rawValue) ? $"[{rawValue}]" : "[None]";
        }

        public static implicit operator Optional<T>(Optional none)
        {
            return None;
        }
    }
}
