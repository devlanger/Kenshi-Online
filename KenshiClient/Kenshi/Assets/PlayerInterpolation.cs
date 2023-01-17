using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using UnityEngine;

[System.Serializable]
public class PositionUpdateSnapshot
{
    public float time;
    public PositionUpdatePacket packet;
}

public class PlayerInterpolation : MonoBehaviour
{
    public List<PositionUpdateSnapshot> receivedPackets = new List<PositionUpdateSnapshot>();
        
    public void Push(PositionUpdateSnapshot snapshot)
    {
        snapshot.time = Time.time;
        receivedPackets.Insert(0, snapshot);

        if (receivedPackets.Count == 10)
        {
            receivedPackets.RemoveAt(9);
        }
    }

    private void Update()
    {
        if (receivedPackets.Count > 1)
        {
            var newSnapshot = receivedPackets[0];
            float delta = Time.deltaTime / (1f / 66f);
            
            transform.position = Vector3.Lerp(transform.position, newSnapshot.packet.Position, delta);
        }
    }
}
