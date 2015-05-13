using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table.Glyf
{
    public abstract class Glyph : IOpenTypeFontSerializable
    {
        public uint Id { get; set; }

        public abstract void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);
    }
}
