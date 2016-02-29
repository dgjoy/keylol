using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using CsQuery;
using CsQuery.Output;
using Keylol.Models;
using Keylol.Utilities;
using Newtonsoft.Json;

namespace Keylol.Controllers
{
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
        // 迁移方法需要保证幂等性

        /// <summary>
        /// v1.1.0 Step 1: 新建简评文章类型，现有据点商店链接转换，资源统一定位，优缺点修正
        /// </summary>
        [Route("v1-1-0-step-1-schema")]
        [HttpGet]
        public async Task<IHttpActionResult> V110Schema()
        {
            // 简评
            if (!await DbContext.ArticleTypes.Where(t => t.Name == "简评").AnyAsync())
            {
                DbContext.ArticleTypes.Add(new ArticleType
                {
                    Name = "简评",
                    AllowVote = true
                });
            }

            foreach (var normalPoint in DbContext.NormalPoints)
            {
                // NormalPoint StoreLink -> SteamAppId
                var storeLinkMatch = Regex.Match(normalPoint.StoreLink,
                    @"https?:\/\/store\.steampowered\.com\/app\/(\d+)", RegexOptions.IgnoreCase);
                if (storeLinkMatch.Success)
                {
                    normalPoint.SteamAppId = int.Parse(storeLinkMatch.Groups[1].Value);
                }

                // NormalPoint AvatarImage, BackgroundImage URI Fix
                var avatarImageMatch = Regex.Match(normalPoint.AvatarImage, @"keylol:\/\/avatars\/(.*)",
                    RegexOptions.IgnoreCase);
                if (avatarImageMatch.Success)
                {
                    normalPoint.AvatarImage = $"keylol://{avatarImageMatch.Groups[1].Value}";
                }
                var backgroundImageMatch = Regex.Match(normalPoint.BackgroundImage, @"^([0-9A-Z\.]+)$",
                    RegexOptions.IgnoreCase);
                if (backgroundImageMatch.Success)
                {
                    normalPoint.BackgroundImage = $"keylol://{backgroundImageMatch.Groups[1].Value}";
                }
            }

            // ProfilePoint BackgroundImage
            foreach (var profilePoint in DbContext.ProfilePoints)
            {
                var backgroundImageMatch = Regex.Match(profilePoint.BackgroundImage, @"^([0-9A-Z\.]+)$",
                    RegexOptions.IgnoreCase);
                if (backgroundImageMatch.Success)
                {
                    profilePoint.BackgroundImage = $"keylol://{backgroundImageMatch.Groups[1].Value}";
                }
            }

            foreach (var article in DbContext.Articles)
            {
                // Pros, Cons Fix
                if (article.Pros == null)
                    article.Pros = article.Type.AllowVote ? "[]" : string.Empty;
                if (article.Cons == null)
                    article.Cons = article.Type.AllowVote ? "[]" : string.Empty;

                // Article Content webp-src, ThumbnailImage URI Fix
                Config.HtmlEncoder = new HtmlEncoderMinimum();
                var dom = CQ.Create(article.Content);
                article.ThumbnailImage = string.Empty;
                foreach (var img in dom["img"])
                {
                    var url = string.Empty;
                    if (string.IsNullOrEmpty(img.Attributes["src"]))
                    {
                        if (string.IsNullOrEmpty(img.Attributes["webp-src"]))
                        {
                            if (string.IsNullOrEmpty(img.Attributes["article-image-src"]))
                                img.Remove();
                            else
                                url = img.Attributes["article-image-src"];
                        }
                        else
                        {
                            var fileName = Upyun.ExtractFileName(img.Attributes["webp-src"]);
                            if (string.IsNullOrEmpty(fileName))
                            {
                                url = img.Attributes["src"] = img.Attributes["webp-src"];
                            }
                            else
                            {
                                url = img.Attributes["article-image-src"] = $"keylol://{fileName}";
                            }
                            img.RemoveAttribute("webp-src");
                        }
                    }
                    else
                    {
                        var fileName = Upyun.ExtractFileName(img.Attributes["src"]);
                        if (string.IsNullOrEmpty(fileName))
                        {
                            url = img.Attributes["src"];
                        }
                        else
                        {
                            url = img.Attributes["article-image-src"] = $"keylol://{fileName}";
                            img.RemoveAttribute("src");
                        }
                    }
                    if (string.IsNullOrEmpty(article.ThumbnailImage))
                        article.ThumbnailImage = url;
                }
                article.Content = dom.Render();
            }

            // KeylolUser AvatarImage URI Fix
            foreach (var user in DbContext.Users)
            {
                var keylolMatch = Regex.Match(user.AvatarImage, @"keylol:\/\/avatars\/(.*)", RegexOptions.IgnoreCase);
                if (keylolMatch.Success)
                {
                    user.AvatarImage = $"keylol://{keylolMatch.Groups[1].Value}";
                    continue;
                }
                var steamMatch = Regex.Match(user.AvatarImage, @"steam:\/\/avatars\/(.*)", RegexOptions.IgnoreCase);
                if (steamMatch.Success)
                {
                    user.AvatarImage = $"keylol://steam/avatars/{steamMatch.Groups[1].Value}";
                }
            }
            await DbContext.SaveChangesAsync();
            return Ok("迁移成功");
        }

