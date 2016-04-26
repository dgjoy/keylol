using System.Web.Http;
using Keylol.Models.DAL;

namespace Keylol.Controllers.UploadSignature
{
    /// <summary>
    ///     又拍云上传签名 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("upload-signature")]
    public partial class UploadSignatureController : ApiController
    {
        private readonly KeylolDbContext _dbContext;

        /// <summary>
        ///     创建 <see cref="UploadSignatureController" />
        /// </summary>
        /// <param name="dbContext">
        ///     <see cref="KeylolDbContext" />
        /// </param>
        public UploadSignatureController(KeylolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}