﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Defines extension methods that allow coding against <see cref="Type"/> without conditional compilation on versions of .NET framework.
    /// </summary>
    internal static class TypeExtensions
    {
#if NET40
        public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public static bool IsAbstract(this Type type)
        {
            return type.IsAbstract;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }
#else
        /// <summary>
        /// Returns a value that indicates whether the specified type can be assigned to the current type.
        /// </summary>
        /// <remarks>
        /// This method emulates the built-in method of the <see cref="Type"/> class which is not available on Windows Runtime.
        /// </remarks>
        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        /// <summary>
        /// Returns all the public properties of the specified type.
        /// </summary>
        /// <remarks>
        /// This method emulates the built-in method of the <see cref="Type"/> class which is not available on Windows Runtime.
        /// Note that, unlike the built-in <see cref="Type"/> method, this method does not return properties defined in any of the base types.
        /// However, this should be sufficient for our public types, which have to be sealed on Windows Runtime.
        /// </remarks>
        public static PropertyInfo[] GetProperties(this Type type)
        {
            var properties = new List<PropertyInfo>();
            properties.AddRange(type.GetTypeInfo().DeclaredProperties);
            var baseType = type.GetTypeInfo().BaseType;
            if (null != baseType)
            {
                properties.AddRange(baseType.GetProperties());
            }

            return properties.ToArray();
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }
#endif
    }
}