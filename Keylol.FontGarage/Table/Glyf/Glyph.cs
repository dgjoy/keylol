using System.IO;

namespace Keylol.FontGarage.Table.Glyf
{
    public abstract class Glyph : IOpenTypeFontSerializable
    {
        public uint Id { get; set; }
        public abstract void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);
    }
}