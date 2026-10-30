using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

namespace Evbishop.Runtime.Pooling
{
    public class PoolItem : MonoBehaviour, IPoolable<PoolItem>
    {
        [SerializeField] private bool _resetScaleOnEnable = true;
        [SerializeField] private bool _autoReturnToPool = false;
        [SerializeField, ShowIf(nameof(_autoReturnToPool))] private float _autoReturnToPoolDelayInSeconds = 1;

        private CancellationTokenSource _cts;
        private bool _isBeingDestroyed = false;
        private Vector3 _originalScale;

        public Pools Pools { get; set; }
        public IObjectPool<PoolItem> Pool { get; set; }

        public event Action OnGet;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnDestroy()
        {
            if (!_isBeingDestroyed)
            {
                HandleDestroy();
            }
        }

        private void OnEnable()
        {
            if (_resetScaleOnEnable)
                transform.localScale = _originalScale;
        }

        public void HandleGet()
        {
            if (_autoReturnToPool)
                ReturnToPoolAsync().Forget();
            OnGet?.Invoke();
        }

        public void HandleRelease()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }
            var instanceID = GetEntityId();
            var instantiatedAddressables = Pools.InstantiatedAddressables;
            if (instantiatedAddressables.ContainsKey(instanceID))
                instantiatedAddressables.Remove(instanceID);
            if (Pools != null)
                transform.SetParent(Pools.transform);
            gameObject.SetActive(false);
        }

        public void HandleDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts = null;
            }

            if (Pools != null)
            {
                var instanceID = GetEntityId();
                var instantiatedAddressables = Pools.InstantiatedAddressables;
                if (instantiatedAddressables.ContainsKey(instanceID))
                    instantiatedAddressables.Remove(instanceID);
            }

            _isBeingDestroyed = true;
            Destroy(gameObject);
        }

        private async UniTaskVoid ReturnToPoolAsync()
        {
            _cts?.Cancel();
            _cts = new();
            bool isCancelled = await UniTask
                .Delay(TimeSpan.FromSeconds(_autoReturnToPoolDelayInSeconds), cancellationToken: _cts.Token)
                .SuppressCancellationThrow();
            if (isCancelled) return;

            Release();
        }

        public void Release()
        {
            if (Pools == null || !Pools.InstantiatedAddressables.ContainsKey(GetEntityId()))
                return;
            Pool.Release(this);
        }
    }
}