using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Keylol.FontGarage.Table
{
    public enum LocaTableVersion : short
    {
        Short = 0,
        Long = 1
    }

    public class LocaTable : IOpenTypeFontTable
    {
        public LocaTable()
        {
            GlyphOffsets = new List<uint?>();
        }

        /// <summary>
        ///     'null' represents no outline.
        /// </summary>
        public List<uint?> GlyphOffsets { get; set; }

        internal uint GlyfTableLength { get; set; }

        public string Tag
        {
            get { return "loca"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            var version = font.Get<HeadTable>().LocaTableVersion;
            var repeat = 1;
            foreach (var glyphOffset in GlyphOffsets)
            {
                if (glyphOffset == null)
                    repeat++;
                else
                {
                    for (var i = 0; i < repeat; i++)
                        WriteNextOffset(writer, glyphOffset.Value, version);
                    repeat = 1;
                }
            }
            WriteNextOffset(writer, GlyfTableLength, version);

            font.Get<MaxpTable>().NumberOfGlyphs = (ushort) GlyphOffsets.Count;
        }

        public static LocaTable Deserialize(BinaryReader reader, long startOffset, ushort numberOfGlyphs,
            LocaTableVersion version)
        {
            var table = new LocaTable();
            reader.BaseStream.Position = startOffset;

            table.GlyphOffsets.AddRange(Enumerable.Range(0, numberOfGlyphs).Select<int, uint?>(i =>
            {
                var offset = ReadNextOffset(reader, version);

                // Peek next offset
                var restorePosition = reader.BaseStream.Position;
                var nextOffset = ReadNextOffset(reader, version);
                reader.BaseStream.Position = restorePosition;

                // Empty outline check
                if (offset == nextOffset)
                    return null;

                return offset;
            }));
            table.GlyfTableLength = ReadNextOffset(reader, version);

            return table;
        }

        private static uint ReadNextOffset(BinaryReader reader, LocaTableVersion version)
        {
            switch (version)
            {
                case LocaTableVersion.Long:
                    return DataTypeConverter.ReadULong(reader);

                case LocaTableVersion.Short:
                    return (uint) (DataTypeConverter.ReadUShort(reader)*2);

                default:
                    throw new NotImplementedException();
            }
        }

        private static void WriteNextOffset(BinaryWriter writer, uint offset, LocaTableVersion version)
        {
            switch (version)
            {
                case LocaTableVersion.Long:
                    DataTypeConverter.WriteULong(writer, offset);
                    break;

                case LocaTableVersion.Short:
                    DataTypeConverter.WriteUShort(writer, (ushort) (offset/2));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void ChangeNumberOfGlyphs(int number)
        {
            GlyphOffsets = new List<uint?>(number);
            GlyphOffsets.AddRange(Enumerable.Repeat<uint?>(null, number));
        }
    }
}