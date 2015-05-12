using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table
{
    public enum LocaTableVersion
    {
        Short,
        Long
    }

    public class LocaTable : IOpenTypeFontTable
    {
        public string Tag
        {
            get { return "loca"; }
        }

        /// <summary>
        /// 'null' represents no outline.
        /// </summary>
        public List<uint?> GlyphOffsets { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public static LocaTable Deserialize(BinaryReader reader, long startOffset, ushort numberOfGlyphs,
            uint glyfTableLength, LocaTableVersion version)
        {
            var table = new LocaTable();
            reader.BaseStream.Position = startOffset;

            table.GlyphOffsets.AddRange(Enumerable.Range(0, numberOfGlyphs + 1).Select<int, uint?>(i =>
            {
                var offset = ReadNextOffset(reader, version);

                // Empty outline check
                uint nextOffset = 0;
                if (i != numberOfGlyphs) // Not last glyph
                {
                    // Peek next offset
                    var restorePosition = reader.BaseStream.Position;
                    nextOffset = ReadNextOffset(reader, version);
                    reader.BaseStream.Position = restorePosition;
                }
                if ((i != numberOfGlyphs && offset == nextOffset) ||
                    (i == numberOfGlyphs && offset == glyfTableLength))
                    return null;

                return offset;
            }));

            return table;
        }

        private static uint ReadNextOffset(BinaryReader reader, LocaTableVersion version)
        {
            switch (version)
            {
                case LocaTableVersion.Long:
                    return DataTypeConverter.ReadULong(reader);

                case LocaTableVersion.Short:
                    return (uint) (DataTypeConverter.ReadUShort(reader) * 2);

                default:
                    throw new NotImplementedException();
            }
        }

        public LocaTable()
        {
            GlyphOffsets = new List<uint?>();
        }
    }
}