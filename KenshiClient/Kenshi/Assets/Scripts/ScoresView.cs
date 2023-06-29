using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class ScoresView : ViewUI
{
    [SerializeField] private ContentList list;
    [SerializeField] private ScoresViewListItem item;

    public void SetScores(DeathmatchMode.Data data)
    {
        list.Clear();

        foreach (var score in data.scores)
        {
            AddScoreItem(new ScoreItemDto()
            {
                name = score.username,
                death = score.deaths,
                kill = score.kills,
                level = 1,
                ping = 40
            });
        }
    }

    private void AddScoreItem(ScoreItemDto scoreItemDto)
    {
        var i = list.SpawnItem(item);
        i.Fill(scoreItemDto);
    }
}

public class ScoreItemDto
{
    public byte level;
    public string name;
    public int kill;
    public int death;
    public int ping;
}
