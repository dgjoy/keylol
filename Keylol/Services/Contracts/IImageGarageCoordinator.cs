using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DTO;

namespace Keylol.Services.Contracts
{
    /// <summary>
    /// ImageGarage 协作器
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IImageGarageCoordinator
    {
        /// <summary>
        /// 获取指定 ID 的文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <returns><see cref="ArticleDto"/></returns>
        [OperationContract]
        Task<ArticleDto> FindArticle(string id);

        /// <summary>
        /// 更新指定文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="content">新的内容</param>
        /// <param name="thumbnailImage">新的缩略图</param>
        /// <param name="rowVersion">参考 RowVersion</param>
        [OperationContract]
        Task UpdateArticle(string id, string content, string thumbnailImage, byte[] rowVersion);
    }
}