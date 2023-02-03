using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomizationImage : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform character;

    private void OnEnable()
    {
        character.localEulerAngles = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        character.localEulerAngles -= new Vector3(0, eventData.delta.x, 0);
    }
}
