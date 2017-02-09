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
    public class RegulasiTeknisController : Controller
    {
        //
        // GET: /RegulasiTeknis/
        private int moduleId = 30;
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

        public ActionResult SniWajib()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult list_SniWajib(DataTables param)
        {

            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            order_field.Add("REGULATOR_CODE");
            order_field.Add("RETEK_NO_SK");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "";

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
                search_clause += " OR LOWER(SNI_NOMOR) = LOWER('%" + search + "%'))";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_WAJIB WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //return Json(new
            //{
            //    aaData = inject_clause_select
            //}, JsonRequestBehavior.AllowGet);
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_WAJIB " + inject_clause_count);

            var SelectedData = db.Database.SqlQuery<VIEW_SNI_WAJIB>(inject_clause_select);
            var userid = Convert.ToInt32(Session["USER_ID"]);

            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString("<a href='../SNI/DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR+"</a>"),
                Convert.ToString(list.SNI_JUDUL),                
                Convert.ToString(list.REGULATOR_CODE),          
                Convert.ToString(list.RETEK_NO_SK)

            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DaftarRegulasi_teknis()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }
        public static string GetInfoSNIRegtek(int regtek_id = 0)
        {
            
            string Output = "";
            using (var db = new SISPKEntities())
            {
                var datasni = db.Database.SqlQuery<VIEW_SNI>("SELECT T2.* FROM TRX_REGULASI_TEKNIS_DETAIL T1 INNER JOIN VIEW_SNI T2 ON T1.RETEK_DETAIL_SNI_ID = T2.SNI_ID WHERE T1.RETEK_DETAIL_RETEK_ID = '" + regtek_id + "'").ToList();
                if (datasni != null) {
                    Output += "<ol style='padding-left:15px;'>";
                    foreach (var i in datasni) {
                        Output += "<li><a href='/SNI/DetailSNI/" + i.SNI_ID + "'>" + i.SNI_NOMOR + "</a> " + i.SNI_JUDUL + "</li>";
                    }
                    Output += "</ol>";
                }
            }

            return Output;
        }
        public ActionResult List_regulasi_teknis(DataTables param)
        {
            var default_order = "RETEK_NO_SK";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("RETEK_NO_SK");
            order_field.Add("RETEK_TENTANG");
            order_field.Add("REGULATOR");
            order_field.Add("RETEK_KETERANGAN");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "RETEK_STATUS = 1";

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
                search_clause += " OR LOWER(RETEK_NO_SK) = LOWER('%" + search + "%'))";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REGTEK WHERE " + where_clause + " " + search_clause + " ORDER BY RETEK_ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REGTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REGTEK>(inject_clause_select);
            var link = (from a in portaldb.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault();
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<table style='width:100%'><tr><td style='vertical-align:top;width:5%'><b>"+no+++". </b></td><td style='vertical-align:top;width:10%'><b>No. SK </b></td><td style='vertical-align:top;width:1%'> : </td><td style='vertical-align:top;width:87%'><span style='color:green'>"+list.RETEK_NO_SK+"</span> "+((list.RETEK_FILE != null) ? "<a href='"+link.LINK_NAME +""+ list.DOC_FILE_PATH + "/" + list.DOC_FILE_NAME + "." + list.DOC_FILETYPE + "' download><img border='0' width='15' height='15' alt='download' src='http://sisni.bsn.go.id//static/images/pdf.ico'></a>" : "")+"</td></tr><tr><td style='vertical-align:top;width:2%'></td><td style='vertical-align:top;width:10%'><b>Tentang</b></td><td style='vertical-align:top;width:1%'> : </td><td style='vertical-align:top;width:87%'>"+list.RETEK_TENTANG+"</td></tr><tr><td style='vertical-align:top;width:2%'></td><td style='vertical-align:top;width:10%'><b>Regulator</b></td><td style='vertical-align:top;width:1%'> : </td><td style='vertical-align:top;width:87%'>"+list.REGULATOR+"</td></tr><tr><td></td><td><b>SNI Terkait</b></td><td>:</td></tr><tr><td></td><td colspan='3'>"+GetInfoSNIRegtek(Convert.ToInt32(list.RETEK_ID))+"</td></tr><tr><td></td><td><b>Keterangan</b></td><td colspan='2'>:</td></tr></tr><tr><td></td><td><b>"+list.RETEK_KETERANGAN+"</b></td></tr></table>")

            };
            //var regtek = from reg in SelectedData
            //             select new
            //             {
            //                 RETEK_NO_SK = "<a href='Detail_regtek/" + reg.RETEK_ID + "'>" + reg.RETEK_NO_SK + "</a>",
            //                 RETEK_TENTANG = reg.RETEK_TENTANG,
            //                 REGULATOR = reg.REGULATOR,
            //                 RETEK_KETERANGAN = reg.RETEK_KETERANGAN,
            //                 RETEK_FILE = (reg.RETEK_FILE != null) ? "<a href='"+link.LINK_NAME +""+ reg.DOC_FILE_PATH + "/" + reg.DOC_FILE_NAME + "." + reg.DOC_FILETYPE + "' download><img border='0' width='15' height='15' alt='download' src='http://sisni.bsn.go.id//static/images/pdf.ico'></a>" : "-",
            //                 RETEK_SNI_TERKAIT = "-"
            //             };


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail_regtek(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var reg = (from t in db.VIEW_REGTEK where t.RETEK_ID == id select t).SingleOrDefault();
            ViewData["data_regtek"] = reg;
            //return Json(new
            //{
            //    list = reg
            //}, JsonRequestBehavior.AllowGet);
            ViewData["link"] = (from a in portaldb.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault();
            return View();
        }

    }
}
