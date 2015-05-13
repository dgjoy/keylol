using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table.Cmap
{
    public struct Environment
    {
        public ushort PlatformId { get; set; }
        public ushort EncodingId { get; set; }
    }

    public abstract class CmapSubtable : IOpenTypeFontSerializable
    {
        public List<Environment> Environments { get; set; }
        public Dictionary<uint, uint> CharGlyphIdMap { get; set; }
        public abstract ushort Format { get; }

        public abstract void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);

        protected CmapSubtable()
        {
            Environments = new List<Environment>();
            CharGlyphIdMap = new Dictionary<uint, uint>();
        }
    }
}
