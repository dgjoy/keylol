using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Keylol.Models;
using Microsoft.AspNet.Identity;

namespace Keylol.Controllers.Message
{
    public partial class MessageController
    {
        /// <summary>
        /// 获取当前登录用户的邮政消息
        /// </summary>
        [Route]
        [HttpGet]
        public async Task<IHttpActionResult> GetListByCurrentUser(MessageFilter? filter = null)
        {
            var userId = User.Identity.GetUserId();
            IQueryable<Models.Message> query;
                switch (filter)
                {
                    case MessageFilter.Like:
                        query = DbContext.Messages.Where(m => m is LikeMessage);
                        break;
                    case MessageFilter.Comment:
                    query = DbContext.Messages.Where(m => m is CommentMessage);
                    break;
                    case MessageFilter.Missive:
                    query = DbContext.Messages.Where(m => m is MissiveMessage);
                    break;
                    case null:
                        query =
                            DbContext.Messages.Include(m => (m as ArticleArchiveMissiveMessage).Article).AsQueryable();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
                }
            var result = await query.ToListAsync();
            return Ok(result);
        }

        public enum MessageFilter
        {
            Like,
            Comment,
            Missive
        }

        public class MessageGetListByCurrentUserResponseDto
        {
        }
    }
}