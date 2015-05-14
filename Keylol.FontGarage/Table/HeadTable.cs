using System;
using System.IO;

namespace Keylol.FontGarage.Table
{
    public class HeadTable : IOpenTypeFontTable
    {
        public string Version { get; set; }
        public string FontRevision { get; set; }
        public ushort Flags { get; set; }
        public ushort UnitsPerEm { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
        public short XMin { get; set; }
        public short YMin { get; set; }
        public short XMax { get; set; }
        public short YMax { get; set; }
        public ushort MacStyle { get; set; }
        public ushort LowestRecPpem { get; set; }
        public LocaTableVersion LocaTableVersion { get; set; }

        public string Tag
        {
            get { return "head"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteFixed(writer, Version);
            DataTypeConverter.WriteFixed(writer, FontRevision);
            DataTypeConverter.WriteULong(writer, 0);
            DataTypeConverter.WriteULong(writer, 0x5F0F3CF5u);
            DataTypeConverter.WriteUShort(writer, Flags);
            DataTypeConverter.WriteUShort(writer, UnitsPerEm);
            DataTypeConverter.WriteLongDateTime(writer, CreateTime);
            DataTypeConverter.WriteLongDateTime(writer, ModifyTime);
            DataTypeConverter.WriteShort(writer, XMin);
            DataTypeConverter.WriteShort(writer, YMin);
            DataTypeConverter.WriteShort(writer, XMax);
            DataTypeConverter.WriteShort(writer, YMax);
            DataTypeConverter.WriteUShort(writer, MacStyle);
            DataTypeConverter.WriteUShort(writer, LowestRecPpem);
            DataTypeConverter.WriteShort(writer, 2);
            DataTypeConverter.WriteShort(writer, (short) LocaTableVersion);
            DataTypeConverter.WriteShort(writer, 0);
        }

        public static HeadTable Deserialize(BinaryReader reader, long startOffset)
        {
            var table = new HeadTable();
            reader.BaseStream.Position = startOffset;

            table.Version = DataTypeConverter.ReadFixed(reader);
            table.FontRevision = DataTypeConverter.ReadFixed(reader);
            reader.BaseStream.Position += 2*DataTypeLength.ULong;
            table.Flags = DataTypeConverter.ReadUShort(reader);
            table.UnitsPerEm = DataTypeConverter.ReadUShort(reader);
            table.CreateTime = DataTypeConverter.ReadLongDateTime(reader);
            table.ModifyTime = DataTypeConverter.ReadLongDateTime(reader);
            table.XMin = DataTypeConverter.ReadShort(reader);
            table.YMin = DataTypeConverter.ReadShort(reader);
            table.XMax = DataTypeConverter.ReadShort(reader);
            table.YMax = DataTypeConverter.ReadShort(reader);
            table.MacStyle = DataTypeConverter.ReadUShort(reader);
            table.LowestRecPpem = DataTypeConverter.ReadUShort(reader);
            reader.BaseStream.Position += DataTypeLength.Short;
            table.LocaTableVersion = (LocaTableVersion) DataTypeConverter.ReadShort(reader);

            return table;
        }
    }
}