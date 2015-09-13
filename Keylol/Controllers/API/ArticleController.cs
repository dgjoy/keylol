using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Keylol.Models;
using Keylol.Models.DTO;
using Keylol.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace Keylol.Controllers.API
{
    [Authorize]
    public class ArticleController : KeylolApiController
    {
        
    }
}