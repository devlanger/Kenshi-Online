using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Models;
using TMPro;
using UnityEngine;

public class GameRoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playersAmountText;
    
    public void Fill(ContainerDto dto)
    {
        this.roomNameText.SetText(dto.Name);
        this.playersAmountText.SetText($"{dto.PlayersCount}/{dto.MaxPlayersCount}");
    }
}
