using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CsQuery;
using CsQuery.Output;
using Ganss.XSS;
using Keylol.Utilities;

namespace Keylol.Controllers.Article
{
    [Authorize]
    [RoutePrefix("article")]
    public partial class ArticleController : KeylolApiController
    {
        private static async Task SanitizeArticle(Models.Article article, bool extractUnstyledContent, bool proxyExternalImages)
        {
            Config.HtmlEncoder = new HtmlEncoderMinimum();
            var sanitizer =
                new HtmlSanitizer(
                    new[]
                    {
                        "br", "span", "a", "img", "b", "strong", "i", "strike", "u", "p", "blockquote", "h1", "hr",
                        "comment", "spoiler", "table", "colgroup", "col", "thead", "tr", "th", "tbody", "td"
                    },
                    null,
                    new[] {"src", "alt", "width", "height", "data-non-image", "href", "style"},
                    null,
                    new[] {"text-align"});
            var dom = CQ.Create(sanitizer.Sanitize(article.Content));
            article.ThumbnailImage = string.Empty;
            foreach (var img in dom["img"])
            {
                var url = string.Empty;
                if (string.IsNullOrEmpty(img.Attributes["src"]))
                {
                    img.Remove();
                }
                else
                {
                    var fileName = Upyun.ExtractFileName(img.Attributes["src"]);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        url = img.Attributes["src"];
                        if (proxyExternalImages)
                        {
                            var client = new HttpClient();
                            var fileData = await client.GetByteArrayAsync(img.Attributes["src"]);
                            if (fileData.Length > 0)
                            {
                                var uri = new Uri(img.Attributes["src"]);
                                var extension = Path.GetExtension(uri.AbsolutePath);
                                if (!string.IsNullOrEmpty(extension))
                                {
                                    var name = await Upyun.UploadFile(fileData, extension);
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        url = $"keylol://{name}";
                                        img.Attributes["article-image-src"] = url;
                                        img.RemoveAttribute("src");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        url = $"keylol://{fileName}";
                        img.Attributes["article-image-src"] = url;
                        img.RemoveAttribute("src");
                    }
                }
                if (string.IsNullOrEmpty(article.ThumbnailImage))
                    article.ThumbnailImage = url;
            }
            article.Content = dom.Render();
            if (extractUnstyledContent)
                article.UnstyledContent = dom.Render(OutputFormatters.PlainText);
        }
    }
}