using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.FontGarage.Table;
using Keylol.FontGarage.Table.Glyf;

namespace Keylol.FontGarage
{
    public class FontGarage
    {
        public static void Subset(OpenTypeFont font, ICollection<uint> characters)
        {
            // Control characters and space are recommended
            for (uint i = 0; i <= 0x20; i++)
            {
                if (!characters.Contains(i))
                    characters.Add(i);
            }

            var cmapTable = font.Get<CmapTable>();
            cmapTable.Subtables.ForEach(subtable =>
            {
                var newMap = new Dictionary<uint, uint>();
                foreach (var character in characters.Where(character => subtable.CharGlyphIdMap.ContainsKey(character)))
                    newMap[character] = subtable.CharGlyphIdMap[character];
                subtable.CharGlyphIdMap = newMap;
            });
            cmapTable.Subtables.RemoveAll(subtable => subtable.CharGlyphIdMap.Count == 0); // Remove empty subtable

            var glyfTable = font.Get<GlyfTable>();
            var glyphIds =  cmapTable.Subtables.SelectMany(subtable => subtable.CharGlyphIdMap.Values).Distinct().ToList();
            for (uint i = 0; i < 4; i++) // First four glyphs are recommended
            {
                if (!glyphIds.Contains(i))
                    glyphIds.Add(i);
            }
            var glyphs = glyfTable.Glyphs.Where(glyph => glyphIds.Contains(glyph.Id)).ToList();
            var components = glyphs.OfType<CompositeGlyph>().SelectMany(glyph => glyph.Components).ToList();
            glyphIds.AddRange(components.Select(component => (uint) component.GlyphId));

            // Remove duplicated glyphs and sort
            glyphIds = glyphIds.Distinct().ToList();
            glyphIds.Sort();
            glyphs = glyfTable.Glyphs.Where(glyph => glyphIds.Contains(glyph.Id)).ToList();

            // Remapping glyph id
            var oldNewGlyphIdMap = glyphIds.Select((id, i) => new {NewId = (uint) i, OldId = id})
                .ToDictionary(arg => arg.OldId, arg => arg.NewId);
            cmapTable.Subtables.ForEach(subtable =>
            {
                var newCharGlyphIdMap = new Dictionary<uint, uint>();
                foreach (var pair in subtable.CharGlyphIdMap)
                {
                    newCharGlyphIdMap[pair.Key] = oldNewGlyphIdMap[pair.Value];
                }
                subtable.CharGlyphIdMap = newCharGlyphIdMap;
            });
            foreach (var glyph in glyphs)
            {
                glyph.Id = oldNewGlyphIdMap[glyph.Id];
            }
            foreach (var component in components)
            {
                component.GlyphId = (ushort) oldNewGlyphIdMap[component.GlyphId];
            }
            glyfTable.Glyphs = glyphs;
            
            // Update loca table
            var locaTable = font.Get<LocaTable>();
            locaTable.GlyphOffsets = new List<uint?>(glyphIds.Count);
            locaTable.GlyphOffsets.AddRange(Enumerable.Repeat<uint?>(null, glyphIds.Count));

            // Update maxp table
            var maxpTable = font.Get<MaxpTable>();
            maxpTable.NumberOfGlyphs = (ushort) glyphIds.Count;
        }
    }
}