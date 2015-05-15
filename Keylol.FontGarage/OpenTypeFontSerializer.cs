using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keylol.FontGarage.Table;

namespace Keylol.FontGarage
{
    public class OpenTypeFontSerializer
    {
        private readonly string[] _supportedSfntVersions = {"1.0000", "OTTO", "true", "typ1"};

        public OpenTypeFontSerializer()
        {
            EnableChecksum = true;
        }

        /// <summary>
        ///     Set to false will no longer calculate checksum to save time.
        /// </summary>
        public bool EnableChecksum { get; set; }

        private static uint CalculateChecksum(BinaryReader reader, long startOffset, uint length)
        {
            uint checksum = 0;
            reader.BaseStream.Position = startOffset;
            var checkCount = (length + 3)/4;
            for (var i = 0; i < checkCount; i++)
                checksum += DataTypeConverter.ReadULong(reader);
            return checksum;
        }

        public void Serialize(BinaryWriter writer, OpenTypeFont font)
        {
            DataTypeConverter.WriteFixed(writer, font.SfntVersion);
            DataTypeConverter.WriteUShort(writer, (ushort) font.Tables.Count);
            {
                var pow = Math.Floor(Math.Log(font.Tables.Count, 2));
                var searchRange = Math.Pow(2, pow)*16;
                DataTypeConverter.WriteUShort(writer, (ushort) searchRange);
                DataTypeConverter.WriteUShort(writer, (ushort) pow);
                DataTypeConverter.WriteUShort(writer, (ushort) (font.Tables.Count*16 - searchRange));
            }
            var startOffsetOfCurrentTableEntry = writer.BaseStream.Position;
            var startOffsetOfCurrentTableData = startOffsetOfCurrentTableEntry +
                                                4*DataTypeLength.ULong*font.Tables.Count;
            font.Tables.Sort((table1, table2) => String.Compare(table1.Tag, table2.Tag, StringComparison.Ordinal));
            var reader = new BinaryReader(writer.BaseStream);
            long headChecksumOffset = 0;
            long hheaNumOfHMetricsOffset = 0;
            foreach (var table in font.Tables)
            {
                table.Serialize(writer, startOffsetOfCurrentTableData, font);

                // A hack to update NumberOfHMetrics in hhea table
                if (table is HheaTable)
                    hheaNumOfHMetricsOffset = writer.BaseStream.Position - DataTypeLength.UShort;
                if (table is HmtxTable)
                {
                    var restorePosition = writer.BaseStream.Position;
                    writer.BaseStream.Position = hheaNumOfHMetricsOffset;
                    DataTypeConverter.WriteUShort(writer, font.Get<HheaTable>().NumberOfHMetrics);
                    writer.BaseStream.Position = restorePosition;
                }

                // Calculate length
                var endOffset = writer.BaseStream.Position;
                var actualLength = endOffset - startOffsetOfCurrentTableData;

                // 4k padding
                if (endOffset%4 != 0)
                {
                    var zeroCount = 4 - endOffset%4;
                    for (var i = 0; i < zeroCount; i++)
                        writer.Write((byte) 0);
                    endOffset = writer.BaseStream.Position;
                }

                // Calculate checksum
                uint checksum = 0;
                if (EnableChecksum)
                {
                    checksum = CalculateChecksum(reader, startOffsetOfCurrentTableData, (uint) actualLength);
                    if (table is HeadTable)
                        headChecksumOffset = startOffsetOfCurrentTableData + DataTypeLength.Fixed*2;
                }

                // Write table info to the directory
                writer.BaseStream.Position = startOffsetOfCurrentTableEntry;
                DataTypeConverter.WriteTag(writer, table.Tag);
                DataTypeConverter.WriteULong(writer, checksum); // checksum
                DataTypeConverter.WriteULong(writer, (uint) startOffsetOfCurrentTableData);
                DataTypeConverter.WriteULong(writer, (uint) actualLength);

                // Set offsets for next table
                startOffsetOfCurrentTableEntry = writer.BaseStream.Position;
                startOffsetOfCurrentTableData = endOffset;
            }

            // Calculate checksum for the entire font
            if (EnableChecksum && headChecksumOffset > 0)
            {
                var entireChecksum = 0xB1B0AFBA - CalculateChecksum(reader, 0, (uint) reader.BaseStream.Length);
                writer.BaseStream.Position = headChecksumOffset;
                DataTypeConverter.WriteULong(writer, entireChecksum);
            }
        }

        public OpenTypeFont Deserialize(BinaryReader reader)
        {
            var font = new OpenTypeFont {SfntVersion = DataTypeConverter.ReadFixed(reader)};
            if (!_supportedSfntVersions.Contains(font.SfntVersion))
                throw new NotSupportedException("Bad sfnt version.");

            // Table directory
            var numberOfTables = DataTypeConverter.ReadUShort(reader);
            reader.BaseStream.Position += 3*DataTypeLength.UShort; // searchRange, entrySelector, rangeShift
            var entryList = new SortedList<int, TableDirectoryEntry>(numberOfTables,
                new TableDirectoryEntryPriorityComparer());
            for (var i = 0; i < numberOfTables; i++)
            {
                var entry = new TableDirectoryEntry {Tag = DataTypeConverter.ReadTag(reader)};
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
                            font.Get<MaxpTable>().NumberOfGlyphs,
                            font.Get<HeadTable>().LocaTableVersion);
                        break;

                    case "glyf":
                        tableToAdd = GlyfTable.Deserialize(reader, entry.Offset, entry.Length,
                            font.Get<LocaTable>());
                        break;

                    case "hhea":
                        tableToAdd = HheaTable.Deserialize(reader, entry.Offset);
                        break;

                    case "hmtx":
                        tableToAdd = HmtxTable.Deserialize(reader, entry.Offset, font.Get<HheaTable>().NumberOfHMetrics,
                            font.Get<MaxpTable>().NumberOfGlyphs);
                        break;

                    case "post":
                        tableToAdd = PostTable.Deserialize(reader, entry.Offset);
                        break;

                    default:
                        tableToAdd = BinaryDataTable.Deserialize(reader, entry.Offset, entry.Length, entry.Tag);
                        break;
                }
                font.Tables.Add(tableToAdd);
            }

            return font;
        }

        private class TableDirectoryEntry
        {
            private readonly Dictionary<string, int> _tagPriorityMap = new Dictionary<string, int>
            {
                {"loca", 100},
                {"glyf", 200},
                {"hmtx", 300}
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
    }
}