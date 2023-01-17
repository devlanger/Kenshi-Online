using System.IO;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer.Interfaces
{
    public interface ISendable
    {
        void Serialize(NetDataWriter writer);
        void Deserialize(NetDataReader reader);
    }
}