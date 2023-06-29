using System;
using System.Collections.Generic;
using LiteNetLib.Utils;

namespace Kenshi.Shared.Packets.GameServer
{
    public static class PacketUtils
    {
        public static void PutList<T>(this NetDataWriter writer, List<T> items, Action<T> writeAction) where T : class
        {
            writer.Put(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                writeAction?.Invoke(items[i]);
            }
        }
        
        public static List<T> GetList<T>(this NetDataReader reader, Action<T> readAction) where T : class, new()
        {
            int count = reader.GetInt();
            List<T> itemList = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                T item = new T();
                readAction?.Invoke(item);
                itemList.Add(item);
            }

            return itemList;
        }
    }
}