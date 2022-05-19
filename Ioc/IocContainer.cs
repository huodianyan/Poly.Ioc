using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Poly.Ioc
{
    public class IocInjectAttribute : Attribute
    {
        public string Name;
    }
    public class IocBindInfo
    {
        public Type BindType;
        public Type ToType;
        public object ToInstance;
        public string BindName;
        public bool IsSingleton;
        public Func<IIocContainer, object> ToFunc;

        public override string ToString()
        {
            return $"[{BindType?.Name},{ToType?.Name},{ToInstance},{IsSingleton},{BindName}]";
        }
    }
    public class IocTypeInjectInfo
    {
        public Type Type;
        public FieldInfo[] FieldInfos;
        public PropertyInfo[] PropertyInfos;
        internal ConstructorInfo[] ConstructorInfos;
        internal bool IsValid;
        public IocTypeInjectInfo(Type type)
        {
            Type = type;
        }
    }

    public class IocContainer : IIocContainer
    {
        #region static
        private static Dictionary<string, IocContainer> containersDict = new Dictionary<string, IocContainer>();
        public static IocContainer GetContainer(string id = "")
        {
            containersDict.TryGetValue(id, out var container);
            return container;
        }
        public static IocContainer CreateContainer(string id, string parentId = "")
        {
            if (containersDict.TryGetValue(id, out var container))
                return container;

            containersDict.TryGetValue(parentId, out var parent);
            return new IocContainer(id, parent);
        }
        #endregion

        private string id;
        private IIocContainer parent;
        //private Dictionary<Type, Dictionary<string, IocBindInfo>> bindInfosDict = new Dictionary<Type, Dictionary<string, IocBindInfo>>();
        private Dictionary<Type, List<IocBindInfo>> bindInfosDict = new Dictionary<Type, List<IocBindInfo>>();
        //private Dictionary<Type, BindInfo> bindInfoDict = new Dictionary<Type, BindInfo>();
        private Dictionary<Type, IocTypeInjectInfo> typeInjectInfoDict = new Dictionary<Type, IocTypeInjectInfo>();
        private bool isBuilt;

        public string Id => id;
        public IIocContainer Parent => parent;
        // public IReadOnlyDictionary<Type, BindInfo> BindInfoDict => bindInfoDict;

        public IocContainer(string id = "", IIocContainer parent = null)
        {
            this.id = id;
            this.parent = parent;

            containersDict[id] = this;
            Bind(typeof(IIocContainer), this, id);
        }

        public void Dispose()
        {
            parent = null;
            bindInfosDict.Clear();
            bindInfosDict = null;
        }
        public void Build()
        {
            if (isBuilt) return;
            isBuilt = true;
            foreach (var bindInfos in bindInfosDict.Values)
                foreach (var bindInfo in bindInfos)
                    if (bindInfo.ToInstance != null) Inject(bindInfo.ToInstance);
        }

        #region Bind/Unbind
        public bool HasBind(Type type, string name = null)
        {
            if (!bindInfosDict.TryGetValue(type, out var bindInfos)) return false;
            if (name == null) return true;
            return bindInfos.FindIndex((info) => info.BindName == name) >= 0;
        }
        public void Bind(Type type, Type toType, bool isSingleton = true, string name = null)
        {
            if (!bindInfosDict.TryGetValue(type, out var bindInfos))
            {
                bindInfos = new List<IocBindInfo>();
                bindInfosDict.Add(type, bindInfos);
            }
            bindInfos.Add(new IocBindInfo
            {
                BindType = type,
                ToType = toType,
                BindName = name,
                IsSingleton = isSingleton
            });
        }
        public void Bind(Type type, Func<IIocContainer, object> toFunc, bool isSingleton = true, string name = null)
        {
            if (!bindInfosDict.TryGetValue(type, out var bindInfos))
            {
                bindInfos = new List<IocBindInfo>();
                bindInfosDict.Add(type, bindInfos);
            }
            bindInfos.Add(new IocBindInfo
            {
                BindType = type,
                BindName = name,
                ToFunc = toFunc,
                IsSingleton = isSingleton
            });
        }
        public void Bind(Type type, object toInstance, string name = null)
        {
            if (!bindInfosDict.TryGetValue(type, out var bindInfos))
            {
                bindInfos = new List<IocBindInfo>();
                bindInfosDict.Add(type, bindInfos);
            }
            bindInfos.Add(new IocBindInfo
            {
                ToInstance = toInstance,
                BindName = name,
                IsSingleton = true
            });
            if (isBuilt)
                Inject(toInstance);
        }
        public void Unbind(Type type, string name = null)
        {
            if (!bindInfosDict.TryGetValue(type, out var bindInfos)) return;
            if (name == null)
            {
                bindInfos.Clear();
                bindInfosDict.Remove(type);
                return;
            }
            bindInfos.RemoveAll((info) => info.BindName == name);
            if (bindInfos.Count == 0) bindInfosDict.Remove(type);
        }
        //public void UnbindAll(Type type)
        //{
        //    if (!bindInfosDict.TryGetValue(type, out var bindInfos)) return;
        //    bindInfos.Clear();
        //    bindInfosDict.Remove(type);
        //}
        #endregion

        #region Resolve
        public IEnumerable ResolveAll(Type type, string name = null)
        {
            if (!bindInfosDict.TryGetValue(type, out var bindInfos))
                return null;
            IEnumerable<IocBindInfo> infos = bindInfos;
            if (name != null)
                infos = bindInfos.Where((info) => info.BindName == name);
            return infos?.Select((info) => Resolve(info));
        }
        public bool TryResolve(Type type, out object obj, string name = null)
        {
            obj = null;
            if (bindInfosDict.TryGetValue(type, out var bindInfos))
            {
                var bindInfo = bindInfos.Find((info) => info.BindName == name);
                if (bindInfo != null)
                {
                    obj = Resolve(bindInfo);
                    return true;
                }
            }
            if (Parent != null)
                return Parent.TryResolve(type, out obj, name);
            return false;
        }
        public object Resolve(Type type, string name = null)
        {
            if (bindInfosDict.TryGetValue(type, out var bindInfos))
            {
                var bindInfo = bindInfos.Find((info) => info.BindName == name);
                if (bindInfo != null)
                    return Resolve(bindInfo);
            }
            return Parent?.Resolve(type, name);
        }
        private object Resolve(IocBindInfo bindInfo)
        {
            var instance = bindInfo.ToInstance;
            if (instance != null)
                return instance;
            if (bindInfo.ToType != null)
            {
                instance = CreateInstance(bindInfo.ToType);
                if (bindInfo.IsSingleton)
                    bindInfo.ToInstance = instance;
                return instance;
            }
            if (bindInfo.ToFunc != null)
            {
                instance = bindInfo.ToFunc.Invoke(this);
                Inject(instance);
                if (bindInfo.IsSingleton)
                    bindInfo.ToInstance = instance;
                return instance;
            }
            return null;
        }
        private object CreateInstance(Type type)
        {
            if (!TryGetTypeInjectInfo(type, out var injectInfo))
                return Activator.CreateInstance(type);
            object instance = null;
            if (injectInfo.ConstructorInfos == null)
                instance = Activator.CreateInstance(type);
            else
            {
                var constructorInfo = injectInfo.ConstructorInfos[0];
                var paramenterInfos = constructorInfo.GetParameters();
                var parameters = new object[paramenterInfos.Length];
                for (int i = 0; i < paramenterInfos.Length; i++)
                {
                    if (TryResolve(paramenterInfos[i].ParameterType, out var obj))
                    {
                        parameters[i] = obj;
                        //Console.WriteLine($"----CreateInstance: {paramenterInfos[i].ParameterType} -> {obj}");
                    }
                }
                instance = constructorInfo.Invoke(parameters);
            }
            Inject(instance, injectInfo);
            return instance;
        }
        #endregion

        #region Inject
        public void Inject(object instance)
        {
            if (instance == null)
                return;
            var type = instance.GetType();
            if (!TryGetTypeInjectInfo(type, out var injectInfo))
                return;
            //Console.WriteLine($"Inject: {instance}[{type.Name}]");
            Inject(instance, injectInfo);
        }
        private void Inject(object instance, IocTypeInjectInfo injectInfo)
        {
            if (injectInfo.FieldInfos != null)
            {
                foreach (var fieldInfo in injectInfo.FieldInfos)
                {
                    var name = fieldInfo.GetCustomAttribute<IocInjectAttribute>().Name;
                    if (TryResolve(fieldInfo.FieldType, out var obj, name))
                    {
                        fieldInfo.SetValue(instance, obj);
                        //Console.WriteLine($"----Inject: {instance}.{fieldInfo.Name}: {fieldInfo.FieldType} -> {obj}");
                    }
                }
            }
            if (injectInfo.PropertyInfos != null)
            {
                foreach (var propertyInfo in injectInfo.PropertyInfos)
                {
                    var name = propertyInfo.GetCustomAttribute<IocInjectAttribute>().Name;
                    if (TryResolve(propertyInfo.PropertyType, out var obj, name))
                    {
                        propertyInfo.SetValue(instance, obj);
                        //Console.WriteLine($"----Inject: {instance}.{propertyInfo.Name}: {propertyInfo.PropertyType} -> {obj}");
                    }
                }
            }
        }
        private bool TryGetTypeInjectInfo(Type type, out IocTypeInjectInfo injectInfo)
        {
            if (!typeInjectInfoDict.TryGetValue(type, out injectInfo))
            {
                //field
                var filedInfoList = new List<FieldInfo>();
                var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < fieldInfos.Length; i++)
                {
                    var fieldInfo = fieldInfos[i];
                    var attribute = fieldInfo.GetCustomAttribute<IocInjectAttribute>();
                    if (attribute != null)
                        filedInfoList.Add(fieldInfo);
                }
                //property
                var propertyInfoList = new List<PropertyInfo>();
                var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < propertyInfos.Length; i++)
                {
                    var propertyInfo = propertyInfos[i];
                    var attribute = propertyInfo.GetCustomAttribute<IocInjectAttribute>();
                    if (attribute != null)
                        propertyInfoList.Add(propertyInfo);
                }
                //constructor
                var constructorInfoList = new List<ConstructorInfo>();
                var constructorInfos = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < constructorInfos.Length; i++)
                {
                    var constructorInfo = constructorInfos[i];
                    var attribute = constructorInfo.GetCustomAttribute<IocInjectAttribute>();
                    if (attribute != null)
                        constructorInfoList.Add(constructorInfo);
                }
                injectInfo = new IocTypeInjectInfo(type);
                injectInfo.IsValid = filedInfoList.Count > 0 || propertyInfoList.Count > 0 || constructorInfoList.Count > 0;
                if (filedInfoList.Count > 0)
                    injectInfo.FieldInfos = filedInfoList.ToArray();
                if (propertyInfoList.Count > 0)
                    injectInfo.PropertyInfos = propertyInfoList.ToArray();
                if (constructorInfoList.Count > 0)
                    injectInfo.ConstructorInfos = constructorInfoList.ToArray();
                typeInjectInfoDict.Add(type, injectInfo);
            }
            return injectInfo.IsValid;
        }
        #endregion
    }
}