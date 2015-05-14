using System.IO;

namespace Keylol.FontGarage.Table.Cmap
{
    public class Format0Subtable : CmapSubtable
    {
        public override ushort Format
        {
            get { return 0; }
        }

        public ushort Language { get; set; }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteUShort(writer, Format);
            var lengthOffset = writer.BaseStream.Position;
            writer.BaseStream.Position += DataTypeLength.UShort;
            DataTypeConverter.WriteUShort(writer, Language);
            for (uint i = 0; i < 256; i++)
            {
                if (CharGlyphIdMap.ContainsKey(i))
                    writer.Write((byte) CharGlyphIdMap[i]);
                else
                    writer.Write((byte) 0);
            }

            // Set length
            var length = writer.BaseStream.Position - startOffset;
            writer.BaseStream.Position = lengthOffset;
            DataTypeConverter.WriteUShort(writer, (ushort) length);

            // Recover writer position
            writer.BaseStream.Position = startOffset + length;
        }

        public static Format0Subtable Deserialize(BinaryReader reader, long startOffset, ushort platformId,
            ushort encodingId)
        {
            var table = new Format0Subtable();
            table.Environments.Add(new Environment {EncodingId = encodingId, PlatformId = platformId});
            reader.BaseStream.Position = startOffset + 2*DataTypeLength.UShort;
            table.Language = DataTypeConverter.ReadUShort(reader);

            for (uint i = 0; i < 256; i++)
                table.CharGlyphIdMap[i] = reader.ReadByte();

            return table;
        }
    }
}