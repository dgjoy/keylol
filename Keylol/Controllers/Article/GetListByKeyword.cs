using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Utilities;

namespace Keylol.Controllers.Article
{
    public partial class ArticleController
    {
        /// <summary>
        ///     根据关键字搜索对应文章
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="full">是否获取完整的文章信息，包括文章概要、作者等，默认 false</param>
        /// <param name="skip">起始位置</param>
        /// <param name="take">获取数量，最大 50，默认 5</param>
        [Route("keyword/{keyword}")]
        [AllowAnonymous]
        [HttpGet]
        [ResponseType(typeof (List<ArticleDto>))]
        public async Task<IHttpActionResult> GetListByKeyword(string keyword, bool full = false, int skip = 0,
            int take = 5)
        {
            if (take > 50) take = 50;
            if (!full)
                return Ok(await _dbContext.Database.SqlQuery<ArticleDto>(@"SELECT
	                [t3].[Id],
	                [t3].[PublishTime],
	                [t3].[Title],
	                [t3].[SequenceNumberForAuthor],
	                [t3].[SequenceNumber],
	                [t3].[AuthorIdCode],
	                (SELECT
                        COUNT(1)
                        FROM [dbo].[Comments]
                        WHERE [t3].[Id] = [dbo].[Comments].[ArticleId]) AS [CommentCount],
	                (SELECT
                        COUNT(1)
                        FROM [dbo].[Likes]
                        WHERE ([t3].[Id] = [dbo].[Likes].[ArticleId])) AS [LikeCount]
	                FROM (SELECT
		                [t1].*,
		                [t4].[IdCode] AS [AuthorIdCode]
		                FROM [dbo].[Articles] AS [t1]
		                INNER JOIN (SELECT * FROM CONTAINSTABLE([dbo].[Articles], ([Title], [Content]), {0})) AS [t2] ON [t1].[Id] = [t2].[KEY]
                        LEFT OUTER JOIN [dbo].[KeylolUsers] AS [t4] ON [t4].[Id] = [t1].[PrincipalId]
                        WHERE [t1].[Archived] = 0 AND [t1].[Rejected] = 'False'
                        ORDER BY [t2].[RANK] DESC, [t1].[SequenceNumber] DESC
                        OFFSET({1}) ROWS FETCH NEXT({2}) ROWS ONLY) AS [t3]",
                    $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync());

            var articles = (await _dbContext.Database.SqlQuery<ArticleDto>(@"SELECT
                [t3].[Count],
	            [t3].[Id],
	            [t3].[PublishTime],
	            [t3].[Title],
	            [t3].[UnstyledContent] AS [Content],
                CASE WHEN [t3].[ThumbnailImage] = '' THEN
                    [t3].[VoteForPointBackgroundImage]
                ELSE
                    [t3].[ThumbnailImage]
                END AS [ThumbnailImage],
	            [t3].[SequenceNumberForAuthor],
	            [t3].[SequenceNumber],
	            [t3].[Type],
	            [t3].[AuthorId],
	            [t3].[AuthorIdCode],
	            [t3].[AuthorUserName],
	            [t3].[AuthorAvatarImage],
                [t3].[VoteForPointId],
                [t3].[VoteForPointPreferredName],
                [t3].[VoteForPointIdCode],
                [t3].[VoteForPointChineseName],
                [t3].[VoteForPointEnglishName],
	            (SELECT
                    COUNT(1)
                    FROM [dbo].[Comments]
                    WHERE [t3].[Id] = [dbo].[Comments].[ArticleId]) AS [CommentCount],
	            (SELECT
                    COUNT(1)
                    FROM [dbo].[Likes]
                    WHERE ([t3].[Id] = [dbo].[Likes].[ArticleId])) AS [LikeCount]
	            FROM (SELECT
                    COUNT(1) OVER() AS [Count],
		            [t1].*,
		            [t5].[Id] AS [AuthorId],
		            [t5].[IdCode] AS [AuthorIdCode],
		            [t5].[UserName] AS [AuthorUserName],
		            [t5].[AvatarImage] AS [AuthorAvatarImage],
                    [t7].[PreferredName] AS [VoteForPointPreferredName],
                    [t7].[IdCode] AS [VoteForPointIdCode],
                    [t7].[ChineseName] AS [VoteForPointChineseName],
                    [t7].[EnglishName] AS [VoteForPointEnglishName],
                    [t7].[BackgroundImage] AS [VoteForPointBackgroundImage]
		            FROM [dbo].[Articles] AS [t1]
		            INNER JOIN (SELECT * FROM CONTAINSTABLE([dbo].[Articles], ([Title], [Content]), {0})) AS [t2] ON [t1].[Id] = [t2].[KEY]
                    LEFT OUTER JOIN [dbo].[KeylolUsers] AS [t5] ON [t5].[Id] = [t1].[PrincipalId]
                    LEFT OUTER JOIN [dbo].[ProfilePoints] AS [t6] ON [t6].[Id] = [t1].[PrincipalId]
                    LEFT OUTER JOIN [dbo].[NormalPoints] AS [t7] ON [t7].[Id] = [t1].[VoteForPointId]
                    WHERE [t1].[Archived] = 0 AND [t1].[Rejected] = 'False'
                    ORDER BY [t2].[RANK] DESC, [t1].[SequenceNumber] DESC
                    OFFSET({1}) ROWS FETCH NEXT({2}) ROWS ONLY) AS [t3]",
                $"\"{keyword}\" OR \"{keyword}*\"", skip, take).ToListAsync())
                .Select(a =>
                {
                    if (a.VoteForPointId != null)
                        a.UnflattenVoteForPoint();
                    a.UnflattenAuthor().TruncateContent(256);
                    if (a.Type != ArticleType.简评)
                        a.TruncateContent(128);
                    else
                        a.ThumbnailImage = null;
                    a.TypeName = a.Type.ToString();
                    a.Type = null;
                    return a;
                })
                .ToList();

            var response = Request.CreateResponse(HttpStatusCode.OK, articles);
            response.Headers.SetTotalCount(articles.Count > 0 ? (articles[0].Count ?? 1) : 0);
            return ResponseMessage(response);
        }
    }
}