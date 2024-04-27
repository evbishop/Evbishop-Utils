using UnityEngine;

namespace Evbishop.Runtime.ModulesSystem
{
    [System.Serializable]
    public abstract class ModulesSystem<T>
        where T : ModuleBase
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
            module = default(U);

            if (_modules == null || _modules.Length == 0) return false;

            for (int i = 0; i < _modules.Length; i++)
            {
                if (_modules[i].GetType() == typeof(U))
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

            for (int i = 0; i < _modules.Length; i++)
                if (_modules[i].GetType() == typeof(U))
                    return true;

            return false;
        }

        public bool HasModule(System.Type type)
        {
            if (_modules == null || _modules.Length == 0)
                return false;

            for (int i = 0; i < _modules.Length; i++)
                if (_modules[i].GetType() == type)
                    return true;

            return false;
        }
    }
}
