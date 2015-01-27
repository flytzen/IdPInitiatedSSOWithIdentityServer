using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RelyingParty.Controllers
{
    using System.Security.Claims;

    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return this.View();
        }

        [Authorize]
        public ActionResult Secure()
        {
            return this.View(this.User as ClaimsPrincipal);
        }
    }
}