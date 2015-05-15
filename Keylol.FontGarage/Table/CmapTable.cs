﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keylol.FontGarage.Table.Cmap;
using Environment = Keylol.FontGarage.Table.Cmap.Environment;

namespace Keylol.FontGarage.Table
{
    public class CmapTable : IOpenTypeFontTable
    {
        public List<CmapSubtable> Subtables;

        public CmapTable()
        {
            Version = 0;
            Subtables = new List<CmapSubtable>();
        }

        public ushort Version { get; set; }

        public string Tag
        {
            get { return "cmap"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteUShort(writer, Version);
            var environments = Subtables.SelectMany(subtable => subtable.Environments).ToList();
            DataTypeConverter.WriteUShort(writer, (ushort) environments.Count);
            var startOffsetOfCurrentEntry = writer.BaseStream.Position;
            var startOffsetOfCurrentSubtable = writer.BaseStream.Position +
                                               environments.Count*
                                               (DataTypeLength.UShort + DataTypeLength.UShort + DataTypeLength.ULong);

            foreach (var subtable in Subtables)
            {
                // 4k padding
                if (startOffsetOfCurrentSubtable%4 != 0)
                    startOffsetOfCurrentSubtable += (4 - startOffsetOfCurrentSubtable%4);

                subtable.Serialize(writer, startOffsetOfCurrentSubtable, font);

                var endOffset = writer.BaseStream.Position;

                // Write subtable info in entry list
                writer.BaseStream.Position = startOffsetOfCurrentEntry;
                foreach (var environment in subtable.Environments)
                {
                    DataTypeConverter.WriteUShort(writer, environment.PlatformId);
                    DataTypeConverter.WriteUShort(writer, environment.EncodingId);
                    DataTypeConverter.WriteULong(writer, (uint) (startOffsetOfCurrentSubtable - startOffset));
                }

                // Set offsets for next subtable
                startOffsetOfCurrentEntry = writer.BaseStream.Position;
                startOffsetOfCurrentSubtable = writer.BaseStream.Position = endOffset;
            }
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
                        PlatformId = platformIds[i]
                    });
                    continue;
                }

                var subtableStartOffset = reader.BaseStream.Position = startOffset + offsets[i];
                var format = DataTypeConverter.ReadUShort(reader);
                CmapSubtable subtable;
                switch (format)
                {
                    case 0:
                        subtable = Format0Subtable.Deserialize(reader, subtableStartOffset, platformIds[i],
                            encodingIds[i]);
                        break;

                    case 4:
                        subtable = Format4Subtable.Deserialize(reader, subtableStartOffset, platformIds[i],
                            encodingIds[i]);
                        break;

                    case 6:
                        subtable = Format6Subtable.Deserialize(reader, subtableStartOffset, platformIds[i],
                            encodingIds[i]);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                offsetSubtableMap[offsets[i]] = subtable;
                table.Subtables.Add(subtable);
            }
            return table;
        }
    }
}