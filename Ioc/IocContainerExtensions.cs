using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Poly.Ioc
{
    public static class IocContainerExtensions
    {
        public static void Bind<FromT, ToT>(this IIocContainer container, bool isSingleton = true, string name = null)
        {
            container.Bind(typeof(FromT), typeof(ToT), isSingleton, name);
        }
        public static void Bind<FromT>(this IIocContainer container, object instance, string name = null)
        {
            var type = instance.GetType();
            var fromType = typeof(FromT);
            if (!fromType.IsAssignableFrom(type))
                throw (new ArgumentException($"Register: {type} is not inherit from {fromType}"));
            container.Bind(fromType, instance, name);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Bind(this IIocContainer container, object instance, string name = null)
        {
            container.Bind(instance.GetType(), instance, name);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unbind<FromT>(this IIocContainer container, string name = null)
        {
            container.Unbind(typeof(FromT), name);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unbind(this IIocContainer container, object instance, string name = null)
        {
            container.Unbind(instance.GetType(), name);
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void UnbindAll<FromT>(this IIocContainer container)
        //{
        //    container.UnbindAll(typeof(FromT));
        //}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasBind<T>(this IIocContainer container, string name = null)
        {
            return container.HasBind(typeof(T), name);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IIocContainer container, string name = null)
        {
            return (T)container.Resolve(typeof(T), name);
        }
        public static bool TryResolve<T>(this IIocContainer container, out T obj, string name = null)
        {
            obj = default;
            if (!container.TryResolve(typeof(T), out var obj1, name))
                return false;
            obj = (T)obj1;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ResolveAll<T>(this IIocContainer container, string name = null)
        {
            return container.ResolveAll(typeof(T), name).Cast<T>();
        }
    }
}