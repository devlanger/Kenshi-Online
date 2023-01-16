using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;

    private void Start()
    {
        nicknameText.SetText(ConnectionController.Instance.connectionDto.nickname);
    }
}
