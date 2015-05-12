using System;
using System.IO;
using System.Linq;

namespace Keylol.FontGarage.Table.Cmap
{
    public class Format4Subtable : CmapSubtable
    {
        public override ushort Format
        {
            get { return 4; }
        }

        public override void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public static Format4Subtable Deserialize(BinaryReader reader, long startOffset, ushort platformId,
            ushort encodingId)
        {
            var table = new Format4Subtable();
            table.Environments.Add(new Environment {PlatformId = platformId, EncodingId = encodingId});
            reader.BaseStream.Position = startOffset + 3*DataTypeLength.UShort;

            var segmentCount = DataTypeConverter.ReadUShort(reader)/2;
            reader.BaseStream.Position += 3*DataTypeLength.UShort;
            var segmentEnds =
                Enumerable.Range(0, segmentCount).Select(i => DataTypeConverter.ReadUShort(reader)).ToArray();
            reader.BaseStream.Position += DataTypeLength.UShort;
            var segmentStarts =
                Enumerable.Range(0, segmentCount).Select(i => DataTypeConverter.ReadUShort(reader)).ToArray();
            var idDeltas =
                Enumerable.Range(0, segmentCount).Select(i => DataTypeConverter.ReadShort(reader)).ToArray();
            var startOfIdRangeOffsets = reader.BaseStream.Position;
            var idRangeOffsets =
                Enumerable.Range(0, segmentCount).Select(i => DataTypeConverter.ReadUShort(reader)).ToArray();
            for (var i = 0; i < segmentCount; i++)
            {
                for (var codePoint = segmentStarts[i]; codePoint <= segmentEnds[i]; codePoint++)
                {
                    if (idRangeOffsets[i] == 0)
                        table.CharGlyphIdMap[codePoint] = (ushort) ((idDeltas[i] + codePoint)%65536);
                    else
                    {
                        reader.BaseStream.Position = idRangeOffsets[i] + 2*(codePoint - segmentStarts[i]) +
                                                     startOfIdRangeOffsets + DataTypeLength.UShort*i;
                        table.CharGlyphIdMap[codePoint] =
                            (ushort) ((DataTypeConverter.ReadUShort(reader) + idDeltas[i])%65536);
                    }
                    if (codePoint == ushort.MaxValue)
                        break;
                }
            }

            return table;
        }
    }
}