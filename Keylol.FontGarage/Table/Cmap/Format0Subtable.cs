using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table.Cmap
{
    public class Format0Subtable : CmapSubtable
    {
        public override ushort Format
        {
            get { return 0; }
        }

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public static Format0Subtable Deserialize(BinaryReader reader, long startOffset, ushort platformId,
            ushort encodingId)
        {
            var table = new Format0Subtable();
            table.Environments.Add(new Environment {EncodingId = encodingId, PlatformId = platformId});
            reader.BaseStream.Position = startOffset + 3*DataTypeLength.UShort;

            for (uint i = 0; i < 256; i++)
            {
                table.CharGlyphIdMap[i] = reader.ReadByte();
            }

            return table;
        }
    }
}
