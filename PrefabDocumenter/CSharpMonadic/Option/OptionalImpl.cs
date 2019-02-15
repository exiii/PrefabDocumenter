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
using System.Reflection;

namespace CenterCLR
{
    /// <summary>
    /// Option support implementations.
    /// </summary>
    internal static class OptionalImpl
    {
#if !NET_STANDARD
        // For netstandard compatibility
        public static Delegate CreateDelegate(this MethodInfo method, Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, method);
        }
#endif
        public static T CreateDelegate<T>(this MethodInfo method)
        {
            return (T)(object)method.CreateDelegate(typeof(T));
        }

        private static class TypeInformation<T>
        {
            static TypeInformation()
            {
                // Instantiate for AOT
                IsNotNullImpl(null);
            }

            public static readonly bool IsValueType = typeof(T).IsValueType;
            private static Func<T, bool> isNotNull;

            public static bool IsNotNullImpl(object value)
            {
                return value != null;
            }

            public static bool IsNotNull(T value)
            {
                if (isNotNull == null)
                {
                    isNotNull =
                        typeof(TypeInformation<T>).
                            GetMethod("IsNotNullImpl").
                            CreateDelegate<Func<T, bool>>();
                }

                return isNotNull(value);
            }
        }

        public static bool HasValue<T>(T value)
        {
            return TypeInformation<T>.IsValueType || TypeInformation<T>.IsNotNull(value);
        }
    }
}
