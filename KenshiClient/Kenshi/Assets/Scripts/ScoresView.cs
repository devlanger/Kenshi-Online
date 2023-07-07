using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(10)]
public class ScoresView : ViewUI
{
    public ScoresContainer scoresContainer;

    public void UpdateData(DeathmatchMode.Data data)
    {
        scoresContainer.SetScores(data.scores.Select(d => new ScoresContainer.Data()
        {
            name = d.username,
            deaths = d.deaths,
            ping = 40,
            level = 1,
            kills = d.kills
        }).ToList());
    }
}

public class ScoreItemDto
{
    public string name;
    public int level;
    public int kill;
    public int death;
    public int ping;
}
