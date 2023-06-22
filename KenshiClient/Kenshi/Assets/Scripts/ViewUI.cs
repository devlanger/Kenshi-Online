using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ViewUI : MonoBehaviour
{
    public Canvas canvas;

    public bool IsActive => canvas != null && canvas.enabled;

    public void Toggle() { if (IsActive) { Deactivate(); } else { Activate(); } }
    public virtual void Activate() { canvas.enabled = true; }
    public virtual void Deactivate() { canvas.enabled = false; }
}