using System;
using System.IO;
using Keylol.FontGarage.Table.Maxp;

namespace Keylol.FontGarage.Table
{
    public abstract class MaxpTable : IOpenTypeFontTable
    {
        public abstract string Version { get; }
        public ushort NumberOfGlyphs { get; internal set; }

        public string Tag
        {
            get { return "maxp"; }
        }

        public abstract void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font);
        public abstract object DeepCopy();

        public static MaxpTable Deserialize(BinaryReader reader, long startOffset)
        {
            reader.BaseStream.Position = startOffset;
            var version = DataTypeConverter.ReadFixed(reader);
            switch (version)
            {
                case "0.5000":
                    return Version05MaxpTable.Deserialize(reader, startOffset);

                case "1.0000":
                    return Version10MaxpTable.Deserialize(reader, startOffset);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}