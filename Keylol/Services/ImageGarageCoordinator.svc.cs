using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Services.Contracts;

namespace Keylol.Services
{
    /// <summary>
    /// <see cref="IImageGarageCoordinator"/> 实现
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ImageGarageCoordinator : IImageGarageCoordinator
    {
        /// <summary>
        /// 获取指定 ID 的文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <returns><see cref="ArticleDto"/></returns>
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
                    RowVersion = article.RowVersion
                };
            }
        }

        /// <summary>
        /// 更新指定文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="content">新的内容</param>
        /// <param name="thumbnailImage">新的缩略图</param>
        /// <param name="rowVersion">参考 RowVersion</param>
        public async Task UpdateArticle(string id, string content = null, string thumbnailImage = null,
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
                if (thumbnailImage != null)
                    article.ThumbnailImage = thumbnailImage;
                await dbContext.SaveChangesAsync(KeylolDbContext.ConcurrencyStrategy.DatabaseWin);
            }
        }
    }
}