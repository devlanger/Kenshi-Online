using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerCollisionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private UnityEvent triggerEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (collisionMask != (collisionMask | (1 << other.gameObject.layer))) 
        {
            return;
        }

        triggerEvent?.Invoke();
    }

    public void SpawnEffect(GameObject go)
    {
        var inst = Instantiate(go, transform.position, transform.rotation);
        GameObject.Destroy(inst.gameObject, 1);
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}
