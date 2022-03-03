using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T instance = null;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                T singleton = FindObjectOfType(typeof(T)) as T;
                instance = singleton;

                if(instance == null)
                {
                    GameObject singletonObj = new GameObject(typeof(T).Name);
                    singletonObj.AddComponent<T>();
                    instance = singletonObj.GetComponent<T>();
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
