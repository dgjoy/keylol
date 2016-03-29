using System.Web.Http;

namespace Keylol.Controllers.UploadSignature
{
    /// <summary>
    /// 又拍云上传签名 Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("upload-signature")]
    public partial class UploadSignatureController : KeylolApiController
    {
        private const string FormKey = "LaetquRR2LDCO0SezzqNNeTxjnQ=";
    }
}