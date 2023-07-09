using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kenshi.Shared.Models;
using Kenshi.Shared.Packets.GameServer;
using LiteNetLib;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    [System.Serializable]
    public class TeamDeathmatchMode : GameMode
    {
        [System.Serializable]
        public class Data
        {
            public string winnerTeamName;
            public int winnerScore;
            public int currentScore => blueTeamData.score + redTeamData.score;
            public int scoreToFinish = 20;
            public bool finished = false;
            
            public TeamData blueTeamData = new TeamData() { name = "blue"};
            public TeamData redTeamData = new TeamData() { name = "red" };

            public TeamData GetUserTeam(string username)
            {
                if (blueTeamData.scores.Any(p => p.username == username))
                {
                    return blueTeamData;
                }
                
                if (redTeamData.scores.Any(p => p.username == username))
                {
                    return redTeamData;
                }
                
                return null;
            }

            public TeamData GetTeamByLowestPlayerCount()
            {
                if (redTeamData.scores.Count > blueTeamData.scores.Count)
                {
                    return blueTeamData;
                }

                return redTeamData;
            }
        }

        public class TeamData
        {
            public string name;
            public int score;
            public List<PlayerScore> scores = new List<PlayerScore>();

            public PlayerScore GetPlayerScore(string username)
            {
                return scores.FirstOrDefault(p => p.username == username);
            }

            public void AddPlayer(PlayerScore score)
            {
                scores.Add(score);
            }
        }
        
        public override GameType GameType => GameType.TEAM_DEATHMATCH;

        public Data data;

        public List<PlayerScore> GetScores() => null;

        public event Action<Data> OnScoresChanged;
        
        [System.Serializable]
        public class PlayerScore
        {
            public string username;
            public string teamName;
            public int kills;
            public int deaths;
        }
        
        public override void Initialize(GameModeController gameModeController)
        {
            base.Initialize(gameModeController);
            
            data = new Data();

            if (GameServer.IsServer)
            {
                GameServerEventsHandler.Instance.OnPlayerJoined += InstanceOnOnPlayerJoined;
                CombatController.Instance.OnPlayerDeath += InstanceOnOnPlayerDeath;
            }
        }

        private void InstanceOnOnPlayerJoined(Player player)
        {
            AddPlayerScore(player);
        }

        public void AddPlayerScore(Player player)
        {
            var team = data.GetUserTeam(player.Username);
            if(team != null) return;
            
            team = data.GetTeamByLowestPlayerCount();

            var p = new PlayerScore()
            {
                username = player.Username,
                teamName = team.name
            };
            player.teamName = team.name;
            team.AddPlayer(p);
        }

        private void InstanceOnOnPlayerDeath(Player attacker, Player target)
        {
            var attackerTeam = data.GetUserTeam(attacker.Username);
            attackerTeam.GetPlayerScore(attacker.Username).kills++;
            attackerTeam.score++;
            
            data.GetUserTeam(target.Username).GetPlayerScore(target.Username).deaths++;
            
            OnScoresChanged?.Invoke(data);

            if (data.currentScore >= data.scoreToFinish)
            {
                data.winnerScore = attackerTeam.score;
                data.winnerTeamName = attackerTeam.name;
                data.finished = true;
                _gameModeController.StartCoroutine(ShowScoreAndFinish());
                CombatController.Instance.OnPlayerDeath -= InstanceOnOnPlayerDeath;
            }
            else
            {
                GameRoomNetworkController.SendPacketToAll(new GameModeEventPacket(data), DeliveryMethod.ReliableUnordered);
            }
        }

        private IEnumerator ShowScoreAndFinish()
        {
            GameRoomNetworkController.SendPacketToAll(new GameModeEventPacket(data), DeliveryMethod.ReliableUnordered);
            yield return new WaitForSeconds(5);
            _gameModeController.FinishGame();
        }
    }
}