using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kenshi.Shared.Packets.GameServer;
using UnityEngine;

public class BotsController : MonoBehaviour
{
    [SerializeField] private GameServerEventsHandler handler;

    private IEnumerator Start()
    {
        yield return UpdateBots();
    }

    IEnumerator UpdateBots()
    {
        while (true)
        {
            if (GameServer.Instance == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            if (GameServer.Instance._netServer == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            var bots = handler.bots.Values.ToList();
            foreach (var bot in bots)
            {
                GameRoomNetworkController.SendPacketToAll(new PositionUpdatePacket(bot.NetworkId, bot.transform.position.x, bot.transform.position.y, bot.transform.position.z, 0));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
