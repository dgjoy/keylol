using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Keylol.FontGarage.Table
{
    public struct HorizontalMetric
    {
        public ushort AdvanceWidth { get; set; }
        public short LeftSideBearing { get; set; }
    }

    public class HmtxTable : IOpenTypeFontTable
    {
        public HmtxTable()
        {
            HorizontalMetrics = new List<HorizontalMetric>();
        }

        public List<HorizontalMetric> HorizontalMetrics { get; set; }

        public string Tag
        {
            get { return "hmtx"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, SerializationInfo additionalInfo)
        {
            writer.BaseStream.Position = startOffset;
            var wall = 0;
            for (var i = HorizontalMetrics.Count - 1; i > 0; i--)
            {
                if (HorizontalMetrics[i].AdvanceWidth == HorizontalMetrics[i - 1].AdvanceWidth) continue;
                wall = i;
                break;
            }
            for (var i = 0; i < HorizontalMetrics.Count; i++)
            {
                if (i <= wall)
                {
                    DataTypeConverter.WriteUShort(writer, HorizontalMetrics[i].AdvanceWidth);
                    DataTypeConverter.WriteShort(writer, HorizontalMetrics[i].LeftSideBearing);
                }
                else
                    DataTypeConverter.WriteShort(writer, HorizontalMetrics[i].LeftSideBearing);
            }

            additionalInfo.NumberOfHMetrics = (ushort) (wall + 1);
        }

        public object DeepCopy()
        {
            var newTable = (HmtxTable) MemberwiseClone();
            newTable.HorizontalMetrics = HorizontalMetrics.ToList();
            return newTable;
        }

        public static HmtxTable Deserialize(BinaryReader reader, long startOffset, ushort numberOfHorizontalMetrics,
            ushort numberOfGlyphs)
        {
            var table = new HmtxTable();
            reader.BaseStream.Position = startOffset;

            table.HorizontalMetrics.AddRange(
                Enumerable.Range(0, numberOfHorizontalMetrics).Select(i => new HorizontalMetric
                {
                    AdvanceWidth = DataTypeConverter.ReadUShort(reader),
                    LeftSideBearing = DataTypeConverter.ReadShort(reader)
                }));

            var remainingAdvanceWidth = table.HorizontalMetrics.Last().AdvanceWidth;
            table.HorizontalMetrics.AddRange(
                Enumerable.Range(0, numberOfGlyphs - numberOfHorizontalMetrics).Select(i => new HorizontalMetric
                {
                    AdvanceWidth = remainingAdvanceWidth,
                    LeftSideBearing = DataTypeConverter.ReadShort(reader)
                }));

            return table;
        }
    }
}