using System.IO;

namespace Keylol.FontGarage.Table
{
    public class HheaTable : IOpenTypeFontTable
    {
        public string Version { get; set; }
        public short Ascender { get; set; }
        public short Descender { get; set; }
        public short LineGap { get; set; }
        public ushort AdvanceWidthMax { get; set; }
        public short MinLeftSideBearing { get; set; }
        public short MinRightSideBearing { get; set; }
        public short XMaxExtent { get; set; }
        public short CaretSlopeRise { get; set; }
        public short CaretSlopeRun { get; set; }
        public short CaretOffset { get; set; }
        public short MetricDataFormat { get; set; }
        public ushort NumberOfHMetrics { get; internal set; }

        public string Tag
        {
            get { return "hhea"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteFixed(writer, Version);
            DataTypeConverter.WriteFword(writer, Ascender);
            DataTypeConverter.WriteFword(writer, Descender);
            DataTypeConverter.WriteFword(writer, LineGap);
            DataTypeConverter.WriteUFword(writer, AdvanceWidthMax);
            DataTypeConverter.WriteFword(writer, MinLeftSideBearing);
            DataTypeConverter.WriteFword(writer, MinRightSideBearing);
            DataTypeConverter.WriteFword(writer, XMaxExtent);
            DataTypeConverter.WriteShort(writer, CaretSlopeRise);
            DataTypeConverter.WriteShort(writer, CaretSlopeRun);
            DataTypeConverter.WriteShort(writer, CaretOffset);
            DataTypeConverter.WriteShort(writer, 0);
            DataTypeConverter.WriteShort(writer, 0);
            DataTypeConverter.WriteShort(writer, 0);
            DataTypeConverter.WriteShort(writer, 0);
            DataTypeConverter.WriteShort(writer, MetricDataFormat);
            DataTypeConverter.WriteUShort(writer, NumberOfHMetrics);
        }

        public static HheaTable Deserialize(BinaryReader reader, long startOffset)
        {
            var table = new HheaTable();
            reader.BaseStream.Position = startOffset;

            table.Version = DataTypeConverter.ReadFixed(reader);
            table.Ascender = DataTypeConverter.ReadFword(reader);
            table.Descender = DataTypeConverter.ReadFword(reader);
            table.LineGap = DataTypeConverter.ReadFword(reader);
            table.AdvanceWidthMax = DataTypeConverter.ReadUFword(reader);
            table.MinLeftSideBearing = DataTypeConverter.ReadFword(reader);
            table.MinRightSideBearing = DataTypeConverter.ReadFword(reader);
            table.XMaxExtent = DataTypeConverter.ReadFword(reader);
            table.CaretSlopeRise = DataTypeConverter.ReadShort(reader);
            table.CaretSlopeRun = DataTypeConverter.ReadShort(reader);
            table.CaretOffset = DataTypeConverter.ReadShort(reader);
            reader.BaseStream.Position += DataTypeLength.Short*4;
            table.MetricDataFormat = DataTypeConverter.ReadShort(reader);
            table.NumberOfHMetrics = DataTypeConverter.ReadUShort(reader);

            return table;
        }
    }
}