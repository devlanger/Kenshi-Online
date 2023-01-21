using System;
using System.Collections;
using System.Collections.Generic;
using Kenshi.Shared.Packets.GameServer;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PositionUpdateSnapshot
{
    public float time;
    public PositionUpdatePacket packet;
}

public class PlayerInterpolation : MonoBehaviour
{
    public List<PositionUpdateSnapshot> m_BufferedState = new List<PositionUpdateSnapshot>();
    int m_TimestampCount;
    
    public double m_InterpolationBackTime = 0.1;
    public double m_ExtrapolationLimit = 0.5;
    
    public void Push(PositionUpdateSnapshot snapshot)
    {
        snapshot.time = Time.time;
        m_BufferedState.Insert(0, snapshot);

        if (m_BufferedState.Count == 10)
        {
            m_BufferedState.RemoveAt(9);
        }
        
        m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Count);
    }

    void Interpolate () {
        // This is the target playback time of the rigid body
        double interpolationTime = Time.time - m_InterpolationBackTime;
       
        // Use interpolation if the target playback time is present in the buffer
        if (m_BufferedState[0].time > interpolationTime)
        {
            // Go through buffer and find correct state to play back
            for (int i=0;i<m_TimestampCount;i++)
            {
                if (m_BufferedState[i].time <= interpolationTime || i == m_TimestampCount-1)
                {
                    // The state one slot newer (<100ms) than the best playback state
                    PositionUpdateSnapshot rhs = m_BufferedState[Mathf.Max(i-1, 0)];
                    // The best playback state (closest to 100 ms old (default time))
                    PositionUpdateSnapshot lhs = m_BufferedState[i];
                   
                    // Use the time between the two slots to determine if interpolation is necessary
                    double length = rhs.time - lhs.time;
                    float t = 0.0F;
                    // As the time difference gets closer to 100 ms t gets closer to 1 in
                    // which case rhs is only used
                    // Example:
                    // Time is 10.000, so sampleTime is 9.900
                    // lhs.time is 9.910 rhs.time is 9.980 length is 0.070
                    // t is 9.900 - 9.910 / 0.070 = 0.14. So it uses 14% of rhs, 86% of lhs
                    if (length > 0.0001)
                        t = (float)((interpolationTime - lhs.time) / length);
                   
                    // if t=0 => lhs is used directly
                    transform.localPosition = Vector3.Lerp(lhs.packet.Position, rhs.packet.Position, t);
                    //transform.localRotation = Quaternion.Slerp(lhs.rot, rhs.rot, t);
                    return;
                }
            }
        }
        // Use extrapolation
        else
        {
            if (m_BufferedState.Count > 1)
            {
                PositionUpdateSnapshot latest = m_BufferedState[0];
                PositionUpdateSnapshot previous = m_BufferedState[1];

                float extrapolationLength = (float)(interpolationTime - latest.time);
                // Don't extrapolation for more than 500 ms, you would need to do that carefully
                if (extrapolationLength < m_ExtrapolationLimit)
                {
                    Vector3 direction = (latest.packet.Position - previous.packet.Position).normalized;
                    transform.position += direction * Time.deltaTime;

                    // float axisLength = extrapolationLength * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
                    // Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);
                    //
                    // rigidbody.position = latest.pos + latest.velocity * extrapolationLength;
                    // rigidbody.rotation = angularRotation * latest.rot;
                    // rigidbody.velocity = latest.velocity;
                    // rigidbody.angularVelocity = latest.angularVelocity;
                }
            }
        }
    }
    
    private void Update()
    {
        if (m_BufferedState.Count <= 1)
        {
            return;
        }

        Interpolate();
    }
}
