using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class ClientConnectionSettings : ScriptableObject
{
    [Header("Client Settings")]
    /// <summary>
    /// Should use local game hub server?
    /// </summary>
    public bool useLocalServer = false;
    
    /// <summary>
    /// Should connect to manually create local test server on play button click? 
    /// </summary>
    public bool useTestServerOnPlayButton = false;
    
    /// <summary>
    /// Should use local api server to login?
    /// </summary>
    public bool useLocalLoginServer = false;
    
    [Header("Ips")]
    public string loginLocalUrl;
    public string loginRemoteUrl;
    public string gameHubHost;

    [Header("Api Endpoints")]
    public string loginApiEndpoint;
    public string checkTokenApiEndpoint;
}
