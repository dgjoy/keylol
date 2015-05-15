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
            reader.BaseStream.Position = startOffset + DataTypeLength.Fixed;
            return new Version10MaxpTable
            {
                NumberOfGlyphs = DataTypeConverter.ReadUShort(reader),
                MaxPoints = DataTypeConverter.ReadUShort(reader),
                MaxContours = DataTypeConverter.ReadUShort(reader),
                MaxCompositePoints = DataTypeConverter.ReadUShort(reader),
                MaxCompositeContours = DataTypeConverter.ReadUShort(reader),
                MaxZones = DataTypeConverter.ReadUShort(reader),
                MaxTwilightPoints = DataTypeConverter.ReadUShort(reader),
                MaxStorage = DataTypeConverter.ReadUShort(reader),
                MaxFunctionDefs = DataTypeConverter.ReadUShort(reader),
                MaxInstructionDefs = DataTypeConverter.ReadUShort(reader),
                MaxStackElements = DataTypeConverter.ReadUShort(reader),
                MaxSizeOfInstructions = DataTypeConverter.ReadUShort(reader),
                MaxComponentElements = DataTypeConverter.ReadUShort(reader),
                MaxComponentDepth = DataTypeConverter.ReadUShort(reader)
            };
        }

        public override object DeepCopy()
        {
            return MemberwiseClone();
        }
    }
}