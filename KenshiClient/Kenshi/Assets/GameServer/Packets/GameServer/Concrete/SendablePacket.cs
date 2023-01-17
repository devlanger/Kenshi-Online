using System;
using System.IO;
using Kenshi.Shared.Enums;
using Kenshi.Shared.Packets.GameServer.Interfaces;
using LiteNetLib.Utils;

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