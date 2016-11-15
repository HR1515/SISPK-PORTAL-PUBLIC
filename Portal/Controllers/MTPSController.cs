using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Helpers;
using SISPK.Models;
using System.IO;

namespace Portal.Controllers
{
    public class MTPSController : Controller
    {
        //
        // GET: /MTPS/
        private int moduleId = 9;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult Index()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

    }
}
