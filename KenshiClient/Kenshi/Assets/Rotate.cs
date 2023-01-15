using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] private Vector3 speed;
    
    void Update()
    {
        transform.localEulerAngles += speed * Time.deltaTime;
    }
}
