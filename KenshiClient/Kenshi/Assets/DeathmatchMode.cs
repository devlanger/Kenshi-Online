using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets;
using UnityEngine;

namespace DefaultNamespace
{
    [System.Serializable]
    public class DeathmatchMode : GameMode
    {
        [System.Serializable]
        public class Data
        {
            public string winnerUsername;
            public int winnerScore;
            public int currentScore;
            public int scoreToFinish = 20;
            public bool finished = false;
            public List<PlayerScore> scores = new List<PlayerScore>();
        }
        
        public override GameType GameType => GameType.DEATHMATCH;

        public Data data;

        private Dictionary<string, PlayerScore> userScores = new Dictionary<string, PlayerScore>();

        public List<PlayerScore> GetScores() => userScores.Values.ToList();

        public event Action<Data> OnScoresChanged;
        
        [System.Serializable]
        public class PlayerScore
        {
            public string username;
            public int kills;
            public int deaths;
        }
        
        private string GetWinnerUsername() => 
            userScores
                .OrderByDescending(u => u.Value.kills)
                .FirstOrDefault()
                .Key;
        
        public override void Initialize(GameModeController gameModeController)
        {
            base.Initialize(gameModeController);
            
            data = new Data();
            
            GameServerEventsHandler.Instance.OnPlayerJoined += InstanceOnOnPlayerJoined;
            CombatController.Instance.OnPlayerDeath += InstanceOnOnPlayerDeath;
        }

        private void InstanceOnOnPlayerJoined(Player player)
        {
            AddPlayerScore(player);
        }

        public void AddPlayerScore(Player player)
        {
            if(userScores.ContainsKey(player.Username)) return;
            
            var p = new PlayerScore()
            {
                username = player.Username
            };
            data.scores.Add(p);
            userScores.Add(p.username, p);
        }

        private void InstanceOnOnPlayerDeath(Player attacker, Player target)
        {
            if (!userScores.ContainsKey(attacker.Username))
            {
                AddPlayerScore(attacker);
            }
            
            if (!userScores.ContainsKey(target.Username))
            {
                AddPlayerScore(target);
            }
            
            userScores[attacker.Username].kills++;
            userScores[target.Username].deaths++;
            
            SetScores(GetScores());
            data.currentScore++;

            if (data.currentScore >= data.scoreToFinish)
            {
                string winner = GetWinnerUsername();
                data.winnerScore = userScores[winner].kills;
                data.winnerUsername = winner;
                data.finished = true;
                _gameModeController.StartCoroutine(ShowScoreAndFinish());
                CombatController.Instance.OnPlayerDeath -= InstanceOnOnPlayerDeath;
            }
            else
            {
                GameRoomNetworkController.SendPacketToAll(new DeathmatchModeEventPacket(data), DeliveryMethod.ReliableUnordered);
            }
        }

        private IEnumerator ShowScoreAndFinish()
        {
            var winnerUsername = GetWinnerUsername();
            GameRoomNetworkController.SendPacketToAll(new DeathmatchModeEventPacket(data), DeliveryMethod.ReliableUnordered);
            
            yield return new WaitForSeconds(5);
            _gameModeController.FinishGame();
        }

        public void SetScores(List<PlayerScore> dataScores)
        {
            userScores = dataScores.ToDictionary(k => k.username, v => v);
            data.scores = dataScores;
            OnScoresChanged?.Invoke(data);
        }
    }
}