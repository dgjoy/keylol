using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Keylol.Models.DAL;
using Keylol.Models.DTO;
using Keylol.Services.Contracts;

namespace Keylol.Services
{
    /// <summary>
    /// <see cref="ImageGarageCoordinator"/> 实现
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
            throw new FaultException("Test");
            using (var dbContext = new KeylolDbContext())
            {
                var article = await dbContext.Articles.FindAsync(id);
                if (article == null)
                    throw new FaultException("文章不存在");
                return new ArticleDto
                {

                };
            }
        }

        /// <summary>
        /// 更新指定文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        public Task UpdateArticle(string id)
        {
            throw new NotImplementedException();
        }
    }
}