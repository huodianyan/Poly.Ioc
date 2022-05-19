using System;
using System.Collections;

namespace Poly.Ioc
{
    public interface IIocContainer : IDisposable
    {
        IIocContainer Parent { get; }
        // BindInfo GetBindInfo(Type type);
        void Build();

        void Bind(Type type, Func<IIocContainer, object> func, bool isSingleton = true, string name = null);
        void Bind(Type type, Type toType, bool isSingleton = true, string name = null);
        void Bind(Type type, object instance, string name = null);
        void Unbind(Type type, string name = null);
        //void UnbindAll(Type type);
        bool HasBind(Type type, string name = null);

        object Resolve(Type type, string name = null);
        bool TryResolve(Type type, out object obj, string name = null);
        IEnumerable ResolveAll(Type type, string name = null);

        void Inject(object instance);
    }
}