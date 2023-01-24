using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameEventListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI attackerName;
    [SerializeField] private TextMeshProUGUI targetName;
    [SerializeField] private Image eventIcon;

    public void Fill(GameEventPacket.PlayerDied data)
    {
        attackerName.SetText(data.attackerName);
        targetName.SetText(data.targetName);
        //eventIcon.sprite = null;
    }
}
