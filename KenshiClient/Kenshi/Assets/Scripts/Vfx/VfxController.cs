using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class VfxController : Singleton<VfxController>
{
    public VfxManager manager;

    public GameObject SpawnFx(int id, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (manager.GetItem(id, out var i))
        {
            GameObject inst = GameObject.Instantiate(i.vfx, pos, rot, parent);
            return inst;
        }

        return null;
    }
    
    public GameObject SpawnFx(VfxId id, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        var fx = manager.items.Find(v => v.vfxId == id);
        if (fx != null)
        {
            GameObject inst = GameObject.Instantiate(fx.vfx, pos, rot, parent);
            return inst;
        }

        return null;
    }
    
    public GameObject SpawnFx(VfxScriptable vfxScriptable, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        GameObject inst = GameObject.Instantiate(vfxScriptable.vfx, pos, rot, parent);
        return inst;
    }
    
    public enum VfxId
    {
        hit_light = 1,
        hit_heavy = 2,
        mana_load = 3,
    }
}


public class Singleton<T> : SerializedMonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }

            return _instance;
        }
    }

    private static T _instance;
}