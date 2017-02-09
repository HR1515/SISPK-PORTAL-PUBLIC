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
    public class LPKController : Controller
    {
        //
        // GET: /LPK/
        private int moduleId = 33;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LembagaSertifikasi()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult LembagaInspeksi()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListdataLemsert(DataTables param) {
            var default_order = "LPK_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("LPK_NAMA");
            order_field.Add("LPK_NOMOR");
            order_field.Add("LPK_ALAMAT");
            order_field.Add("LPK_TELEPON");
            order_field.Add("LPK_EMAIL");
            order_field.Add("LPK_PERIODE_AWAL");
            order_field.Add("LPK_PERIODE_AKHIR");
            order_field.Add("LPK_LINGKUP_NAME");
            order_field.Add("LPK_CONTACT_PERSON");
            order_field.Add("JML_SNI");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = " LPK_STATUS = 1 AND LPK_KATEGORI = 1";

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
                search_clause += " OR LOWER(LPK_NAMA) = LOWER('%" + search + "%'))";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_LPK WHERE " + where_clause + " " + search_clause + " ORDER BY LPK_ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_LPK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_LPK>(inject_clause_select);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<table style='width:100%'>"+
                                    "<tr>"+
                                    "    <td style='vertical-align: top; width: 4%;'><b>"+(no++)+".</b></td>"+
                                        "<td style='vertical-align: top; color: green;' colspan='3'><a href='/LPK/lemsert_detail/"+list.LPK_ID+"' style='font-weight:bold;'>"+list.LPK_NAMA+"</a></td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>No LPK</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td><span style='color: green'>"+list.LPK_NOMOR+"</span></td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Lingkup</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_LINGKUP_NAME+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Alamat</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_ALAMAT+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Telpon</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_TELEPON+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Email</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_EMAIL+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Contact Person</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_CONTACT_PERSON+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Periode Akreditasi</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_PERIODE_AWAL_DATE_NUMBER+" - "+list.LPK_PERIODE_AKHIR_DATE_NUMBER+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>SNI yang Terkait</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.JML_SNI+" SNI</td>"+
                                    "</tr>"+
                                "</table>")

            };
            
            //var result = from lpk in SelectedData
            //             select new
            //             {
            //                 LPK_NAMA = "<a href='lemsert_detail/"+lpk.LPK_ID+"'>" + lpk.LPK_NAMA + "</a>",
            //                 LPK_NOMOR = lpk.LPK_NOMOR,
            //                 LPK_LINGKUP = lpk.LPK_LINGKUP,
            //                 LPK_LINGKUP_DETAIL = lpk.LPK_LINGKUP_DETAIL,
            //                 LPK_ALAMAT = lpk.LPK_ALAMAT,
            //                 LPK_TELEPON = lpk.LPK_TELEPON,
            //                 LPK_EMAIL = lpk.LPK_EMAIL,
            //                 LPK_CONTACT_PERSON = lpk.LPK_CONTACT_PERSON.Replace("|",", "),
            //                 PERIODE = "<center>" + lpk.LPK_PERIODE_AWAL_DATE_NUMBER + "<br /> s/d <br />" + lpk.LPK_PERIODE_AKHIR_DATE_NUMBER + "</center>",
            //                 JML_SNI = lpk.JML_SNI +" SNI"                             
            //             };


            return Json(new
            {
                draw = param.sEcho,
                recordsTotal = CountData,
                recordsFiltered = CountData,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ListLabolatoriumInspeksi(DataTables param) {
            var default_order = "LPK_NAMA";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("LPK_NAMA");
            order_field.Add("LPK_NOMOR");
            order_field.Add("LPK_LINGKUP");
            order_field.Add("LPK_ALAMAT");
            order_field.Add("LPK_TELEPON");
            order_field.Add("LPK_EMAIL");
            order_field.Add("LPK_CONTACT_PERSON");
            order_field.Add("LPK_PERIODE_AWAL");
            order_field.Add("LPK_PERIODE_AKHIR");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = " LPK_STATUS = 1 AND LPK_KATEGORI != 1";

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
                search_clause += " OR LOWER(LPK_NAMA) = LOWER('%" + search + "%'))";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_LPK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_LPK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_LPK>(inject_clause_select);

            var link = (from a in portaldb.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault();
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
              Convert.ToString("<table style='width:100%'>"+
                                    "<tr>"+
                                    "    <td style='vertical-align: top; width: 4%;'><b>"+(no++)+".</b></td>"+
                                        "<td style='vertical-align: top; color: green;' colspan='3'><a href='/LPK/lemins_detail/"+list.LPK_ID+"' style='font-weight:bold;'>"+list.LPK_NAMA+"</a></td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>No LPK</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td><span style='color: green'>"+list.LPK_NOMOR+"</span></td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Lingkup</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_LINGKUP_NAME+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Lingkup Detail</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>-</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Alamat</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_ALAMAT+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Telpon</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_TELEPON+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Website</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_WEBSITE+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Email</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_EMAIL+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Contact Person</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_CONTACT_PERSON+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>Batas Akhir Akreditasi</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.LPK_PERIODE_AKHIR_DATE_NUMBER+"</td>"+
                                    "</tr>"+
                                    "<tr>"+
                                        "<td style='vertical-align: top; width: 1%'></td>"+
                                        "<td style='vertical-align: top; width: 20%'><b>SNI yang Terkait</b></td>"+
                                        "<td style='vertical-align: top; width: 1%'><b>:</b></td>"+
                                        "<td>"+list.JML_SNI+" SNI</td>"+
                                    "</tr>"+
                                "</table>")
            };
            //var result = from lpk in SelectedData
            //             select new
            //             {
            //                 LPK_NAMA = "<a href='lemins_detail/" + lpk.LPK_ID + "'>" + lpk.LPK_NAMA + "</a>",
            //                 LPK_NOMOR = lpk.LPK_NOMOR,
            //                 LPK_LINGKUP = lpk.LPK_LINGKUP,
            //                 LPK_LINGKUP_DETAIL = (lpk.LPK_LAMPIRAN != null) ? "<a href='" + link.LINK_NAME + "" + lpk.LPK_LAMPIRAN.Replace("/U", "U") + "'><img border='0' width='15' height='15' alt='download' src='http://sisni.bsn.go.id//static/images/pdf.ico'></a>" : "-",
            //                 LPK_ALAMAT = lpk.LPK_ALAMAT,
            //                 LPK_TELEPON = lpk.LPK_TELEPON,
            //                 LPK_EMAIL = lpk.LPK_EMAIL,
            //                 LPK_CONTACT_PERSON = lpk.LPK_CONTACT_PERSON.Replace("|", ", "),
            //                 PERIODE = lpk.LPK_PERIODE_AKHIR_DATE_NUMBER,
            //                 JML_SNI = lpk.JML_SNI + " SNI"
            //             };

            return Json(new
            {
                draw = param.sEcho,
                recordsTotal = CountData,
                recordsFiltered = CountData,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult lemsert_detail(int id = 0) {
            ViewData["moduleId"] = moduleId;
            var lemsert = (from a in db.VIEW_LPK where a.LPK_ID == id select a).SingleOrDefault();
            ViewData["lemsert"] = lemsert;
            ViewData["kab"] = (from c in db.VIEW_WILAYAH_KABUPATEN where c.WILAYAH_ID == lemsert.LPK_KABUPATENKOTA select c).SingleOrDefault();
            ViewData["prov"] = (from c in db.VIEW_WILAYAH_PROVINSI where c.WILAYAH_ID == lemsert.LPK_PROVINSI select c).SingleOrDefault();
            ViewData["detail"] = (from b in db.VIEW_LPK_DETAIL where b.LPK_LINGKUP_LPK_ID == id && b.LPK_LINGKUP_STATUS == 1 select b).ToList();
            ViewData["sni"] = (from a in db.VIEW_LPK_SNI where a.LPK_DETAIL_SNI_LPK_ID == id && a.LPK_DETAIL_SNI_STATUS == 1 select a).ToList();
            ViewData["scope"] = (from s in db.VIEW_LPK_SCOPE where s.LPK_SCOPE_LPK_ID == id && s.LPK_SCOPE_STATUS == 1 select s).ToList();
            return View();
        }

        public ActionResult lemins_detail(int id = 0) {
            ViewData["moduleId"] = moduleId;
            var lemins = (from a in db.VIEW_LPK where a.LPK_ID == id select a).SingleOrDefault();
            ViewData["lemins"] = lemins;
            ViewData["kab"] = (from c in db.VIEW_WILAYAH_KABUPATEN where c.WILAYAH_ID == lemins.LPK_KABUPATENKOTA select c).SingleOrDefault();
            ViewData["prov"] = (from c in db.VIEW_WILAYAH_PROVINSI where c.WILAYAH_ID == lemins.LPK_PROVINSI select c).SingleOrDefault();
            ViewData["detail"] = (from b in db.VIEW_LPK_DETAIL where b.LPK_LINGKUP_LPK_ID == id && b.LPK_LINGKUP_STATUS == 1 select b).ToList();
            ViewData["sni"] = (from a in db.VIEW_LPK_SNI where a.LPK_DETAIL_SNI_LPK_ID == id && a.LPK_DETAIL_SNI_STATUS == 1 select a).ToList();
            ViewData["scope"] = (from s in db.VIEW_LPK_SCOPE where s.LPK_SCOPE_LPK_ID == id && s.LPK_SCOPE_STATUS == 1 select s).ToList();
            var link = (from a in portaldb.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault();
            ViewData["link"] = link;
            return View();
        }

    }
}
