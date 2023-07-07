using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Kenshi.Shared.Packets.GameServer;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathmatchCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI gameScoreText;
    
    [SerializeField] private TextMeshProUGUI finishScoreText;
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button leaveButton;
    [SerializeField] private ScoresView _scoresView;
    
    private void Awake()
    {
        leaveButton?.onClick.AddListener(LeaveClick);
    }

    private void LeaveClick()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void Finish(DeathmatchMode.Data data)
    {
        nicknameText.SetText(data.winnerUsername);
        finishScoreText.SetText(data.winnerScore.ToString());
        finishPanel.gameObject.SetActive(true);
        gamePanel.gameObject.SetActive(false);
    }

    public void SetScore(int dataCurrentScore, int dataScoreToFinish)
    {
        gameScoreText.SetText($"{dataCurrentScore}/{dataScoreToFinish}");
    }

    public void UpdateData(DeathmatchMode.Data dmData)
    {
        if (dmData.finished)
        {
            Finish(dmData);
        }
        else
        {
            SetScore(dmData.currentScore, dmData.scoreToFinish);
        }
        
        _scoresView.UpdateData(dmData);
    }
}
