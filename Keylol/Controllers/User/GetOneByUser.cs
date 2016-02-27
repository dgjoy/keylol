using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace Keylol.Controllers.User
{
    public partial class UserController
    {
        public enum IdType
        {
            Id,
            IdCode,
            UserName
        }

        /// <summary>
        ///     根据 Id、UserName 或者 IdCode 取得一名用户
        /// </summary>
        /// <param name="id">用户 ID</param>
        /// <param name="includeProfilePointBackgroundImage">是否包含用户据点背景图片，默认 false</param>
        /// <param name="includeClaims">是否包含用户权限级别，默认 false</param>
        /// <param name="includeSecurity">是否包含用户安全信息（邮箱、登录保护等），用户只能获取自己的安全信息（除非是运维职员），默认 false</param>
        /// <param name="includeSteam">是否包含用户 Steam 信息，用户只能获取自己的 Steam 信息（除非是运维职员），默认 false</param>
        /// <param name="includeSteamBot">是否包含用户所属 Steam 机器人（用户只能获取自己的机器人（除非是运维职员），默认 false</param>
        /// <param name="includeSubscribeCount">是否包含用户订阅数量（用户只能获取自己的订阅信息（除非是运维职员），默认 false</param>
        /// <param name="includeStats">是否包含用户读者数和文章数，默认 false</param>
        /// <param name="includeSubscribed">是否包含该用户有没有被当前用户的信息，默认 false</param>
        /// <param name="includeMoreOptions">是否包含更多杂项设置（例如通知偏好设置），默认 false</param>
        /// <param name="includeCommentLike">是否包含用户有无新的评论和认可，用户只能获取自己的信息（除非是运维职员），默认 false</param>
        /// <param name="includeReviewStats">是否包含用户评测文章数和简评数，默认 false</param>
        /// <param name="idType">ID 类型，默认 "Id"</param>
        [Route("{id}")]
        [HttpGet]
        [ResponseType(typeof (UserDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound, "指定用户不存在")]
        public async Task<IHttpActionResult> GetOneByUser(string id, bool includeProfilePointBackgroundImage = false,
            bool includeClaims = false, bool includeSecurity = false, bool includeSteam = false,
            bool includeSteamBot = false, bool includeSubscribeCount = false, bool includeStats = false,
            bool includeSubscribed = false, bool includeMoreOptions = false, bool includeCommentLike = false,
            bool includeReviewStats = false, IdType idType = IdType.Id)
        {
            KeylolUser user;
            switch (idType)
            {
                case IdType.UserName:
                    user = await DbContext.Users.SingleOrDefaultAsync(u => u.UserName == id);
                    break;

                case IdType.IdCode:
                    user = await DbContext.Users.SingleOrDefaultAsync(u => u.IdCode == id);
                    break;

                case IdType.Id:
                    user = await DbContext.Users.SingleOrDefaultAsync(u => u.Id == id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }

            var visitorId = User.Identity.GetUserId();
            var visitorStaffClaim = await UserManager.GetStaffClaimAsync(visitorId);

            if (user == null)
                return NotFound();

            var userDTO = includeMoreOptions ? new UserWithMoreOptionsDTO(user) : new UserDTO(user);

            if (includeProfilePointBackgroundImage)
                userDTO.ProfilePointBackgroundImage = user.ProfilePoint.BackgroundImage;

            if (includeClaims)
            {
                userDTO.StatusClaim = await UserManager.GetStatusClaimAsync(user.Id);
                userDTO.StaffClaim = await UserManager.GetStaffClaimAsync(user.Id);
            }

            if (includeSecurity && (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator))
                userDTO.IncludeSecurity();

            if (includeSteam && (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator))
                userDTO.IncludeSteam();

            if (includeSteamBot && (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator))
                userDTO.SteamBot = new SteamBotDTO(user.SteamBot);

            if (includeSubscribeCount)
            {
                userDTO.SubscribedPointCount =
                    await DbContext.Users.Where(u => u.Id == user.Id).SelectMany(u => u.SubscribedPoints).CountAsync();
            }

            if (includeStats)
            {
                var stats = await DbContext.Users.Where(u => u.Id == user.Id)
                    .Select(u =>
                        new
                        {
                            subscriberCount = u.ProfilePoint.Subscribers.Count,
                            articleCount = u.ProfilePoint.Entries.OfType<Models.Article>().Count()
                        })
                    .SingleOrDefaultAsync();
                userDTO.SubscriberCount = stats.subscriberCount;
                userDTO.ArticleCount = stats.articleCount;
            }

            if (includeReviewStats)
            {
                var reviewStats = await DbContext.Users.Where(u => u.Id == user.Id)
                    .Select(u => new
                    {
                        reviewCount = u.ProfilePoint.Entries.OfType<Models.Article>().Count(a => a.Type.Name == "评"),
                        shortReviewCount =
                            u.ProfilePoint.Entries.OfType<Models.Article>().Count(a => a.Type.Name == "简评")
                    })
                    .SingleOrDefaultAsync();
                userDTO.ReviewCount = reviewStats.reviewCount;
                userDTO.ShortReviewCount = reviewStats.shortReviewCount;
            }

            if (includeSubscribed)
            {
                userDTO.Subscribed = await DbContext.Users.Where(u => u.Id == visitorId)
                    .SelectMany(u => u.SubscribedPoints)
                    .Select(p => p.Id)
                    .ContainsAsync(user.Id);
            }

            if (includeCommentLike && (visitorId == user.Id || visitorStaffClaim == StaffClaim.Operator))
            {
                userDTO.HasNewComment =
                    await DbContext.CommentReplies.Where(r =>
                        r.Comment.CommentatorId == user.Id &&
                        r.IgnoredByCommentAuthor == false &&
                        r.ReadByCommentAuthor == false).AnyAsync() ||
                    await DbContext.Comments.Where(c =>
                        c.Article.PrincipalId == user.Id &&
                        c.IgnoredByArticleAuthor == false &&
                        c.ReadByArticleAuthor == false).AnyAsync();
                userDTO.HasNewLike =
                    await DbContext.ArticleLikes.Where(l =>
                        l.Backout == false &&
                        l.Article.PrincipalId == user.Id &&
                        l.IgnoredByTargetUser == false &&
                        l.ReadByTargetUser == false).AnyAsync() ||
                    await DbContext.CommentLikes.Where(l =>
                        l.Backout == false &&
                        l.Comment.CommentatorId == user.Id &&
                        l.IgnoredByTargetUser == false &&
                        l.ReadByTargetUser == false).AnyAsync();
            }

            return Ok(userDTO);
        }
    }
}