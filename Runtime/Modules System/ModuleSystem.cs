using System;
using UnityEngine;

namespace Evbishop.Runtime.ModulesSystem
{
    [Serializable]
    public abstract class ModulesSystem<T> where T : ModuleBase
    {
        [SerializeReference] private T[] _modules;

        public T[] Modules
        {
            get => _modules;
            set => _modules = value;
        }

        /// <summary>
        /// True if a module of same type is found.
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
                if (_modules[i].GetType() == type)
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
                if (_modules[i].GetType() == type)
                    return true;

            return false;
        }

        public bool HasModule(Type type)
        {
            if (_modules == null || _modules.Length == 0)
                return false;

            for (int i = 0; i < _modules.Length; i++)
                if (_modules[i].GetType() == type)
                    return true;

            return false;
        }

        public bool HasInterface<I>() where I : IModule
        {
            if (_modules == null || _modules.Length == 0)
                return false;

            string interfaceName = typeof(I).ToString();

            for (int i = 0; i < _modules.Length; i++)
                if (_modules[i].GetType().GetInterface(interfaceName) != null)
                    return true;

            return false;
        }
    }
}
