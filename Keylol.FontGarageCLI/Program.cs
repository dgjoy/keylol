using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Keylol.FontGarage;
using Keylol.FontGarage.Table;
using Keylol.FontGarage.Table.Cmap;
using Environment = System.Environment;

namespace Keylol.FontGarageCLI
{
    internal class Program
    {
        private static void Main()
        {
            var fontData = File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Keylol FakeMTR Extended.otf"));
            var watch = Stopwatch.StartNew();
            var serializer = new OpenTypeFontSerializer
            {
                EnableChecksum = false
            };
            var font = serializer.Deserialize(new BinaryReader(new MemoryStream(fontData)));
            watch.Stop();
            Console.WriteLine("Parsing time: {0}ms", watch.ElapsedMilliseconds);
            Console.WriteLine("sfnt version: {0}", font.SfntVersion);
            Console.WriteLine("Tables: {0}", string.Join(", ", font.Tables.Select(t => t.Tag)));
            Console.WriteLine("Cmap format-4 subtable (3, 1) character count: {0}",
                font.Tables.OfType<CmapTable>()
                    .Single()
                    .Subtables.OfType<Format4Subtable>()
                    .Single(
                        subtable =>
                            subtable.Environments.Exists(
                                environment => environment.PlatformId == 3 && environment.EncodingId == 1))
                    .CharGlyphIdMap.Count());
            Console.WriteLine("Glyph count: {0}",
                font.Tables.OfType<LocaTable>()
                    .Single()
                    .GlyphOffsets.Count());
            Console.WriteLine("Non-empty glyph count: {0}\n",
                font.Tables.OfType<GlyfTable>()
                    .Single()
                    .Glyphs.Count());

            var subset = new List<uint>();
            //subset.AddRange(Enumerable.Range(0, 0xFFFF).Select(i => (uint) i));
            var random = new Random();
            subset.AddRange(Enumerable.Range(0, 0x8888).Select(result => (uint) random.Next(0, 0xFFFF)));
            //new List<uint> {0x5937, 0x21, 0x59D4, 0x5C09, 0x6216, 0x978D}
            watch = Stopwatch.StartNew();
            font.Subset(subset);
            watch.Stop();
            Console.WriteLine("Subsetting time: {0}ms\n", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew();
            var memoryStream = new MemoryStream();
            serializer.Serialize(new BinaryWriter(memoryStream), font);
            watch.Stop();
            using (var file = File.OpenWrite(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Generated.otf")))
                memoryStream.WriteTo(file);
            Console.WriteLine("Serializing time: {0}ms", watch.ElapsedMilliseconds);

            Console.ReadKey();
        }
    }
}