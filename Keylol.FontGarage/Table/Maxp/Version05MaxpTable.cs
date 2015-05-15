using System.IO;

namespace Keylol.FontGarage.Table.Maxp
{
    public class Version05MaxpTable : MaxpTable
    {
        public override string Version
        {
            get { return "0.5000"; }
        }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteFixed(writer, Version);
            DataTypeConverter.WriteUShort(writer, NumberOfGlyphs);
        }

        public new static Version05MaxpTable Deserialize(BinaryReader reader, long startOffset)
        {
            reader.BaseStream.Position = startOffset + DataTypeLength.Fixed;
            return new Version05MaxpTable {NumberOfGlyphs = DataTypeConverter.ReadUShort(reader)};
        }

        public override object DeepCopy()
        {
            return MemberwiseClone();
        }
    }
}