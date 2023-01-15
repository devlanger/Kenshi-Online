using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

public class RoomsController : MonoBehaviour
{
    private ConnectionController connection;
    
    private void Awake()
    {
        connection = FindObjectOfType<ConnectionController>();
    }
}
