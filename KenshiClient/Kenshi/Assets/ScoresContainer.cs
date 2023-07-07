using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoresContainer : MonoBehaviour
{
    [SerializeField] private ContentList list;
    [SerializeField] private ScoresViewListItem item;

    public class Data
    {
        public string name;
        public int deaths, kills, level, ping;
    }

    public void SetScores(List<Data> scores)
    {
        list.Clear();

        foreach (var score in scores)
        {
            AddScoreItem(new ScoreItemDto()
            {
                name = score.name,
                death = score.deaths,
                kill = score.kills,
                level = score.level,
                ping = score.ping
            });
        }
    }

    private void AddScoreItem(ScoreItemDto scoreItemDto)
    {
        var i = list.SpawnItem(item);
        i.Fill(scoreItemDto);
    }
}