        /// <summary>
        /// v1.1.0 Step 2: 删除现有类型、厂商据点
        /// </summary>
        [Route("v1-1-0-step-2-clear-points")]
        [HttpGet]
        public async Task<IHttpActionResult> V110ClearPoints()
        {
            var points = await DbContext.NormalPoints.ToListAsync();
            foreach (var normalPoint in points)
            {
                normalPoint.AssociatedToPoints.Clear();
            }
            foreach (var article in await DbContext.Articles.ToListAsync())
            {
                foreach (var attachedPoint in article.AttachedPoints.ToList())
                {
                    if (string.IsNullOrEmpty(attachedPoint.NameInSteamStore) &&
                        (attachedPoint.Type == NormalPointType.Genre ||
                         attachedPoint.Type == NormalPointType.Manufacturer))
                    {
                        article.AttachedPoints.Remove(attachedPoint);
                    }
                }
            }
            DbContext.NormalPoints.RemoveRange(await DbContext.NormalPoints
                .Where(p => string.IsNullOrEmpty(p.NameInSteamStore) &&
                            (p.Type == NormalPointType.Genre || p.Type == NormalPointType.Manufacturer))
                .ToListAsync());
            await DbContext.SaveChangesAsync();
            return Ok("删除成功");
        }

        /// <summary>
        /// v1.1.0 Step 3: 获取需要重新抓取的游戏据点和需要手动编辑推送的文章
        /// </summary>
        [Route("v1-1-0-step-3-update-list")]
        [HttpGet]
        public async Task<IHttpActionResult> V110ManualUpdateList()
        {
            var needUpdateGamePoints = await DbContext.NormalPoints
                .Where(p => p.Type == NormalPointType.Game && string.IsNullOrEmpty(p.NameInSteamStore))
                .ToListAsync();
            var needUpdateArticles = await DbContext.Articles
                .Where(a => a.Type.Name == "评" && a.VoteForPointId == null)
                .Concat(DbContext.Articles.Where(a => a.Type.Name != "评" && a.AttachedPoints.Count == 0))
                .ToListAsync();
            return Ok(new
            {
                Points = needUpdateGamePoints.Select(p => $"{p.IdCode} {p.SteamAppId}"),
                Articles = needUpdateArticles.Select(a => $"{a.Principal.User.IdCode}/{a.SequenceNumberForAuthor}")
            });
        }

        /// <summary>
        /// v1.1.0 Step 4: 重新推送现有的“评”类且有评价对象的文章到相关据点
        /// </summary>
        [Route("v1-1-0-step-4-reattach-articles")]
        [HttpGet]
        public async Task<IHttpActionResult> V110ReattachArticles()
        {
            var articles = await DbContext.Articles
                .Include(a => a.AttachedPoints)
                .Include(a => a.VoteForPoint)
                .Where(a => a.Type.Name == "评" && a.VoteForPointId != null)
                .ToListAsync();
            foreach (var article in articles)
            {
                article.AttachedPoints = article.VoteForPoint.DeveloperPoints
                    .Concat(article.VoteForPoint.PublisherPoints)
                    .Concat(article.VoteForPoint.SeriesPoints)
                    .Concat(article.VoteForPoint.GenrePoints)
                    .Concat(article.VoteForPoint.TagPoints).ToList();
                article.AttachedPoints.Add(article.VoteForPoint);
            }
            await DbContext.SaveChangesAsync();
            return Ok("推送成功");
        }
    }
}