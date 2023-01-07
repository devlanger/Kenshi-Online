using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ExtendedSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    protected virtual void Awake()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke();
    }
}