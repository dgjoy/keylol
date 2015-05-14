using System.IO;

namespace Keylol.FontGarage.Table.Maxp
{
    public class Version10MaxpTable : MaxpTable
    {
        public override string Version
        {
            get { return "1.0000"; }
        }

        public ushort MaxPoints { get; set; }
        public ushort MaxContours { get; set; }
        public ushort MaxCompositePoints { get; set; }
        public ushort MaxCompositeContours { get; set; }
        public ushort MaxZones { get; set; }
        public ushort MaxTwilightPoints { get; set; }
        public ushort MaxStorage { get; set; }
        public ushort MaxFunctionDefs { get; set; }
        public ushort MaxInstructionDefs { get; set; }
        public ushort MaxStackElements { get; set; }
        public ushort MaxSizeOfInstructions { get; set; }
        public ushort MaxComponentElements { get; set; }
        public ushort MaxComponentDepth { get; set; }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteFixed(writer, Version);
            DataTypeConverter.WriteUShort(writer, NumberOfGlyphs);
            DataTypeConverter.WriteUShort(writer, MaxPoints);
            DataTypeConverter.WriteUShort(writer, MaxContours);
            DataTypeConverter.WriteUShort(writer, MaxCompositePoints);
            DataTypeConverter.WriteUShort(writer, MaxCompositeContours);
            DataTypeConverter.WriteUShort(writer, MaxZones);
            DataTypeConverter.WriteUShort(writer, MaxTwilightPoints);
            DataTypeConverter.WriteUShort(writer, MaxStorage);
            DataTypeConverter.WriteUShort(writer, MaxFunctionDefs);
            DataTypeConverter.WriteUShort(writer, MaxInstructionDefs);
            DataTypeConverter.WriteUShort(writer, MaxStackElements);
            DataTypeConverter.WriteUShort(writer, MaxSizeOfInstructions);
            DataTypeConverter.WriteUShort(writer, MaxComponentElements);
            DataTypeConverter.WriteUShort(writer, MaxComponentDepth);
        }

        public new static Version10MaxpTable Deserialize(BinaryReader reader, long startOffset)
        {
            var table = new Version10MaxpTable();
            reader.BaseStream.Position = startOffset + DataTypeLength.Fixed;

            table.NumberOfGlyphs = DataTypeConverter.ReadUShort(reader);
            table.MaxPoints = DataTypeConverter.ReadUShort(reader);
            table.MaxContours = DataTypeConverter.ReadUShort(reader);
            table.MaxCompositePoints = DataTypeConverter.ReadUShort(reader);
            table.MaxCompositeContours = DataTypeConverter.ReadUShort(reader);
            table.MaxZones = DataTypeConverter.ReadUShort(reader);
            table.MaxTwilightPoints = DataTypeConverter.ReadUShort(reader);
            table.MaxStorage = DataTypeConverter.ReadUShort(reader);
            table.MaxFunctionDefs = DataTypeConverter.ReadUShort(reader);
            table.MaxInstructionDefs = DataTypeConverter.ReadUShort(reader);
            table.MaxStackElements = DataTypeConverter.ReadUShort(reader);
            table.MaxSizeOfInstructions = DataTypeConverter.ReadUShort(reader);
            table.MaxComponentElements = DataTypeConverter.ReadUShort(reader);
            table.MaxComponentDepth = DataTypeConverter.ReadUShort(reader);

            return table;
        }
    }
}