using System.IO;

namespace Keylol.FontGarage.Table.Glyf
{
    public abstract class Glyph : IOpenTypeFontSerializable, IDeepCopyable
    {
        public uint Id { get; set; }
        public abstract object DeepCopy();
        public abstract void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);
    }
}