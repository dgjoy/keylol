using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Services.Contracts;

namespace Keylol.Services
{
    /// <summary>
    ///     <see cref="IImageGarageCoordinator" /> 实现
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ImageGarageCoordinator : IImageGarageCoordinator
    {
        /// <summary>
        ///     获取指定 ID 的文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <returns>
        ///     <see cref="ArticleDto" />
        /// </returns>
        public async Task<ArticleDto> FindArticle(string id)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var article = await dbContext.Articles.FindAsync(id);
                if (article == null)
                    throw new FaultException("文章不存在");
                return new ArticleDto
                {
                    Id = article.Id,
                    Content = article.Content,
                    Title = article.Title,
                    CoverImage = article.CoverImage,
                    RowVersion = article.RowVersion
                };
            }
        }

        /// <summary>
        ///     获取指定 ID 的文章评论
        /// </summary>
        /// <param name="id">文章评论 ID</param>
        /// <returns>
        ///     <see cref="ArticleCommentDto" />
        /// </returns>
        public async Task<ArticleCommentDto> FindArticleComment(string id)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var comment = await dbContext.ArticleComments.FindAsync(id);
                if (comment == null)
                    throw new FaultException("评论不存在");
                return new ArticleCommentDto
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    RowVersion = comment.RowVersion
                };
            }
        }

        /// <summary>
        ///     心跳测试
        /// </summary>
        public void Ping()
        {
        }

        /// <summary>
        ///     更新指定文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="content">新的内容</param>
        /// <param name="coverImage">新的封面图</param>
        /// <param name="rowVersion">参考 RowVersion</param>
        public async Task UpdateArticle(string id, string content = null, string coverImage = null,
            byte[] rowVersion = null)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var article = await dbContext.Articles.FindAsync(id);
                if (article == null)
                    throw new FaultException("文章不存在");
                if (rowVersion != null && !article.RowVersion.SequenceEqual(rowVersion))
                    throw new FaultException("检测到文章已被编辑过，更新失败");
                if (content != null)
                    article.Content = content;
                if (coverImage != null)
                    article.CoverImage = coverImage;
                await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.DatabaseWin);
            }
        }

        /// <summary>
        ///     更新指定文章评论
        /// </summary>
        /// <param name="id">文章评论 ID</param>
        /// <param name="content">新的内容</param>
        /// <param name="rowVersion">参考 RowVersion</param>
        public async Task UpdateArticleComment(string id, string content = null, byte[] rowVersion = null)
        {
            using (var dbContext = new KeylolDbContext())
            {
                var comment = await dbContext.ArticleComments.FindAsync(id);
                if (comment == null)
                    throw new FaultException("评论不存在");
                if (rowVersion != null && !comment.RowVersion.SequenceEqual(rowVersion))
                    throw new FaultException("检测到文章已被编辑过，更新失败");
                if (content != null)
                    comment.Content = content;
                await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.DatabaseWin);
            }
        }
    }
}