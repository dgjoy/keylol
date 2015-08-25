using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Keylol.Models;
using Keylol;

namespace Keylol.Controllers.API
{
    public class SampleController : ApiController
    {
        [Authorize]
        public Like Get()
        {
            return new ArticleLike();
        }
    }
}
