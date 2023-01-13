using System;
using System.IO;
using Kenshi.Shared.Packets.GameServer;

namespace Kenshi.Backend.GameServer
{
    class Protocol
    {
        private void InitWriter()
        {
            m_buffer = new byte[256];
            m_stream = new MemoryStream(m_buffer);
            m_writer = new BinaryWriter(m_stream);
        }

        private void InitReader(byte[] buffer)
        {
            m_stream = new MemoryStream(buffer);
            m_reader = new BinaryReader(m_stream);
        }

        public byte[] Serialize(SendablePacket packet)
        {
            InitWriter();
            packet.Serialize(m_writer);
            return m_buffer;
        }

        public void Deserialize(byte[] buf, out byte code, out int value)
        {
            InitReader(buf);
            
            code = m_reader.ReadByte();
            value = m_reader.ReadInt32();
        }

        private BinaryWriter m_writer;
        private BinaryReader m_reader;
        private MemoryStream m_stream;
        private byte[] m_buffer;
    }
}