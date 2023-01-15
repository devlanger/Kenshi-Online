using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoresViewListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lvText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI pingText;
    
    public void Fill(ScoreItemDto dto)
    {
        lvText.SetText($"{dto.level}");        
        nameText.SetText($"{dto.name}");        
        killText.SetText($"{dto.kill}");        
        deathText.SetText($"{dto.death}");        
        pingText.SetText($"{dto.ping}");        
    }
}
