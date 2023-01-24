using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetAnimatorMaterial : MonoBehaviour
{
    [SerializeField] Vector2 scrollSpeed;
    Renderer rend;

    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer> ();
    }

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        rend.material.SetTextureOffset("_BaseMap", offset);
    }
}
