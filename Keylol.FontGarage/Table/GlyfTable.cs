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

        public void Serialize(BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public static GlyfTable Deserialize(BinaryReader reader, long startOffset, uint length, LocaTable locaTable)
        {
            var table = new GlyfTable();
            reader.BaseStream.Position = startOffset;
            var glyphOffsets = locaTable.GlyphOffsets.Where(u => u != null).Select(u => u.Value).ToList();

            for (var i = 0; i < glyphOffsets.Count; i++)
            {

                // Peek number of contours
                var glyphStartOffset = reader.BaseStream.Position = glyphOffsets[i];
                var numberOfContours = DataTypeConverter.ReadShort(reader);
                if (numberOfContours >= 0)
                {
                    uint nextGlyphStartOffset;
                    if (i == glyphOffsets.Count - 1)
                    {
                        nextGlyphStartOffset = (uint) (startOffset + length);
                    }
                    else
                    {
                        nextGlyphStartOffset = glyphOffsets[i + 1];
                    }
                    reader.BaseStream.Position = nextGlyphStartOffset - 1;
                    // Skip padded zero
                    while (reader.ReadByte() == 0)
                        reader.BaseStream.Position -= 2;
                    table.Glyphs.Add(SimpleGlyph.Deserialize(reader, glyphStartOffset,
                        (uint)(reader.BaseStream.Position - glyphStartOffset)));
                }
                else
                {
                    table.Glyphs.Add(CompositeGlyph.Deserialize(reader, glyphStartOffset));
                }
            }

            return table;
        }

        public GlyfTable()
        {
            Glyphs = new List<Glyph>();
        }
    }
}