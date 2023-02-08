using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Models;
using TMPro;
using UnityEngine;

public class UserDataComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nicknameLabel;

    void Awake()
    {
        if (ConnectionController.Instance)
        {
            ConnectionController.Instance.OnLogged += InstanceOnOnLogged;
        }
    }

    private void OnDestroy()
    {
        if (ConnectionController.Instance)
        {
            ConnectionController.Instance.OnLogged -= InstanceOnOnLogged;
        }
    }

    private void InstanceOnOnLogged(ConnectionDto obj)
    {
        if (obj == null)
        {
            return;
        }
        
        nicknameLabel.SetText(obj.nickname);
    }
}
