using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Keylol.FontGarage.OnlineDemo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Dictionary<string, OpenTypeFont> _loadedFonts = new Dictionary<string, OpenTypeFont>();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetFontSubset(string fontName, string text)
        {
            var allChars = new string(text.ToCharArray().Distinct().OrderBy(c => c).ToArray());
            var identityHash = StringMd5(allChars);
            var path =
                new FileInfo(Path.Combine(Server.MapPath("~/fonts/cache"),
                    string.Format("{0}-{1}.ttf", fontName, identityHash)));
            var returnData =
                new
                {
                    fontName = string.Format("{0}-{1}", fontName, identityHash),
                    fileName = string.Format("cache/{0}-{1}", fontName, identityHash)
                };

            if (!path.Exists)
            {
                var serializer = new OpenTypeFontSerializer {EnableChecksum = false};
                OpenTypeFont font;
                if (!_loadedFonts.TryGetValue(fontName, out font))
                {
                    var fontPath = Path.Combine(Server.MapPath("~/fonts"), string.Format("{0}.ttf", fontName));
                    if (!System.IO.File.Exists(fontPath))
                        return HttpNotFound();
                    font =
                        serializer.Deserialize(new BinaryReader(new MemoryStream(System.IO.File.ReadAllBytes(fontPath))));
                    _loadedFonts[fontName] = font;
                }

                using (var memoryStream = new MemoryStream())
                {
                    OpenTypeFont subset;
                    font.SubsetTo(out subset, new HashSet<uint>(allChars.ToCharArray().Select(c => (uint) c)));
                    serializer.Serialize(new BinaryWriter(memoryStream), subset);
                    if (path.Directory != null) path.Directory.Create();
                    using (var fileStream = path.Open(FileMode.Create))
                        memoryStream.WriteTo(fileStream);
                }
            }

            return Json(returnData);
        }

        private string StringMd5(string inputString)
        {
            var hashBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(inputString));
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}