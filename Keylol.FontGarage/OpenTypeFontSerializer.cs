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
            var restorePosition = reader.BaseStream.Position;
            reader.BaseStream.Position = startOffset;
            var checkCount = (length + 3)/4;
            for (var i = 0; i < checkCount; i++)
                checksum += DataTypeConverter.ReadULong(reader);
            reader.BaseStream.Position = restorePosition;
            return checksum;
        }

        public void Serialize(BinaryWriter writer, OpenTypeFont font)
        {
            DataTypeConverter.WriteFixed(writer, font.SfntVersion);
            DataTypeConverter.WriteUShort(writer, (ushort) font.Tables.Count);

            var pow = Math.Floor(Math.Log(font.Tables.Count, 2));
            var searchRange = Math.Pow(2, pow)*16;
            DataTypeConverter.WriteUShort(writer, (ushort) searchRange);
            DataTypeConverter.WriteUShort(writer, (ushort) pow);
            DataTypeConverter.WriteUShort(writer, (ushort) (font.Tables.Count*16 - searchRange));

            var tableDirectoryStartOffset = writer.BaseStream.Position;
            writer.BaseStream.Position += 4*DataTypeLength.ULong*font.Tables.Count;

            var entryList = new List<TableDirectoryEntry>(font.Tables.Count);
            var additionalInfo = new SerializationInfo(); // Shared info among all IOpenTypeFontSerializable class

            foreach (
                var table in font.Tables.OrderByDescending(table => TableDirectoryEntry.GetPriorityOfTag(table.Tag)))
            {
                var startOffsetOfThisTable = writer.BaseStream.Position;

                table.Serialize(writer, startOffsetOfThisTable, additionalInfo);

                entryList.Add(new TableDirectoryEntry
                {
                    Tag = table.Tag,
                    Offset = (uint) startOffsetOfThisTable,
                    Length = (uint) (writer.BaseStream.Position - startOffsetOfThisTable)
                });

                // 4byte padding
                if (writer.BaseStream.Position%4 != 0)
                {
                    var zeroCount = 4 - writer.BaseStream.Position%4;
                    for (var i = 0; i < zeroCount; i++)
                        writer.Write((byte) 0);
                }
            }

            // Write table directory
            var reader = new BinaryReader(writer.BaseStream);
            writer.BaseStream.Position = tableDirectoryStartOffset;
            foreach (var entry in entryList.OrderBy(entry => entry.Tag, StringComparer.Ordinal))
            {
                DataTypeConverter.WriteTag(writer, entry.Tag);
                DataTypeConverter.WriteULong(writer,
                    EnableChecksum ? CalculateChecksum(reader, entry.Offset, entry.Length) : 0);
                DataTypeConverter.WriteULong(writer, entry.Offset);
                DataTypeConverter.WriteULong(writer, entry.Length);
            }

            // Calculate checksum for the entire font
            if (EnableChecksum)
            {
                writer.BaseStream.Position = entryList.Single(entry => entry.Tag == "head").Offset +
                                             2*DataTypeLength.Fixed;
                DataTypeConverter.WriteULong(writer,
                    0xB1B0AFBA - CalculateChecksum(reader, 0, (uint) reader.BaseStream.Length));
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
            var entryList = Enumerable.Range(0, numberOfTables).Select(i =>
            {
                var entry = new TableDirectoryEntry {Tag = DataTypeConverter.ReadTag(reader)};
                reader.BaseStream.Position += DataTypeLength.ULong; // checksum
                entry.Offset = DataTypeConverter.ReadULong(reader);
                entry.Length = DataTypeConverter.ReadULong(reader);
                return entry;
            }).ToList();

            // Tables
            font.Tables.AddRange(
                entryList.OrderBy(entry => entry.Priority).Select<TableDirectoryEntry, IOpenTypeFontTable>(entry =>
                {
                    switch (entry.Tag)
                    {
                        case "cmap":
                            return CmapTable.Deserialize(reader, entry.Offset);

                        case "head":
                            return HeadTable.Deserialize(reader, entry.Offset);

                        case "maxp":
                            return MaxpTable.Deserialize(reader, entry.Offset);

                        case "loca":
                            return LocaTable.Deserialize(reader, entry.Offset,
                                font.Get<MaxpTable>().NumberOfGlyphs,
                                font.Get<HeadTable>().LocaTableVersion);

                        case "glyf":
                            return GlyfTable.Deserialize(reader, entry.Offset, entry.Length,
                                font.Get<LocaTable>());

                        case "hhea":
                            return HheaTable.Deserialize(reader, entry.Offset);

                        case "hmtx":
                            return HmtxTable.Deserialize(reader, entry.Offset, font.Get<HheaTable>().NumberOfHMetrics,
                                font.Get<MaxpTable>().NumberOfGlyphs);

                        case "post":
                            return PostTable.Deserialize(reader, entry.Offset);

                        default:
                            return BinaryDataTable.Deserialize(reader, entry.Offset, entry.Length, entry.Tag);
                    }
                }));

            return font;
        }

        private class TableDirectoryEntry
        {
            private static readonly Dictionary<string, int> TagPriorityMap = new Dictionary<string, int>
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
                get { return GetPriorityOfTag(Tag); }
            }

            public static int GetPriorityOfTag(string tag)
            {
                return TagPriorityMap.ContainsKey(tag) ? TagPriorityMap[tag] : 0;
            }
        }
    }
}