using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keylol.FontGarage.Table
{
    public class HeadTable : IOpenTypeFontTable
    {
        public string Tag
        {
            get { return "head"; }
        }

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

        public void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
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
            table.LocaTableVersion = DataTypeConverter.ReadShort(reader) == 0
                ? LocaTableVersion.Short
                : LocaTableVersion.Long;

            return table;
        }
    }
}
