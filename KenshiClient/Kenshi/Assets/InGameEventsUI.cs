using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using UnityEngine;

public class InGameEventsUI : MonoBehaviour
{
    [SerializeField] private ContentList _contentList;
    [SerializeField] private InGameEventListItem _inGameEventListItemPrefab;

    private void Awake()
    {
        // AddInGameEventLabel(new GameEventPacket.PlayerDied
        // {
        //     attackerName = "Test1",
        //     targetName = "Target",
        //     dt = GameEventPacket.PlayerDied.DeathType.melee,
        //     playerId = 0
        // });
        //
        // AddInGameEventLabel(new GameEventPacket.PlayerDied
        // {
        //     attackerName = "Test1",
        //     targetName = "Target",
        //     dt = GameEventPacket.PlayerDied.DeathType.skill,
        //     playerId = 0
        // });
    }

    public void AddInGameEventLabel(GameEventPacket.PlayerDied data)
    {
        StartCoroutine(AddLabel(data));
    }

    private IEnumerator AddLabel(GameEventPacket.PlayerDied data)
    {
        var item = _contentList.SpawnItem(_inGameEventListItemPrefab);
        item.Fill(data);
        yield return new WaitForSeconds(4);
        Destroy(item.gameObject);
    }
}
