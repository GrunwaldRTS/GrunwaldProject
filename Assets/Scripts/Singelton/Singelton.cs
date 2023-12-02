using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singelton<T> : MonoBehaviour where T : Component
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
}
