using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models.DTO;
using Keylol.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Keylol.Controllers.CouponLog
{
    public partial class CouponLogController
    {
        /// <summary>
        ///     获取当前登录用户的文券变动记录
        /// </summary>
        /// <param name="skip">起始位置，默认 0</param>
        /// <param name="take">获取数量，默认 30，最大 50</param>
        [Route]
        [HttpGet]
        [ResponseType(typeof (List<CouponLogDto>))]
        public async Task<HttpResponseMessage> GetListByCurrentUser(int skip = 0, int take = 30)
        {
            if (take > 50) take = 50;
            var userId = User.Identity.GetUserId();
            var couponLogs = await DbContext.CouponLogs.Where(cl => cl.UserId == userId)
                .OrderByDescending(cl => cl.CreateTime)
                .Skip(() => skip)
                .Take(() => take)
                .ToListAsync();
            var result = new List<CouponLogDto>(couponLogs.Count);
            foreach (var couponLog in couponLogs)
            {
                var dto = new CouponLogDto
                {
                    Id = couponLog.Id,
                    Event = couponLog.Event,
                    Change = couponLog.Change,
                    Balance = couponLog.Balance,
                    CreateTime = couponLog.CreateTime,
                    Description = JsonConvert.DeserializeObject(couponLog.Description)
                };

                #region 解析各种类型的描述

                try
                {
                    Func<object, JObject> jObject =
                        o => JObject.FromObject(o, new JsonSerializer {NullValueHandling = NullValueHandling.Ignore});
                    if (dto.Description.ArticleId != null)
                    {
                        var article = await DbContext.Articles.FindAsync((string) dto.Description.ArticleId);
                        ((JObject) dto.Description).Remove("ArticleId");
                        dto.Description.Article = jObject(new ArticleDto
                        {
                            Id = article.Id,
                            Title = article.Title,
                            SequenceNumberForAuthor = article.SequenceNumberForAuthor,
                            AuthorIdCode = article.Principal.User.IdCode
                        });
                    }
                    if (dto.Description.CommentId != null)
                    {
                        var comment = await DbContext.Comments.FindAsync((string) dto.Description.CommentId);
                        ((JObject) dto.Description).Remove("CommentId");
                        dto.Description.Comment = jObject(new CommentDto
                        {
                            Id = comment.Id,
                            Content =
                                comment.Content.Length > 15 ? $"{comment.Content.Substring(0, 15)} …" : comment.Content,
                            ArticleAuthorIdCode = comment.Article.Principal.User.IdCode,
                            ArticleSequenceNumberForAuthor = comment.Article.SequenceNumberForAuthor,
                            SequenceNumberForArticle = comment.SequenceNumberForArticle
                        });
                    }
                    if (dto.Description.OperatorId != null)
                    {
                        var user = DbContext.Users.Find((string) dto.Description.OperatorId);
                        ((JObject) dto.Description).Remove("OperatorId");
                        dto.Description.Operotor = jObject(new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            IdCode = user.IdCode
                        });
                    }
                    if (dto.Description.UserId != null)
                    {
                        var user = DbContext.Users.Find((string) dto.Description.UserId);
                        ((JObject) dto.Description).Remove("UserId");
                        dto.Description.User = jObject(new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            IdCode = user.IdCode
                        });
                    }
                    if (dto.Description.InviterId != null)
                    {
                        var user = DbContext.Users.Find((string) dto.Description.InviterId);
                        ((JObject) dto.Description).Remove("InviterId");
                        dto.Description.Inviter = jObject(new UserDto
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            IdCode = user.IdCode
                        });
                    }
                }
                catch (RuntimeBinderException)
                {
                }

                #endregion

                result.Add(dto);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK, result);
            var totalCount = await DbContext.CouponLogs.CountAsync(cl => cl.UserId == userId);
            response.Headers.SetTotalCount(totalCount);
            return response;
        }
    }
}