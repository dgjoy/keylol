using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using CsQuery;
using CsQuery.Output;
using Keylol.Utilities;

namespace Keylol.Controllers
{
    [Authorize]
    [ClaimsAuthorize(StaffClaim.ClaimType, StaffClaim.Operator)]
    [RoutePrefix("database-migration")]
    public class DatabaseMigrationController : KeylolApiController
    {
        [Route("v1")]
        public async Task<IHttpActionResult> Get()
        {
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
                var avatarImageMatch = Regex.Match(normalPoint.AvatarImage, @"keylol:\/\/avatars\/(.*)", RegexOptions.IgnoreCase);
                if (avatarImageMatch.Success)
                {
                    normalPoint.AvatarImage = $"keylol://{avatarImageMatch.Groups[1].Value}";
                }
                var backgroundImageMatch = Regex.Match(normalPoint.BackgroundImage, @"^([0-9A-Z\.]+)$", RegexOptions.IgnoreCase);
                if (backgroundImageMatch.Success)
                {
                    normalPoint.BackgroundImage = $"keylol://{backgroundImageMatch.Groups[1].Value}";
                }
            }

            // ProfilePoint BackgroundImage
            foreach (var profilePoint in DbContext.ProfilePoints)
            {
                var backgroundImageMatch = Regex.Match(profilePoint.BackgroundImage, @"^([0-9A-Z\.]+)$", RegexOptions.IgnoreCase);
                if (backgroundImageMatch.Success)
                {
                    profilePoint.BackgroundImage = $"keylol://{backgroundImageMatch.Groups[1].Value}";
                }
            }

            foreach (var article in DbContext.Articles)
            {
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
            return Ok("Success");
        }
    }
}