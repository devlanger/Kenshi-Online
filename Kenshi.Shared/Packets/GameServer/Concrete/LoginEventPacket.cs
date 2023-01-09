using System.IO;
using Kenshi.Shared.Enums;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginEventPacket : SendablePacket
    {
        public uint _playerId;

        public LoginEventPacket() : base(PacketId.LoginEvent)
        {
        }

        public LoginEventPacket(uint playerId) : base(PacketId.LoginEvent)
        {
            _playerId = playerId;
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.ReadUInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(_playerId);
        }
    }
}