using System.IO;

namespace Keylol.FontGarage.Table
{
    public class PostTable : IOpenTypeFontTable
    {
        public string Version { get; set; }
        public string ItalicAngle { get; set; }
        public short UnderlinePosition { get; set; }
        public short UnderlineThickness { get; set; }
        public uint IsFixedPitch { get; set; }
        public uint MinMemType42 { get; set; }
        public uint MaxMemType42 { get; set; }
        public uint MinMemType1 { get; set; }
        public uint MaxMemType1 { get; set; }

        public string Tag
        {
            get { return "post"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            // Force version 3.0, we will drop any glyph name strings in this table
            DataTypeConverter.WriteFixed(writer, "3.0000");
            DataTypeConverter.WriteFixed(writer, ItalicAngle);
            DataTypeConverter.WriteShort(writer, UnderlinePosition);
            DataTypeConverter.WriteShort(writer, UnderlineThickness);
            DataTypeConverter.WriteULong(writer, IsFixedPitch);
            DataTypeConverter.WriteULong(writer, MinMemType42);
            DataTypeConverter.WriteULong(writer, MaxMemType42);
            DataTypeConverter.WriteULong(writer, MinMemType1);
            DataTypeConverter.WriteULong(writer, MaxMemType1);
        }

        public static PostTable Deserialize(BinaryReader reader, long startOffset)
        {
            reader.BaseStream.Position = startOffset;
            return new PostTable
            {
                Version = DataTypeConverter.ReadFixed(reader),
                ItalicAngle = DataTypeConverter.ReadFixed(reader),
                UnderlinePosition = DataTypeConverter.ReadShort(reader),
                UnderlineThickness = DataTypeConverter.ReadShort(reader),
                IsFixedPitch = DataTypeConverter.ReadULong(reader),
                MinMemType42 = DataTypeConverter.ReadULong(reader),
                MaxMemType42 = DataTypeConverter.ReadULong(reader),
                MinMemType1 = DataTypeConverter.ReadULong(reader),
                MaxMemType1 = DataTypeConverter.ReadULong(reader)
            };
        }
    }
}