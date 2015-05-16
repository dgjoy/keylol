using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keylol.FontGarage.Table.Cmap;
using Environment = Keylol.FontGarage.Table.Cmap.Environment;

namespace Keylol.FontGarage.Table
{
    public class CmapTable : IOpenTypeFontTable
    {
        public CmapTable()
        {
            Version = 0;
            Subtables = new List<CmapSubtable>();
        }

        public List<CmapSubtable> Subtables { get; set; }
        public ushort Version { get; set; }

        public string Tag
        {
            get { return "cmap"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, SerializationInfo additionalInfo)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteUShort(writer, Version);
            var environments =
                Subtables.SelectMany(subtable => subtable.Environments)
                    .OrderBy(environment => environment.PlatformId)
                    .ThenBy(environment => environment.EncodingId)
                    .ToList();
            DataTypeConverter.WriteUShort(writer, (ushort) environments.Count);
            var startOffsetOfEnvironments = writer.BaseStream.Position;
            writer.BaseStream.Position += environments.Count*
                                          (DataTypeLength.UShort + DataTypeLength.UShort + DataTypeLength.ULong);

            foreach (var subtable in Subtables)
            {
                // 4k padding
                if (writer.BaseStream.Position%4 != 0)
                    writer.BaseStream.Position += (4 - writer.BaseStream.Position%4);

                subtable.Environments.ForEach(
                    environment => environment.SubtableOffset = (uint) (writer.BaseStream.Position - startOffset));

                subtable.Serialize(writer, writer.BaseStream.Position, additionalInfo);
            }

            var endOffset = writer.BaseStream.Position;
            writer.BaseStream.Position = startOffsetOfEnvironments;
            environments.ForEach(environment =>
            {
                DataTypeConverter.WriteUShort(writer, environment.PlatformId);
                DataTypeConverter.WriteUShort(writer, environment.EncodingId);
                DataTypeConverter.WriteULong(writer, (ushort) environment.SubtableOffset);
            });

            // Restore writer position
            writer.BaseStream.Position = endOffset;
        }

        public object DeepCopy()
        {
            var newTable = (CmapTable) MemberwiseClone();
            newTable.Subtables = Subtables.Select(subtable => (CmapSubtable) subtable.DeepCopy()).ToList();
            return newTable;
        }

        public static CmapTable Deserialize(BinaryReader reader, long startOffset)
        {
            var table = new CmapTable();
            reader.BaseStream.Position = startOffset;

            table.Version = DataTypeConverter.ReadUShort(reader);

            var numberOfSubtables = DataTypeConverter.ReadUShort(reader);
            var platformIds = new ushort[numberOfSubtables];
            var encodingIds = new ushort[numberOfSubtables];
            var offsets = new uint[numberOfSubtables];
            for (var i = 0; i < numberOfSubtables; i++)
            {
                platformIds[i] = DataTypeConverter.ReadUShort(reader);
                encodingIds[i] = DataTypeConverter.ReadUShort(reader);
                offsets[i] = DataTypeConverter.ReadULong(reader);
            }
            var offsetSubtableMap = new Dictionary<uint, CmapSubtable>();
            for (var i = 0; i < numberOfSubtables; i++)
            {
                if (offsetSubtableMap.ContainsKey(offsets[i])) // Multiple map
                {
                    offsetSubtableMap[offsets[i]].Environments.Add(new Environment
                    {
                        EncodingId = encodingIds[i],
                        PlatformId = platformIds[i],
                        SubtableOffset = offsets[i]
                    });
                    continue;
                }

                var subtableStartOffset = reader.BaseStream.Position = startOffset + offsets[i];
                var format = DataTypeConverter.ReadUShort(reader);
                CmapSubtable subtable;
                switch (format)
                {
                    case 0:
                        subtable = Format0Subtable.Deserialize(reader, subtableStartOffset);
                        break;

                    case 4:
                        subtable = Format4Subtable.Deserialize(reader, subtableStartOffset);
                        break;

                    case 6:
                        subtable = Format6Subtable.Deserialize(reader, subtableStartOffset);
                        break;

                    case 12:
                        subtable = Format12Subtable.Deserialize(reader, subtableStartOffset);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                subtable.Environments.Add(new Environment
                {
                    PlatformId = platformIds[i],
                    EncodingId = encodingIds[i],
                    SubtableOffset = offsets[i]
                });
                offsetSubtableMap[offsets[i]] = subtable;
                table.Subtables.Add(subtable);
            }
            return table;
        }
    }
}