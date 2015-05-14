using System.Collections.Generic;
using System.IO;

namespace Keylol.FontGarage.Table.Cmap
{
    public struct Environment
    {
        public ushort PlatformId { get; set; }
        public ushort EncodingId { get; set; }
    }

    public abstract class CmapSubtable : IOpenTypeFontSerializable
    {
        protected CmapSubtable()
        {
            Environments = new List<Environment>();
            CharGlyphIdMap = new Dictionary<uint, uint>();
        }

        public List<Environment> Environments { get; set; }
        public Dictionary<uint, uint> CharGlyphIdMap { get; set; }
        public abstract ushort Format { get; }
        public abstract void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);
    }
}