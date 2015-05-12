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

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public static Format6Subtable Deserialize(BinaryReader reader, long startOffset, ushort platformId,
            ushort encodingId)
        {
            var table = new Format6Subtable();
            table.Environments.Add(new Environment {PlatformId = platformId, EncodingId = encodingId});
            reader.BaseStream.Position = startOffset + 3*DataTypeLength.UShort;

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