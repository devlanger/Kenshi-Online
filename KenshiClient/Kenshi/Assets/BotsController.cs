using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kenshi.Shared.Packets.GameServer;
using UnityEngine;
using UnityEngine.AI;

public class BotsController : MonoBehaviour
{
    [SerializeField] private GameServerEventsHandler handler;   
    [SerializeField] private Transform botsParent;
    [SerializeField] private AggressiveBot botPrefab;

    public int botsAmount = 3;

    private void Awake()
    {
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < botsAmount; i++)
        {
            var inst = Instantiate(botPrefab, SpawnPointsController.Instance.GetRandomSpawnPoint(),
                Quaternion.identity, botsParent);
            handler.AddBotToNetwork(inst.GetComponent<Player>());
        }
    }
}
