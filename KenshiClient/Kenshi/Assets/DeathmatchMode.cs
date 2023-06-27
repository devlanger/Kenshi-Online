using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets;
using UnityEngine;

namespace DefaultNamespace
{
    public class DeathmatchMode : GameMode
    {
        public override GameType GameType => GameType.DEATHMATCH;

        public int deathCounter = 0;
        public int deathsToFinishGame = 30;
        
        public override void Initialize(GameModeController gameModeController)
        {
            base.Initialize(gameModeController);
            
            CombatController.Instance.OnPlayerDeath += InstanceOnOnPlayerDeath;
        }

        private void InstanceOnOnPlayerDeath(Player obj)
        {
            deathCounter++;

            if (deathCounter >= deathsToFinishGame)
            {
                _gameModeController.StartCoroutine(ShowScoreAndFinish());
                CombatController.Instance.OnPlayerDeath -= InstanceOnOnPlayerDeath;
            }
        }

        private IEnumerator ShowScoreAndFinish()
        {
            GameRoomNetworkController.SendPacketToAll(new DeathmatchModeEndPacket(), DeliveryMethod.ReliableUnordered);
            yield return new WaitForSeconds(5);
            _gameModeController.FinishGame();
        }
    }
}