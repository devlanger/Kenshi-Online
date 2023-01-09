using System.IO;

namespace Kenshi.Shared.Packets.GameServer.Interfaces
{
    public interface ISendable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}