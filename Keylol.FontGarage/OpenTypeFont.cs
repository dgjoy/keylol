using System;
using System.Collections.Generic;
using System.Linq;
using Keylol.FontGarage.Table;
using Keylol.FontGarage.Table.Glyf;

namespace Keylol.FontGarage
{
    public class OpenTypeFont
    {
        public OpenTypeFont(string sfntVersion) : this()
        {
            SfntVersion = sfntVersion;
        }

        public OpenTypeFont()
        {
            Tables = new List<IOpenTypeFontTable>();
        }

        public string SfntVersion { get; set; }
        public List<IOpenTypeFontTable> Tables { get; set; }

        private static IEnumerable<uint> GlyphIdsTransitiveClosure(ISet<uint> glyphIds, GlyfTable glyfTable,
            int depth = 0)
        {
            if (depth > 10)
                throw new ArgumentOutOfRangeException("depth", "Too many composite redirects.");

            var compositeGlyphs =
                glyfTable.Glyphs.OfType<CompositeGlyph>().Where(glyph => glyphIds.Contains(glyph.Id)).ToList();
            if (!compositeGlyphs.Any())
                return glyphIds;

            glyphIds.UnionWith(
                GlyphIdsTransitiveClosure(
                    new HashSet<uint>(
                        compositeGlyphs.SelectMany(glyph => glyph.Components)
                            .Select(component => (uint) component.GlyphId)), glyfTable, depth + 1));
            return glyphIds;
        }

        /// <summary>
        ///     Get a OpenType table of a specified type. You cannot use this to get BinaryDataTable.
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <returns>The specified OpenType font table.</returns>
        public T Get<T>()
        {
            return Tables.OfType<T>().Single();
        }

        public OpenTypeFont DeepCopy()
        {
            var font = (OpenTypeFont) MemberwiseClone();
            font.Tables = Tables.Select(table => (IOpenTypeFontTable) table.DeepCopy()).ToList();
            return font;
        }

        /// <summary>
        ///     Subset operation is divided into sereval steps:
        ///     Step 1: Add some characters for better compatibility (U+0000-U+0020).
        ///     Step 2: Remove unwanted characters in all cmap subtables.
        ///     Step 3: Remove empty cmap subtables.
        ///     Step 4: Add glyph id (0-3) to pending glyph sorted set for better compatibility.
        ///     Step 5: Add glyph ids from remaining cmap subtables to pending glyph sorted set.
        ///     Step 6: Following the composite relationship, calculate the glyph id closure as the pending glyph sorted set.
        ///     Step 7: Remove unwanted glyphs in glyf table.
        ///     Step 8: Remap glyph ids, make the oldNewGlyphIdMap.
        ///     Step 9: Update glyph ids and counts in cmap/glyf/loca/hmtx table.
        ///     Step 10: voila!
        /// </summary>
        /// <param name="characters">Character set to keep in the new font.</param>
        public void Subset(HashSet<uint> characters)
        {
            // Step 1
            for (uint i = 0; i <= 0x20; i++)
                characters.Add(i);

            // Step 2
            var cmapTable = Get<CmapTable>();
            cmapTable.Subtables.ForEach(subtable =>
            {
                subtable.CharGlyphIdMap =
                    subtable.CharGlyphIdMap.Where(pair => characters.Contains(pair.Key))
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
            });

            // Step 3
            cmapTable.Subtables.RemoveAll(subtable => subtable.CharGlyphIdMap.Count == 0); // Remove empty subtable

            // Step 4
            var pendingGlyphIds = new HashSet<uint> {0, 1, 2, 3};

            // Step 5 & Step 6
            var glyfTable = Get<GlyfTable>();
            pendingGlyphIds.UnionWith(cmapTable.Subtables.SelectMany(subtable => subtable.CharGlyphIdMap.Values));
            pendingGlyphIds = new HashSet<uint>(GlyphIdsTransitiveClosure(pendingGlyphIds, glyfTable).OrderBy(u => u));

            // Step 7
            glyfTable.Glyphs.RemoveAll(glyph => !pendingGlyphIds.Contains(glyph.Id));

            // Step 8
            var oldNewGlyphIdMap =
                pendingGlyphIds.Select((id, i) => new {NewId = (uint) i, OldId = id})
                    .ToDictionary(arg => arg.OldId, arg => arg.NewId);

            // Step 9
            // cmap
            cmapTable.Subtables.ForEach(subtable =>
            {
                var newCharGlyphIdMap = new Dictionary<uint, uint>();
                foreach (var pair in subtable.CharGlyphIdMap)
                    newCharGlyphIdMap[pair.Key] = oldNewGlyphIdMap[pair.Value];
                subtable.CharGlyphIdMap = newCharGlyphIdMap;
            });

            // glyf
            glyfTable.Glyphs.ForEach(glyph => glyph.Id = oldNewGlyphIdMap[glyph.Id]);
            foreach (var component in glyfTable.Glyphs.OfType<CompositeGlyph>().SelectMany(glyph => glyph.Components))
                component.GlyphId = (ushort) oldNewGlyphIdMap[component.GlyphId];

            // loca
            var locaTable = Get<LocaTable>();
            locaTable.ChangeNumberOfGlyphs(pendingGlyphIds.Count);

            // hmtx
            var hmtxTable = Get<HmtxTable>();
            hmtxTable.HorizontalMetrics =
                pendingGlyphIds.Select(glyphId => hmtxTable.HorizontalMetrics[(int) glyphId]).ToList();
        }

        public void SubsetTo(out OpenTypeFont newFont, HashSet<uint> characters)
        {
            newFont = DeepCopy();
            newFont.Subset(characters);
        }
    }
}