using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class TeamScoresView : ScoresView
{
    public ScoresContainer blueTeamScoresContainer;

    public void UpdateData(TeamDeathmatchMode.Data mode)
    {
        scoresContainer.SetScores(mode.redTeamData.scores.Select(d => new ScoresContainer.Data()
        {
            name = d.username,
            deaths = d.deaths,
            ping = 40,
            level = 1,
            kills = d.kills
        }).ToList());
        
        blueTeamScoresContainer.SetScores(mode.blueTeamData.scores.Select(d => new ScoresContainer.Data()
        {
            name = d.username,
            deaths = d.deaths,
            ping = 40,
            level = 1,
            kills = d.kills
        }).ToList());
    }
}