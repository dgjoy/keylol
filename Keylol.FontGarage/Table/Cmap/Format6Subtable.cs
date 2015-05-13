using System;
using System.IO;
using System.Linq;

namespace Keylol.FontGarage.Table.Cmap
{
    public class Format6Subtable : CmapSubtable
    {
        public override ushort Format
        {
            get { return 6; }
        }

        public ushort Language { get; set; }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteUShort(writer, Format);
            var lengthOffset = writer.BaseStream.Position;
            writer.BaseStream.Position += DataTypeLength.UShort;
            DataTypeConverter.WriteUShort(writer, Language);

            var firstCode = CharGlyphIdMap.Keys.Min();
            var lastCode = CharGlyphIdMap.Keys.Max();
            DataTypeConverter.WriteUShort(writer, (ushort) firstCode);
            DataTypeConverter.WriteUShort(writer, (ushort) (lastCode - firstCode + 1));

            for (var i = firstCode; i <= lastCode; i++)
            {
                if (CharGlyphIdMap.ContainsKey(i))
                    DataTypeConverter.WriteUShort(writer, (ushort) CharGlyphIdMap[i]);
                else
                    DataTypeConverter.WriteUShort(writer, 0);
            }

            // Set length
            var length = writer.BaseStream.Position - startOffset;
            writer.BaseStream.Position = lengthOffset;
            DataTypeConverter.WriteUShort(writer, (ushort)length);

            // Recover writer position
            writer.BaseStream.Position = startOffset + length;
        }


        public static Format6Subtable Deserialize(BinaryReader reader, long startOffset, ushort platformId,
            ushort encodingId)
        {
            var table = new Format6Subtable();
            table.Environments.Add(new Environment {PlatformId = platformId, EncodingId = encodingId});
            reader.BaseStream.Position = startOffset + 2*DataTypeLength.UShort;
            table.Language = DataTypeConverter.ReadUShort(reader);

            var firstCode = DataTypeConverter.ReadUShort(reader);
            var entryCount = DataTypeConverter.ReadUShort(reader);
            for (uint i = 0; i < entryCount; i++)
            {
                table.CharGlyphIdMap[firstCode + i] = DataTypeConverter.ReadUShort(reader);
            }

            return table;
        }
    }
}