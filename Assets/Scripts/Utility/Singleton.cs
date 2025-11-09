using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// MonoBehaviour Singleton
/// usage£º
/// public class GameManager : Singleton<GameManager> { ... }
/// use GameManager.Instance to access it
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object _lock = new object();
    public static bool HasInstance => instance != null;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    lock (_lock)
                    {
                        if ( instance == null )
                        {
                            Debug.Log("make new game object " + typeof(T).Name);
                            GameObject singletonObj = new GameObject(typeof(T).Name);
                            instance = singletonObj.AddComponent<T>();
                            //DontDestroyOnLoad(singletonObj);
                        }
                    }
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if ( instance == null )
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
