using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keylol.FontGarage.Table.Glyf;

namespace Keylol.FontGarage.Table
{
    public class GlyfTable : IOpenTypeFontTable
    {
        public GlyfTable()
        {
            Glyphs = new List<Glyph>();
        }

        public List<Glyph> Glyphs { get; set; }

        public string Tag
        {
            get { return "glyf"; }
        }

        public void Serialize(BinaryWriter writer, long startOffset, SerializationInfo additionalInfo)
        {
            writer.BaseStream.Position = startOffset;

            foreach (var glyph in Glyphs)
            {
                // 4byte padding
                if (writer.BaseStream.Position%4 != 0)
                    writer.BaseStream.Position += (4 - writer.BaseStream.Position%4);

                additionalInfo.GlyphOffsets[glyph.Id] = (uint) (writer.BaseStream.Position - startOffset);
                glyph.Serialize(writer, writer.BaseStream.Position, additionalInfo);
            }

            additionalInfo.GlyfTableLength = (uint) (writer.BaseStream.Position - startOffset);
        }

        public object DeepCopy()
        {
            var newTable = (GlyfTable) MemberwiseClone();
            newTable.Glyphs = Glyphs.Select(glyph => (Glyph) glyph.DeepCopy()).ToList();
            return newTable;
        }

        public static GlyfTable Deserialize(BinaryReader reader, long startOffset, uint length, LocaTable locaTable)
        {
            var table = new GlyfTable();
            reader.BaseStream.Position = startOffset;
            var glyphOffsets =
                locaTable.GlyphOffsets.Select((u, i) => new {GlyphId = i, Offset = u})
                    .Where(glyph => glyph.Offset != null)
                    .Select(pair => new {pair.GlyphId, Offset = pair.Offset.Value}).ToList();

            for (var i = 0; i < glyphOffsets.Count; i++)
            {
                Glyph glyphToAdd;

                // Peek number of contours
                var glyphStartOffset = reader.BaseStream.Position = glyphOffsets[i].Offset + startOffset;
                var numberOfContours = DataTypeConverter.ReadShort(reader);
                if (numberOfContours >= 0)
                {
                    uint nextGlyphStartOffset;
                    if (i == glyphOffsets.Count - 1)
                        nextGlyphStartOffset = (uint) (startOffset + length);
                    else
                        nextGlyphStartOffset = (uint) (glyphOffsets[i + 1].Offset + startOffset);

                    // TODO: Remove padded zeros

                    glyphToAdd = SimpleGlyph.Deserialize(reader, glyphStartOffset,
                        (uint) (nextGlyphStartOffset - glyphStartOffset));
                }
                else
                    glyphToAdd = CompositeGlyph.Deserialize(reader, glyphStartOffset);
                glyphToAdd.Id = (uint) glyphOffsets[i].GlyphId;
                table.Glyphs.Add(glyphToAdd);
            }

            return table;
        }
    }
}