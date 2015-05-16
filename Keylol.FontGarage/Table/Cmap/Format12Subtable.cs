using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Keylol.FontGarage.Table.Cmap
{
    public class Format12Subtable : CmapSubtable
    {
        public override ushort Format
        {
            get { return 12; }
        }

        public uint Language { get; set; }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteUShort(writer, Format);
            DataTypeConverter.WriteUShort(writer, 0);
            writer.BaseStream.Position += DataTypeLength.ULong;
            DataTypeConverter.WriteULong(writer, Language);

            var charList = CharGlyphIdMap.Keys.OrderBy(u => u).ToList();
            var segStarts = new List<uint>();
            var segEnds = new List<uint>();
            for (var i = 0; i < charList.Count; i++)
            {
                if (i == 0 || charList[i] - 1 != charList[i - 1] ||
                    CharGlyphIdMap[charList[i]] - 1 != CharGlyphIdMap[charList[i - 1]])
                    segStarts.Add(charList[i]);
                if (i == charList.Count - 1 || charList[i] + 1 != charList[i + 1] ||
                    CharGlyphIdMap[charList[i]] + 1 != CharGlyphIdMap[charList[i + 1]])
                    segEnds.Add(charList[i]);
            }

            var segCount = segStarts.Count;
            DataTypeConverter.WriteULong(writer, (uint) segCount);
            for (var i = 0; i < segCount; i++)
            {
                DataTypeConverter.WriteULong(writer, segStarts[i]);
                DataTypeConverter.WriteULong(writer, segEnds[i]);
                DataTypeConverter.WriteULong(writer, CharGlyphIdMap[segStarts[i]]);
            }

            // Set length
            var length = writer.BaseStream.Position - startOffset;
            writer.BaseStream.Position = startOffset + 2*DataTypeLength.UShort;
            DataTypeConverter.WriteULong(writer, (uint) length);

            // Restore write position
            writer.BaseStream.Position = startOffset + length;
        }

        public static Format12Subtable Deserialize(BinaryReader reader, long startOffset)
        {
            var table = new Format12Subtable();
            reader.BaseStream.Position = startOffset + 2*DataTypeLength.UShort + DataTypeLength.ULong;
            table.Language = DataTypeConverter.ReadULong(reader);
            var numberOfGroups = DataTypeConverter.ReadULong(reader);
            for (var i = 0; i < numberOfGroups; i++)
            {
                var startCharCode = DataTypeConverter.ReadULong(reader);
                var endCharCode = DataTypeConverter.ReadULong(reader);
                var startGlyphId = DataTypeConverter.ReadULong(reader);
                for (var j = startCharCode; j <= endCharCode; j++)
                    table.CharGlyphIdMap[j] = startGlyphId + j - startCharCode;
            }
            return table;
        }
    }
}