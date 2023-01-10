using System.IO;
using Kenshi.Shared.Enums;

namespace Kenshi.Shared.Packets.GameServer
{
    public class LoginResponsePacket : SendablePacket
    {
        public int _playerId;

        public LoginResponsePacket() : base(PacketId.LoginResponse)
        {
        }

        public LoginResponsePacket(int playerId) : base(PacketId.LoginResponse)
        {
            _playerId = playerId;
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(_playerId);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            _playerId = reader.ReadInt32();
        }
    }
}