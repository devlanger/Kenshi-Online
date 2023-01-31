using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerCollisionHandler : MonoBehaviour
{
    public Player owner;

    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private UnityEvent<Collider> triggerEvent;
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float destroyAfter = 0;

    private void Start()
    {
        if (destroyAfter != 0)
        {
            Destroy(gameObject, destroyAfter);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collisionMask != (collisionMask | (1 << other.gameObject.layer))) 
        {
            return;
        }

        if (owner != null && other.gameObject == owner.gameObject)
        {
            return;
        }

        triggerEvent?.Invoke(other);

        if (destroyOnHit)
        {
            Destroy();
        }
    }

    public void SpawnEffect(GameObject go)
    {
        var inst = Instantiate(go, transform.position, transform.rotation);
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}
