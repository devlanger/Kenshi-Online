using System;
using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;
using LiteNetLib.Utils;
using UnityEngine;

namespace Kenshi.Shared.Packets.GameServer
{
    public abstract class SendablePacket : ISendable
    {
        public abstract PacketId packetId { get; }

        public NetDataWriter writer = new NetDataWriter();
        public NetDataReader reader = new NetDataReader();

        public SendablePacket()
        {
            
        }

        public void PutVector3(Vector3 vector)
        {
            writer.Put(vector.x);
            writer.Put(vector.y);
            writer.Put(vector.z);
        }
        
        public Vector3 ReadVector3(NetDataReader reader)
        {
            return new Vector3(reader.GetFloat(),reader.GetFloat(),reader.GetFloat());
        }
        
        public virtual void Serialize(NetDataWriter writer)
        {
        }

        public virtual void Deserialize(NetDataReader reader)
        {
        }

        public static T Deserialize<T>(PacketId id, NetDataReader reader) where T : SendablePacket => Deserialize<T>((byte)id, reader);
        
        public static T Deserialize<T>(byte id, NetDataReader reader) where T : SendablePacket
        {
            var p = Activator.CreateInstance<T>();
            p.Deserialize(reader);
            return p;
        }
    }
}