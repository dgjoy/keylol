using System.ServiceModel;
using System.Threading.Tasks;
using Keylol.Models.DTO;

namespace Keylol.Services.Contracts
{
    /// <summary>
    ///     ImageGarage 协作器
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IImageGarageCoordinator
    {
        /// <summary>
        ///     心跳测试
        /// </summary>
        [OperationContract]
        void Ping();

        /// <summary>
        ///     获取指定 ID 的文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <returns>
        ///     <see cref="ArticleDto" />
        /// </returns>
        [OperationContract]
        Task<ArticleDto> FindArticle(string id);

        /// <summary>
        ///     更新指定文章
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <param name="content">新的内容</param>
        /// <param name="coverImage">新的封面图</param>
        /// <param name="rowVersion">参考 RowVersion</param>
        [OperationContract]
        Task UpdateArticle(string id, string content, string coverImage, byte[] rowVersion);

        /// <summary>
        ///     获取指定 ID 的文章评论
        /// </summary>
        /// <param name="id">文章评论 ID</param>
        /// <returns>
        ///     <see cref="ArticleCommentDto" />
        /// </returns>
        [OperationContract]
        Task<ArticleCommentDto> FindArticleComment(string id);

        /// <summary>
        ///     更新指定文章评论
        /// </summary>
        /// <param name="id">文章评论 ID</param>
        /// <param name="content">新的内容</param>
        /// <param name="rowVersion">参考 RowVersion</param>
        [OperationContract]
        Task UpdateArticleComment(string id, string content, byte[] rowVersion);
    }
}