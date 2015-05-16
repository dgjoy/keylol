using System.Collections.Generic;
using System.IO;

namespace Keylol.FontGarage.Table
{
    public class SerializationInfo
    {
        public ushort NumberOfHMetrics { get; set; }
        public Dictionary<uint, uint> GlyphOffsets { get; set; }
        public uint GlyfTableLength { get; set; }
        public ushort NumberOfGlyphs { get; set; }
        public LocaTableVersion LocaTableVersion { get; set; }

        public SerializationInfo()
        {
            GlyphOffsets = new Dictionary<uint, uint>();
        }
    }

    public interface IOpenTypeFontSerializable
    {
        void Serialize(BinaryWriter writer, long startOffset, SerializationInfo additionalInfo);
    }
}