using System.IO;
using Kenshi.Shared.Enums;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginRequestPacket : SendablePacket
    {
        public LoginRequestPacket() : base(PacketId.LoginRequest)
        {
            
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.ReadInt32();
        }

        public int _playerId { get; set; }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(_playerId);
        }
    }
}