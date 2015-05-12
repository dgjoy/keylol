using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keylol.FontGarage;
using Keylol.FontGarage.Table;
using Keylol.FontGarage.Table.Cmap;
using Environment = System.Environment;

namespace Keylol.FontGarageCLI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var fontData = File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "arial.ttf"));
            var watch = Stopwatch.StartNew();
            var serializer = new OpenTypeFontSerializer();
            var font = serializer.Deserialize(new BinaryReader(new MemoryStream(fontData)));
            //var font =
            //    serializer.Deserialize(
            //        new BinaryReader(
            //            File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            //                "Keylol FakeMTR Extended.otf"))));
            watch.Stop();

            Console.WriteLine("Parsing time: {0}ms", watch.ElapsedMilliseconds);
            Console.WriteLine("sfnt version: {0}", font.SfntVersion);
            Console.WriteLine("Tables: {0}", string.Join(", ", font.Tables.Select(t => t.Tag)));
            Console.WriteLine("Cmap format-4 subtable character count: {0}",
                font.Tables.OfType<CmapTable>()
                    .Single()
                    .Subtables.OfType<Format4Subtable>()
                    .Single()
                    .CharGlyphIdMap.Count());
            Console.WriteLine("Glyph count: {0}",
                font.Tables.OfType<LocaTable>()
                    .Single()
                    .GlyphOffsets.Count());
            Console.WriteLine("Non-empty glyph count: {0}",
                font.Tables.OfType<GlyfTable>()
                    .Single()
                    .Glyphs.Count());
            Console.ReadKey();
        }
    }
}