using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoresView : ViewUI
{
    [SerializeField] private ContentList list;
    [SerializeField] private ScoresViewListItem item;

    private void Start()
    {
        AddScoreItem(new ScoreItemDto()
        {
            name = "Adminn",
            death = 3,
            kill = 7,
            level = 99,
            ping = 52
        });
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
    public byte kill;
    public byte death;
    public int ping;
}
