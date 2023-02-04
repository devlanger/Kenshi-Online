using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(1)]
public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;

    private void Start()
    {
        if (!ConnectionController.Instance)
            return;

        nicknameText.SetText(ConnectionController.Instance.connectionDto.nickname);
    }
}
