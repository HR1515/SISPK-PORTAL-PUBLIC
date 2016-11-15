using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Helpers;
using SISPK.Models;

namespace Portal.Controllers
{
    public class NewsController : Controller
    {
 
        // GET: /News/
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portal = new PortalBsnEntities();
        private int moduleId = 0;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult News_detail(int id = 0) {
            ViewData["moduleId"] = moduleId;
            ViewData["link"] = (from a in portal.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault();
            ViewData["news_detail"] = (from news in db.VIEW_NEWS where news.NEWS_ID == id select news).SingleOrDefault();
            ViewData["hot_news"] = db.Database.SqlQuery<VIEW_NEWS>("select * from ( select * from VIEW_NEWS VN order by VN.NEWS_CREATE_DATE desc ) where ROWNUM <= 3");
            return View();
        }

    }
}
