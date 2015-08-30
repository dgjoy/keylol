using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Keylol.FontGarage.Table;
using Keylol.FontGarage.Table.Cmap;
using Environment = System.Environment;

namespace Keylol.FontGarage.CLI
{
    internal class Program
    {
        private static void Main()
        {
            SubsetFont("keylol-rail-sung-full.ttf", "keylol-rail-sung-", new[]
            {
                "`其乐",
                "推荐据点",
                "客务中心",
                "讯息轨道",
                "评测",
                "好评",
                "资讯",
                "差评",
                "模组",
                "感悟",
                "请无视游戏与艺术之间的空隙",
                "提交注册申请",
                "登入其乐",
                "发布文章",
                "由你筛选的游戏讯息轨道"
            });
            SubsetFont("lisong-full.ttf", "lisong-", new[]
            {
                "评测好评差评模组资讯",
                "会员注册表单",
                "登录表单",
                "连接游戏平台",
                "昵称",
                "账户头像",
                "登录口令",
                "确认登录口令",
                "电子邮箱",
                "人机验证",
                "声明",
                "桌面类蒸汽第一人称射击时空枪使命召唤侠盗猎车手橘子孢子上帝视角文明红色警戒模拟城市塔防即时策略折扣资讯原声控僵尸末日泰拉瑞亚独立游戏"
            });
            Console.ReadKey();
        }

        private static void SubsetFont(string srcFileName, string dstFileNamePrefix, string[] phrases)
        {
            var fontData =
                File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    srcFileName));
            var serializer = new OpenTypeFontSerializer();
            var font = serializer.Deserialize(new BinaryReader(new MemoryStream(fontData)));

            var allChars = new string(string.Join("", phrases).ToCharArray().Distinct().OrderBy(c => c).ToArray());
            var identityHash = StringMd5(allChars);

            font.Subset(new HashSet<uint>(allChars.ToCharArray().Select(c => (uint) c)));

            var memoryStream = new MemoryStream();
            serializer.Serialize(new BinaryWriter(memoryStream), font);
            var fileName = string.Format("{0}{1}.woff", dstFileNamePrefix, identityHash);
            using (var file = File.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                fileName), FileMode.Create))
            {
                FontFormatConverter.SfntToWoff(new BinaryReader(memoryStream), new BinaryWriter(file), true);
                //memoryStream.WriteTo(file);
            }
            Console.WriteLine("{0} generated.", fileName);
        }

        private static void Benchmark()
        {
            var fontData = File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "SourceHanSansSC-Regular.ttf"));
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

            watch = Stopwatch.StartNew();
            font.DeepCopy();
            watch.Stop();
            Console.WriteLine("Deep copy time: {0}ms\n", watch.ElapsedMilliseconds);

            var subset = new List<uint>();
            subset.AddRange(Enumerable.Range(0, 0xFFFF).Select(i => (uint) i));
            //var random = new Random();
            //subset.AddRange(Enumerable.Range(0, 0xFFFF).Select(result => (uint)random.Next(0, 0xFFFF)));
            //var subset = new List<uint> {0x5937, 0x21, 0x59D4, 0x5C09, 0x6216, 0x978D};
            watch = Stopwatch.StartNew();
            font.Subset(new HashSet<uint>(subset));
            watch.Stop();
            Console.WriteLine("Subsetting time: {0}ms\n", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew();
            var memoryStream = new MemoryStream();
            serializer.Serialize(new BinaryWriter(memoryStream), font);
            watch.Stop();
            using (var file = File.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Generated.otf"), FileMode.Create))
                memoryStream.WriteTo(file);
            Console.WriteLine("Serializing time: {0}ms", watch.ElapsedMilliseconds);

            watch = Stopwatch.StartNew();
            using (var file = File.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Generated.woff"), FileMode.Create))
                FontFormatConverter.SfntToWoff(new BinaryReader(memoryStream), new BinaryWriter(file));
            watch.Stop();
            Console.WriteLine("TTF to WOFF time: {0}ms", watch.ElapsedMilliseconds);
        }

        private static string StringMd5(string inputString)
        {
            var hashBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(inputString));
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}