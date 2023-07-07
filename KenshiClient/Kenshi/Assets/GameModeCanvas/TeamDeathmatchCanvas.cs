using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Kenshi.Shared.Packets.GameServer;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamDeathmatchCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI gameScoreText;
    [SerializeField] private TextMeshProUGUI redScore;
    [SerializeField] private TextMeshProUGUI blueScore;
    
    [SerializeField] private TextMeshProUGUI finishScoreText;
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button leaveButton;
    
    [SerializeField] private TeamScoresView _scoresView;
    
    private void Awake()
    {
        leaveButton?.onClick.AddListener(LeaveClick);
    }

    private void LeaveClick()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void Finish(TeamDeathmatchMode.Data data)
    {
        nicknameText.SetText(data.winnerTeamName);
        finishScoreText.SetText(data.winnerScore.ToString());
        finishPanel.gameObject.SetActive(true);
        gamePanel.gameObject.SetActive(false);
    }

    public void SetScore(int dataCurrentScore, int dataScoreToFinish)
    {
        gameScoreText.SetText($"{dataCurrentScore}/{dataScoreToFinish}");
    }

    public void UpdateData(TeamDeathmatchMode.Data tmData)
    {
        if (tmData.finished)
        {
            Finish(tmData);
        }
        else
        {
            SetScore(tmData.currentScore, tmData.scoreToFinish);
        }
    
        blueScore.SetText(tmData.blueTeamData.score.ToString());
        redScore.SetText(tmData.redTeamData.score.ToString());
        _scoresView.UpdateData(tmData);
    }
}
