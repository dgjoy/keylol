using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Keylol.FontGarage.Table;

namespace Keylol.FontGarage
{
    internal static class DataTypeLength
    {
        public const int UShort = 2;
        public const int Short = 2;
        public const int F2Dot14 = 2;
        public const int ULong = 4;
        public const int Fixed = 4;
        public const int LongDateTime = 8;
    }

    internal static class DataTypeConverter
    {
        private static SmartBitConverter _bitConverter = new SmartBitConverter(Endian.BigEndian);

        public static ushort ReadUShort(BinaryReader reader)
        {
            return _bitConverter.ToUInt16(reader.ReadBytes(DataTypeLength.UShort), 0);
        }

        public static short ReadShort(BinaryReader reader)
        {
            return _bitConverter.ToInt16(reader.ReadBytes(DataTypeLength.Short), 0);
        }

        public static uint ReadULong(BinaryReader reader)
        {
            return _bitConverter.ToUInt32(reader.ReadBytes(DataTypeLength.ULong), 0);
        }

        public static string ReadFixed(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(DataTypeLength.Fixed);
            var text = Encoding.ASCII.GetString(bytes);
            if (text == "OTTO" || text == "true" || text == "typ1")
                return text;
            return string.Format("{0}.{1:X2}{2:X2}", _bitConverter.ToUInt16(bytes, 0), bytes[2], bytes[3]);
        }

        public static DateTime ReadLongDateTime(BinaryReader reader)
        {
            var seconds = _bitConverter.ToInt64(reader.ReadBytes(DataTypeLength.LongDateTime), 0);
            return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds);
        }
    }

    public class OpenTypeFontSerializer
    {
        private class TableDirectoryEntry
        {
            private Dictionary<string, int> _tagPriorityMap = new Dictionary<string, int>
            {
                {"loca", 100},
                {"glyf", 200}
            };

            public string Tag { get; set; }
            public uint Offset { get; set; }
            public uint Length { get; set; }

            public int Priority
            {
                get { return _tagPriorityMap.ContainsKey(Tag) ? _tagPriorityMap[Tag] : 0; }
            }
        }

        private class TableDirectoryEntryPriorityComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return x > y ? 1 : -1;
            }
        }

        public void Serialize(BinaryWriter writer, OpenTypeFont font) {}

        public OpenTypeFont Deserialize(BinaryReader reader)
        {
            return Deserialize(reader, reader.BaseStream.Position);
        }

        public OpenTypeFont Deserialize(BinaryReader reader, long startOffset)
        {
            var font = new OpenTypeFont();
            reader.BaseStream.Position = startOffset;

            font.SfntVersion = DataTypeConverter.ReadFixed(reader);

            // Table directory
            var numberOfTables = DataTypeConverter.ReadUShort(reader);
            reader.BaseStream.Position += 3*DataTypeLength.UShort; // searchRange, entrySelector, rangeShift
            var entryList = new SortedList<int, TableDirectoryEntry>(numberOfTables,
                new TableDirectoryEntryPriorityComparer());
            for (var i = 0; i < numberOfTables; i++)
            {
                var entry = new TableDirectoryEntry();
                entry.Tag = Encoding.ASCII.GetString(reader.ReadBytes(DataTypeLength.ULong)).Trim();
                reader.BaseStream.Position += DataTypeLength.ULong; // checksum
                entry.Offset = DataTypeConverter.ReadULong(reader);
                entry.Length = DataTypeConverter.ReadULong(reader);
                entryList.Add(entry.Priority, entry);
            }

            // Tables
            foreach (var entry in entryList.Values)
            {
                IOpenTypeFontTable tableToAdd;
                switch (entry.Tag)
                {
                    case "cmap":
                        tableToAdd = CmapTable.Deserialize(reader, entry.Offset);
                        break;

                    case "head":
                        tableToAdd = HeadTable.Deserialize(reader, entry.Offset);
                        break;

                    case "maxp":
                        tableToAdd = MaxpTable.Deserialize(reader, entry.Offset);
                        break;

                    case "loca":
                        tableToAdd = LocaTable.Deserialize(reader, entry.Offset,
                            font.Tables.OfType<MaxpTable>().Single().NumberOfGlyphs,
                            entryList.Values.Single(e => e.Tag == "glyf").Length,
                            font.Tables.OfType<HeadTable>().Single().LocaTableVersion);
                        break;

                    case "glyf":
                        tableToAdd = GlyfTable.Deserialize(reader, entry.Offset, entry.Length,
                            font.Tables.OfType<LocaTable>().Single());
                        break;

                    default:
                        tableToAdd = BinaryDataTable.Deserialize(reader, entry.Offset, entry.Length, entry.Tag);
                        break;
                }
                font.Tables.Add(tableToAdd);
            }

            return font;
        }
    }
}