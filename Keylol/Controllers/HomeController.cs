using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace Keylol.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}