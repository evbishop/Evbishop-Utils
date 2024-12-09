using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Evbishop.Runtime.ModulesSystem
{
    [Serializable]
    public abstract class ModulesSystem<T>
        where T : ModuleBase
    {
        [OdinSerialize]
#if UNITY_EDITOR
        [OnCollectionChanged(nameof(BeforeModulesChanged), nameof(AfterModulesChanged))]
#endif
        private T[] _modules = new T[0];

#if UNITY_EDITOR
        public virtual void BeforeModulesChanged(CollectionChangeInfo info, object value)
        {

        }

        public virtual void AfterModulesChanged(CollectionChangeInfo info, object value)
        {

        }
#endif

        public T[] Modules
        {
            get => _modules;
            set => _modules = value;
        }

        /// <summary>
        /// True if a module of the specified type or derived type is found.
        /// </summary>
        /// <param name="module">Found module. Null if not found.</param>
        /// <returns></returns>
        public bool TryGetModule<U>(out U module) where U : T
        {
            module = default;

            if (_modules == null || _modules.Length == 0)
                return false;

            Type type = typeof(U);

            for (int i = 0; i < _modules.Length; i++)
            {
                if (type.IsInstanceOfType(_modules[i]))
                {
                    module = (U)_modules[i];
                    break;
                }
            }

            return module != null;
        }

        public bool HasModule<U>() where U : T
        {
            if (_modules == null || _modules.Length == 0)
                return false;

            Type type = typeof(U);

            for (int i = 0; i < _modules.Length; i++)
                if (type.IsInstanceOfType(_modules[i]))
                    return true;

            return false;
        }

        public bool HasModule(Type type)
        {
            if (_modules == null || _modules.Length == 0)
                return false;

            for (int i = 0; i < _modules.Length; i++)
                if (type.IsInstanceOfType(_modules[i]))
                    return true;

            return false;
        }

        public bool HasInterface<I>() where I : IModule
        {
            if (_modules == null || _modules.Length == 0)
                return false;

            Type interfaceType = typeof(I);

            for (int i = 0; i < _modules.Length; i++)
            {
                Type moduleType = _modules[i].GetType();
                if (interfaceType.IsAssignableFrom(moduleType))
                    return true;
            }

            return false;
        }

        public List<I> GetModulesWithInterface<I>() where I : IModule
        {
            List<I> result = new();

            if (_modules == null || _modules.Length == 0)
                return result;

            Type interfaceType = typeof(I);

            for (int i = 0; i < _modules.Length; i++)
            {
                Type moduleType = _modules[i].GetType();
                if (interfaceType.IsAssignableFrom(moduleType))
                {
                    result.Add((I)(IModule)_modules[i]);
                }
            }

            return result;
        }
    }
}