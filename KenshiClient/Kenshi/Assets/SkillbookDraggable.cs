using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillbookDraggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    private Vector3 startPos;
    private Transform parent;
    public int abilityId;

    public event Action<SkillbookDraggable> DragStart; 
    public event Action<SkillbookDraggable> DragEnd;

    private void Awake()
    {
        parent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        GetComponent<CanvasGroup>().alpha = 0.6f;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        DragStart?.Invoke(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().alpha = 1f;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.parent = parent;
        transform.position = startPos;
        var droppedOn = eventData.pointerEnter;
        if (droppedOn != null)
        {
            if (droppedOn.TryGetComponent(out SkillbarItem skillbarItem))
            {
                if (AbilitiesController.Instance.abilitiesManager.GetItem(abilityId, out var ab) && ab.type == skillbarItem.type)
                {
                    AbilitiesController.Instance.SetHotkey(skillbarItem.hotkeyId, abilityId);
                    AbilitiesController.Instance.SaveSkillmap();
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerDrag);
    }
}
