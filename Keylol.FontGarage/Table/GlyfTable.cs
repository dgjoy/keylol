using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.FontGarage.Table.Glyf;

namespace Keylol.FontGarage.Table
{
    public class GlyfTable : IOpenTypeFontTable
    {
        public string Tag
        {
            get { return "glyf"; }
        }

        public List<Glyph> Glyphs { get; set; }

        public void Serialize(BinaryWriter writer, long startOffset, OpenTypeFont font)
        {
            writer.BaseStream.Position = startOffset;

            // Reset loca table
            var locaTable = font.Get<LocaTable>();
            for (var i = 0; i < locaTable.GlyphOffsets.Count; i++)
                locaTable.GlyphOffsets[i] = null;

            foreach (var glyph in Glyphs)
            {
                // 4k padding
                if (writer.BaseStream.Position%4 != 0)
                    writer.BaseStream.Position += (4 - writer.BaseStream.Position%4);

                locaTable.GlyphOffsets[(int) glyph.Id] = (uint) (writer.BaseStream.Position - startOffset);
                glyph.Serialize(writer, writer.BaseStream.Position, font);
            }

            if (locaTable.GlyphOffsets.Count > 0 && locaTable.GlyphOffsets[locaTable.GlyphOffsets.Count - 1] == null)
                locaTable.GlyphOffsets[locaTable.GlyphOffsets.Count - 1] =
                    (uint) (writer.BaseStream.Position - startOffset);
        }

        public static GlyfTable Deserialize(BinaryReader reader, long startOffset, uint length, LocaTable locaTable)
        {
            var table = new GlyfTable();
            reader.BaseStream.Position = startOffset;

            var glyphOffsetToIdLookup =
                locaTable.GlyphOffsets.Select((u, i) => new {Value = u, Index = i})
                    .Where(arg => arg.Value != null)
                    .ToDictionary(arg => arg.Value.Value, arg => arg.Index);
            var glyphOffsets = locaTable.GlyphOffsets.Where(u => u != null).Select(u => u.Value).ToList();

            for (var i = 0; i < glyphOffsets.Count; i++)
            {
                Glyph glyphToAdd;

                // Peek number of contours
                var glyphStartOffset = reader.BaseStream.Position = glyphOffsets[i] + startOffset;
                var numberOfContours = DataTypeConverter.ReadShort(reader);
                if (numberOfContours >= 0)
                {
                    uint nextGlyphStartOffset;
                    if (i == glyphOffsets.Count - 1)
                        nextGlyphStartOffset = (uint) (startOffset + length);
                    else
                        nextGlyphStartOffset = (uint) (glyphOffsets[i + 1] + startOffset);

                    // TODO: Remove padded zeros

                    glyphToAdd = SimpleGlyph.Deserialize(reader, glyphStartOffset,
                        (uint) (nextGlyphStartOffset - glyphStartOffset));
                }
                else
                    glyphToAdd = CompositeGlyph.Deserialize(reader, glyphStartOffset);
                glyphToAdd.Id = (uint) glyphOffsetToIdLookup[glyphOffsets[i]];
                table.Glyphs.Add(glyphToAdd);
            }

            return table;
        }

        public GlyfTable()
        {
            Glyphs = new List<Glyph>();
        }
    }
}