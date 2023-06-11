using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using UnityEngine;
using UnityEngine.AI;

public class FreeBot : MonoBehaviour
{
    [SerializeField] private bool move = true;
    
    private void Start()
    {
        StartCoroutine(UpdateBots());
        if (move)
        {
            StartCoroutine(MoveRandomlyBots());
        }
    }

    private IEnumerator MoveRandomlyBots()
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
            
            var agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                continue;
            }
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 10;
            agent.SetDestination(agent.transform.position + randomDirection);
            //GameRoomNetworkController.SendPacketToAll(new PositionUpdatePacket(bot.NetworkId, bot.transform.position.x, bot.transform.position.y, bot.transform.position.z, 0));

            yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 8f));
        }
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

            var bot = GetComponent<Player>();
            float speed = 0;
            if (bot.agent != null)
            {
                speed = bot.agent.velocity.sqrMagnitude > 0.01f ? 2 : 0;
            }

            bot.playerStateMachine.CurrentState?.UpdateInput(bot.playerStateMachine);
            bot.Input.leftClick = true;
            
            GameRoomNetworkController.SendPacketToAll(new PositionUpdatePacket(bot.NetworkId, bot.transform.position.x, bot.transform.position.y, bot.transform.position.z, (byte)(bot.transform.eulerAngles.y / 5), speed));

            yield return new WaitForSeconds(0.1f);
        }
    }
}
