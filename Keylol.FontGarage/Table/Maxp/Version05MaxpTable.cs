using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table.Maxp
{
    public class Version05MaxpTable : MaxpTable
    {
        public override string Version
        {
            get { return "0.5000"; }
        }

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public new static Version05MaxpTable Deserialize(BinaryReader reader, long startOffset)
        {
            var table = new Version05MaxpTable();
            reader.BaseStream.Position = startOffset + DataTypeLength.Fixed;

            table.NumberOfGlyphs = DataTypeConverter.ReadUShort(reader);

            return table;
        }
    }
}
