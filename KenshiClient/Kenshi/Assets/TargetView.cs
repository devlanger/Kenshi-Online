using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetView : ViewUI
{
    [SerializeField] private Transform img;
    private TargetController targetController;
    
    private void Awake()
    {
        targetController = GameObject.FindObjectOfType<TargetController>();
        targetController.OnTargetSet += TargetControllerOnOnTargetSet;
        targetController.OnTargetHighlight += TargetControllerOnOnTargetHighlight;
    }

    private void TargetControllerOnOnTargetHighlight(Player obj)
    {
        
    }

    private void TargetControllerOnOnTargetSet(Player obj)
    {
        if (obj == null)
        {
            //Deactivate();
        }
        else
        {
            //Activate();
        }
    }

    private void Update()
    {
        if (targetController.highlightTarget == null)
        {
            img.gameObject.SetActive(false);
        }
        else
        {
            img.gameObject.SetActive(true);
            img.transform.position = Camera.main.WorldToScreenPoint(targetController.highlightTarget.transform.position + Vector3.up);
        }
    }
}
