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
    public class DokumenController : Controller
    {
        //
        // GET: /Dokumen/
        private int moduleId = 36; 
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DaftarList()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListData(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "DOC_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("DOC_CREATE_DATE");
            order_field.Add("DOC_NAME");         



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;

            string where_clause = " DOC_FOLDER_ID = 5";
       
            string search_clause = "";
            if (search != "")
            {
                if (where_clause != "")
                {
                    search_clause += " AND ";
                }
                search_clause += "(";
                var i = 1;
                foreach (var fields in order_field)
                {
                    if (fields != "")
                    {
                        search_clause += "LOWER(" + fields + ")  LIKE LOWER('%" + search + "%')";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR LOWER(DOC_CREATE_DATE) = LOWER('%" + search + "%'))";
            }
            
            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM TRX_DOCUMENTS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //return Json(new { query = inject_clause_select }, JsonRequestBehavior.AllowGet);
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  TRX_DOCUMENTS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<TRX_DOCUMENTS>(inject_clause_select);

            var link = (from a in portaldb.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault();
            
            var no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                //Convert.ToString(list.DOC_CREATE_DATE),
                Convert.ToString(no++),
                Convert.ToString(list.DOC_NAME),
                Convert.ToString((Session["USER_ID"] != null)?"<a href='" + link.LINK_NAME + "" + list.DOC_FILE_PATH.Replace("/U", "U") + "" + list.DOC_FILE_NAME + "." + list.DOC_FILETYPE + "'><i class='fa fa-file'></i></a>":"<a href='javascript()'><i class='fa fa-file'></i></a>")                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);       
        }

    }
}
