using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingeltonPersistant<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new();
                go.name = typeof(T).Name;
                go.hideFlags = HideFlags.DontSave;
                _instance = go.AddComponent<T>();
            }
            return _instance;
        }
    }

    public virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
}
