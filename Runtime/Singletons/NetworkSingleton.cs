#if FISHNET
using FishNet.Object;

public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkSingleton<T>
{
    public static T Instance { get; private set; }

    protected bool TryInitializeSingleton()
    {
        if (Instance is null)
        {
            Instance = this as T;
            return true;
        }

        Destroy(gameObject);
        return false;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
#endif
