using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                lock (_lock)
                {
                    _instance = FindObjectOfType<T>();

                    if (!_instance)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                    }
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"Duplicate instance of {typeof(T).Name} found and destroyed.");
            Destroy(gameObject);
        }
    }

    protected void OnDestroy()
    {
        _instance = null;
    }
}