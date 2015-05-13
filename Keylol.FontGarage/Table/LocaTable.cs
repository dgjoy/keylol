using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table
{
    public enum LocaTableVersion : short
    {
        Short = 0,
        Long = 1
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
                    {
                        switch (version)
                        {
                            case LocaTableVersion.Short:
                                DataTypeConverter.WriteUShort(writer, (ushort) glyphOffset.Value);
                                break;

                            case LocaTableVersion.Long:
                                DataTypeConverter.WriteULong(writer, glyphOffset.Value);
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                    repeat = 1;
                }
            }
        }

        public static LocaTable Deserialize(BinaryReader reader, long startOffset, ushort numberOfGlyphs,
            uint glyfTableLength, LocaTableVersion version)
        {
            var table = new LocaTable();
            reader.BaseStream.Position = startOffset;

            table.GlyphOffsets.AddRange(Enumerable.Range(0, numberOfGlyphs).Select<int, uint?>(i =>
            {
                var offset = ReadNextOffset(reader, version);

                // Empty outline check
                uint nextOffset = 0;
                if (i != numberOfGlyphs - 1) // Not last glyph
                {
                    // Peek next offset
                    var restorePosition = reader.BaseStream.Position;
                    nextOffset = ReadNextOffset(reader, version);
                    reader.BaseStream.Position = restorePosition;
                }
                if ((i != numberOfGlyphs - 1 && offset == nextOffset) ||
                    (i == numberOfGlyphs - 1 && offset == glyfTableLength))
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
                    return (uint) (DataTypeConverter.ReadUShort(reader)*2);

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