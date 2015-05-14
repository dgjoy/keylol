using System.IO;

namespace Keylol.FontGarage.Table
{
    public interface IOpenTypeFontSerializable
    {
        void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);
    }
}