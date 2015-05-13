using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public ushort Language { get; set; }

        private bool IsSegmentGlyphIdContiguous(uint segmentStart, uint segmentEnds)
        {
            for (var i = segmentStart; i < segmentEnds; i++)
            {
                if (CharGlyphIdMap[i] + 1 != CharGlyphIdMap[i + 1])
                    return false;
            }
            return true;
        }

        public override void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;
            DataTypeConverter.WriteUShort(writer, Format);
            var lengthOffset = writer.BaseStream.Position;
            writer.BaseStream.Position += DataTypeLength.UShort;
            DataTypeConverter.WriteUShort(writer, Language);

            CharGlyphIdMap[0xFFFF] = 0;
            var charList = CharGlyphIdMap.Keys.OrderBy(u => u).ToList();
            var segStarts = new List<uint>();
            var segEnds = new List<uint>();
            for (var i = 0; i < charList.Count; i++)
            {
                if (i == 0 || charList[i] - 1 != charList[i - 1])
                    segStarts.Add(charList[i]);
                if (i == charList.Count - 1 || charList[i] + 1 != charList[i + 1])
                    segEnds.Add(charList[i]);
            }

            var segCount = segStarts.Count;
            DataTypeConverter.WriteUShort(writer, (ushort) (segCount*2));
            {
                var pow = Math.Floor(Math.Log(segCount, 2));
                var searchRange = Math.Pow(2, pow + 1);
                DataTypeConverter.WriteUShort(writer, (ushort) searchRange);
                DataTypeConverter.WriteUShort(writer, (ushort) pow);
                DataTypeConverter.WriteUShort(writer, (ushort) (segCount*2 - searchRange));
            }

            foreach (var end in segEnds)
                DataTypeConverter.WriteUShort(writer, (ushort) end);
            DataTypeConverter.WriteUShort(writer, 0);
            foreach (var start in segStarts)
                DataTypeConverter.WriteUShort(writer, (ushort) start);
            var idDeltaOffset = writer.BaseStream.Position;
            var idRangeOffsetOffset = idDeltaOffset + segCount*DataTypeLength.Short;
            var glyphIdArrayOffset = idRangeOffsetOffset + segCount*DataTypeLength.UShort;
            for (var i = 0; i < segCount; i++)
            {
                if (IsSegmentGlyphIdContiguous(segStarts[i], segEnds[i]))
                {
                    writer.BaseStream.Position = idDeltaOffset;
                    DataTypeConverter.WriteShort(writer, (short) (CharGlyphIdMap[segStarts[i]] - segStarts[i]));

                    writer.BaseStream.Position = idRangeOffsetOffset;
                    DataTypeConverter.WriteUShort(writer, 0);
                }
                else
                {
                    writer.BaseStream.Position = idDeltaOffset;
                    DataTypeConverter.WriteShort(writer, 0);

                    writer.BaseStream.Position = idRangeOffsetOffset;
                    DataTypeConverter.WriteUShort(writer, (ushort) (glyphIdArrayOffset - idRangeOffsetOffset));

                    writer.BaseStream.Position = glyphIdArrayOffset;
                    for (var j = segStarts[i]; j <= segEnds[i]; j++)
                        DataTypeConverter.WriteUShort(writer, (ushort) CharGlyphIdMap[j]);
                    glyphIdArrayOffset = writer.BaseStream.Position;
                }
                idDeltaOffset += DataTypeLength.Short;
                idRangeOffsetOffset += DataTypeLength.UShort;
            }

            // Set length
            var length = glyphIdArrayOffset - startOffset;
            writer.BaseStream.Position = lengthOffset;
            DataTypeConverter.WriteUShort(writer, (ushort) length);

            // Recover writer position
            writer.BaseStream.Position = startOffset + length;
        }


        public static Format4Subtable Deserialize(BinaryReader reader, long startOffset, ushort platformId,
            ushort encodingId)
        {
            var table = new Format4Subtable();
            table.Environments.Add(new Environment {PlatformId = platformId, EncodingId = encodingId});
            reader.BaseStream.Position = startOffset + 2*DataTypeLength.UShort;
            table.Language = DataTypeConverter.ReadUShort(reader);

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
                        var glyphIdRaw = DataTypeConverter.ReadUShort(reader);
                        if (glyphIdRaw != 0)
                            table.CharGlyphIdMap[codePoint] = (ushort) ((glyphIdRaw + idDeltas[i])%65536);
                    }
                    if (codePoint == ushort.MaxValue)
                        break;
                }
            }

            return table;
        }
    }
}