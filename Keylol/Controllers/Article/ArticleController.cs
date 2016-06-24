using System.Web.Http;
using CsQuery;
using CsQuery.Output;
using Ganss.XSS;
using Keylol.Identity;
using Keylol.Models.DAL;
using Keylol.Provider;
using Keylol.ServiceBase;
using RabbitMQ.Client;

namespace Keylol.Controllers.Article
{
    /// <summary>
    ///     文章 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("article")]
    public partial class ArticleController : ApiController
    {
        private readonly CouponProvider _coupon;
        private readonly KeylolDbContext _dbContext;
        private readonly IModel _mqChannel;
        private readonly KeylolUserManager _userManager;

        /// <summary>
        ///     创建 <see cref="ArticleController" />
        /// </summary>
        /// <param name="mqChannel">
        ///     <see cref="IModel" />
        /// </param>
        /// <param name="coupon">
        ///     <see cref="CouponProvider" />
        /// </param>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        /// <param name="userManager">
        ///     <see cref="KeylolUserManager" />
        /// </param>
        public ArticleController(IModel mqChannel, CouponProvider coupon, KeylolDbContext dbContext,
            KeylolUserManager userManager)
        {
            _mqChannel = mqChannel;
            _coupon = coupon;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        /// <summary>
        /// 净化富文本
        /// </summary>
        /// <param name="html">HTML 代码</param>
        /// <returns>净化后的 HTML 代码</returns>
        public static string SanitizeRichText(string html)
        {
            Config.HtmlEncoder = new HtmlEncoderNone();
            Config.OutputFormatter = OutputFormatters.HtmlEncodingNone;
            var sanitizer = new HtmlSanitizer(
                new[]
                {
                    "br", "span", "a", "img", "b", "strong", "i", "strike", "u", "p", "blockquote", "h1", "hr",
                    "comment", "spoiler", "table", "colgroup", "col", "thead", "tr", "th", "tbody", "td"
                },
                null,
                new[] {"src", "alt", "width", "height", "data-non-image", "href", "style"},
                null,
                new[] {"text-align"});
            var dom = CQ.Create(sanitizer.Sanitize(html));
            foreach (var img in dom["img"])
            {
                if (string.IsNullOrWhiteSpace(img.Attributes["src"]))
                {
                    img.Remove();
                }
                else
                {
                    var fileName = UpyunProvider.ExtractFileName(img.Attributes["src"]);
                    if (string.IsNullOrWhiteSpace(fileName)) continue;
                    img.Attributes["article-image-src"] = $"keylol://{fileName}";
                    img.RemoveAttribute("src");
                }
            }
            return dom.Render();
        }

        /// <summary>
        /// 如果图片属于自有存储，则提取出来替换 Schema，否则返回原图
        /// </summary>
        /// <param name="coverImage">封面图</param>
        /// <returns>净化后的封面图</returns>
        public static string SanitizeCoverImage(string coverImage)
        {
            var fileName = UpyunProvider.ExtractFileName(coverImage);
            return string.IsNullOrWhiteSpace(fileName) ? coverImage : $"keylol://{fileName}";
        }
    }
}