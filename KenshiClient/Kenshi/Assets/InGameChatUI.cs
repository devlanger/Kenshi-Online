using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameChatUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private CanvasGroup scrollBar;
    
    public bool IsChatActive { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {
        IsChatActive = false;
        RefreshState();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown((KeyCode.Return)))
        {
            IsChatActive = !IsChatActive;
            RefreshState();
        }
    }

    private void RefreshState()
    {
        _inputField.gameObject.SetActive(IsChatActive);
        background.enabled = IsChatActive;
        scrollBar.alpha = IsChatActive ? 1 : 0;
    }
}
