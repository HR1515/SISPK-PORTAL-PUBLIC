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
    public class SNIController : Controller
    {
        //
        // GET: /SNI/
        private int moduleId = 17;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DaftarList()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["kt"] = (from a in db.MASTER_KOMITE_TEKNIS where a.KOMTEK_STATUS == 1 orderby a.KOMTEK_CODE select a).ToList();
            ViewData["ics"] = (from a in db.MASTER_ICS where a.ICS_STATUS == 1 orderby a.ICS_CODE select a).ToList();
            return View();
        }

        [HttpGet]
        public ActionResult DaftarList(string q = "")
        {
            ViewData["q"] = q;
            ViewData["moduleId"] = moduleId;
            ViewData["kt"] = (from a in db.MASTER_KOMITE_TEKNIS where a.KOMTEK_STATUS == 1 orderby a.KOMTEK_CODE select a).ToList();
            ViewData["ics"] = (from a in db.MASTER_ICS where a.ICS_STATUS == 1 orderby a.ICS_CODE select a).ToList();
            return View();
        }

        public ActionResult DetilSNI(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var sni = (from s in db.VIEW_SNI where s.SNI_ID == id select s).SingleOrDefault();
            ViewData["sni"] = sni;
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.ICS_CODE ascending select an).ToList();
            //var sni_merevisi = (from mr in db.VIEW_SNI where mr.PROPOSAL_REVISI_SNI_ID == sni.SNI_ID select mr).SingleOrDefault();

            //ViewData["merevisi"] = sni_merevisi;
            //ViewData["DataProposal"] = DataProposal;
            ViewData["link"] = (from c in portaldb.SYS_LINK where c.LINK_IS_USE == 1 select c).SingleOrDefault();
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            return View();
        }

        public ActionResult DetailSNI(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var sni = (from s in db.VIEW_SNI where s.SNI_ID == id select s).SingleOrDefault();
            ViewData["sni"] = sni;
            ViewData["link"] = (from c in portaldb.SYS_LINK where c.LINK_IS_USE == 1 select c).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == sni.PROPOSAL_ID orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + sni.PROPOSAL_ID + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();

            ViewData["link"] = (from c in portaldb.SYS_LINK where c.LINK_IS_USE == 1 select c).SingleOrDefault();
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;

            return View();
        }

        public ActionResult RekapSNI_ICS()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult List_Recap_SNI_ICS(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "ICS_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("ICS_CODE");
            order_field.Add("ICS_NAME");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR ICS_CODE = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_ICS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_ICS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_ICS>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.ICS_CODE +" - "+ list.ICS_NAME_IND),
                Convert.ToString((list.JML_PNPS > 0)?"<a href='Detail_RekapSNI_PNPS/"+list.ICS_CODE+"'>"+list.JML_PNPS+"</a>":""+list.JML_PNPS),
                Convert.ToString((list.JML_RSNI1 > 0)?"<a href='Detail_RekapSNI_RSNI1/"+list.ICS_CODE+"'>"+list.JML_RSNI1+"</a>":""+list.JML_RSNI1),
                Convert.ToString((list.JML_RSNI2 > 0)?"<a href='Detail_RekapSNI_RSNI2/"+list.ICS_CODE+"'>"+list.JML_RSNI2+"</a>":""+list.JML_RSNI2),
                Convert.ToString((list.JML_RSNI3 > 0)?"<a href='Detail_RekapSNI_RSNI3/"+list.ICS_CODE+"'>"+list.JML_RSNI3+"</a>":""+list.JML_RSNI3),
                Convert.ToString((list.JML_RSNI4 > 0)?"<a href='Detail_RekapSNI_RSNI4/"+list.ICS_CODE+"'>"+list.JML_RSNI4+"</a>":""+list.JML_RSNI4),
                Convert.ToString((list.JML_RSNI5 > 0)?"<a href='Detail_RekapSNI_RSNI5/"+list.ICS_CODE+"'>"+list.JML_RSNI5+"</a>":""+list.JML_RSNI5),
                Convert.ToString((list.JML_RASNI > 0)?"<a href='Detail_RekapSNI_RASNI/"+list.ICS_CODE+"'>"+list.JML_RASNI+"</a>":""+list.JML_RASNI),
                Convert.ToString((list.JML_SNI > 0)?"<a href='Detail_RekapSNI_SNI/"+list.ICS_CODE+"'>"+list.JML_SNI+"</a>":""+list.JML_SNI)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detil_RekapSNI_PNPS(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }
        public ActionResult Detail_RekapSNI_PNPS(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_PNPS(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 3";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //return Json(new { query = "SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count }, JsonRequestBehavior.AllowGet);
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);
            //return Json(new { query = inject_clause_select }, JsonRequestBehavior.AllowGet);

            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Detil_RekapSNI_RSNI1(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }
        public ActionResult Detail_RekapSNI_RSNI1(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }
        public ActionResult List_Recap_SNI_ICS_RSNI1(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 4";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),  
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detil_RekapSNI_RSNI2(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_RSNI2(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_RSNI2(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 5";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detil_RekapSNI_RSNI3(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_RSNI3(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_RSNI3(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 6";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),               
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detil_RekapSNI_RSNI4(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_RSNI4(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_RSNI4(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 7";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),              
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Detil_RekapSNI_RSNI5(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_RSNI5(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_RSNI5(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 8";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),              
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Detil_RekapSNI_RSNI6(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_RSNI6(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_RSNI6(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 9";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),            
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detil_RekapSNI_DT(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }
        public ActionResult Detil_RekapSNI_RASNI(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_RASNI(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_RASNI(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 10";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),             
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Detil_RekapSNI_SNI(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult Detail_RekapSNI_SNI(string id = "")
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["ics_name"] = (from ics in db.MASTER_ICS where ics.ICS_CODE == id select ics).SingleOrDefault();
            return View();
        }

        public ActionResult List_Recap_SNI_ICS_SNI(DataTables param, string id = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_ID IN (SELECT BB.PROPOSAL_ID FROM TRX_PROPOSAL BB JOIN TRX_PROPOSAL_ICS_REF AA ON AA.PROPOSAL_ICS_REF_PROPOSAL_ID = BB.PROPOSAL_ID INNER JOIN MASTER_ICS CC ON AA.PROPOSAL_ICS_REF_ICS_ID = CC.ICS_ID WHERE substr(CC.ICS_CODE,1,2) LIKE '" + id + "%' GROUP BY BB.PROPOSAL_ID) AND PROPOSAL_STATUS = 11";


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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),              
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG), 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RekapSNI_PT()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListSNI_PT(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "KOMTEK_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR KOMTEK_CODE = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_KOMTEK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_KOMTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_KOMTEK>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.KOMTEK_CODE +" - "+ list.KOMTEK_NAME),
                Convert.ToString((list.JML_PNPS > 0)?"<a href='SNI_Komtek_PNPS/"+list.KOMTEK_ID+"'>"+list.JML_PNPS+"</a>":""+list.JML_PNPS),
                Convert.ToString((list.JML_RSNI1 > 0)?"<a href='SNI_Komtek_RSNI1/"+list.KOMTEK_ID+"'>"+list.JML_RSNI1+"</a>":""+list.JML_RSNI1),
                Convert.ToString((list.JML_RSNI2 > 0)?"<a href='SNI_Komtek_RSNI2/"+list.KOMTEK_ID+"'>"+list.JML_RSNI2+"</a>":""+list.JML_RSNI2),
                Convert.ToString((list.JML_RSNI3 > 0)?"<a href='SNI_Komtek_RSNI3/"+list.KOMTEK_ID+"'>"+list.JML_RSNI3+"</a>":""+list.JML_RSNI3),
                Convert.ToString((list.JML_RSNI4 > 0)?"<a href='SNI_Komtek_RSNI4/"+list.KOMTEK_ID+"'>"+list.JML_RSNI4+"</a>":""+list.JML_RSNI4),
                Convert.ToString((list.JML_RSNI5 > 0)?"<a href='SNI_Komtek_RSNI5/"+list.KOMTEK_ID+"'>"+list.JML_RSNI5+"</a>":""+list.JML_RSNI5),
                Convert.ToString((list.JML_RSNI6 > 0)?"<a href='SNI_Komtek_RSNI6/"+list.KOMTEK_ID+"'>"+list.JML_RSNI6+"</a>":""+list.JML_RSNI6),
                Convert.ToString((list.JML_RASNI > 0)?"<a href='SNI_Komtek_RASNI/"+list.KOMTEK_ID+"'>"+list.JML_RASNI+"</a>":""+list.JML_RASNI),
                Convert.ToString((list.JML_SNI > 0)?"<a href='SNI_Komtek_SNI/"+list.KOMTEK_ID+"'>"+list.JML_SNI+"</a>":""+list.JML_SNI)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_PNPS(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_PNPS_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 3 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RSNI1(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RSNI1_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 4 AND KOMTEK_ID =" + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RSNI2(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RSNI2_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 5 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RSNI3(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RSNI3_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = " PROPOSAL_STATUS = 6 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RSNI4(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RSNI4_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 7 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RSNI5(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RSNI5_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 8 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RSNI6(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RSNI6_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 9 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_RASNI(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_RASNI_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JUDUL_PNPS_ENG");
            order_field.Add("KOMTEK_CODE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 10 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR PROPOSAL_NO_SNI_PROPOSAL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString("<a href ='../Detail_SNI/"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SNI_Komtek_SNI(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["komtek_id"] = id;
            return View();
        }

        public ActionResult SNI_Komtek_SNI_List(DataTables param, int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_JUDUL_ENG");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_STATUS = 11 AND KOMTEK_ID = " + id;

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_NOMOR),
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString(list.SNI_JUDUL_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),                 
                Convert.ToString("<a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SNI_Amandemen()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult List_SNI_Amandemen(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "DSK_DOC_NUMBER";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("DSK_DOC_NUMBER");
            order_field.Add("SNI_NOMOR");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "PROPOSAL_JENIS_PERUMUSAN = 4";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR DSK_DOC_NUMBER = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.DSK_DOC_NUMBER),
                Convert.ToString(list.SNI_NOMOR),
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString(list.DSK_DOC_NAME),
                Convert.ToString(list.SNI_JUDUL_ENG),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(""),
                Convert.ToString(""),
                Convert.ToString("") 
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        //SNI Per SK

        public ActionResult Rekap_SNI_SK()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListSniSK(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_YEAR_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_YEAR_NAME");
            order_field.Add("SNI_BARU");
            order_field.Add("SNI_MEREVISI");
            order_field.Add("SNI_DIREVISI");
            order_field.Add("SNI_ABOLISI");
            order_field.Add("DOK_BARU");
            order_field.Add("DOK_REVISI");
            order_field.Add("DOK_ABOLISI");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_YEAR_NAME = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString("<a href='Sni_SK_Tahun?thn="+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_SK_YEAR_NAME+"</a>"),
                Convert.ToString((list.SNI_BARU != 0)?"<a href='Sni_SK_Baru_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_BARU+"</a>":""+list.SNI_BARU),
                Convert.ToString((list.SNI_MEREVISI != 0)?"<a href='Sni_SK_Merevisi_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_MEREVISI+"</a>":""+list.SNI_MEREVISI),
                Convert.ToString((list.SNI_DIREVISI != 0)?"<a href='Sni_SK_Direvisi_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_DIREVISI+"</a>":""+list.SNI_DIREVISI),
                Convert.ToString((list.SNI_ABOLISI != 0)?"<a href='Sni_SK_Abolisi_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_ABOLISI+"</a>":""+list.SNI_ABOLISI),
                Convert.ToString((list.DOK_BARU != 0)?"<a href='Sni_SK_DOK_BARU_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.DOK_BARU+"</a>":""+list.DOK_BARU),
                Convert.ToString((list.DOK_REVISI != 0)?"<a href='Sni_SK_DOK_ABOLISI_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.DOK_ABOLISI+"</a>":""+list.DOK_ABOLISI),
                Convert.ToString((list.DOK_REVISI != 0)?"<a href='Sni_SK_DOK_REVISI_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.DOK_REVISI+"</a>":""+list.DOK_REVISI)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_Tahun(int thn = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = thn;
            return View();
        }

        public ActionResult Sni_SK_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_YEAR_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_YEAR_NAME");
            order_field.Add("SNI_BARU");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_YEAR_NAME = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString("<a href='Sni_SK_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_SK_YEAR_NAME+"</a>"),
                Convert.ToString((list.SNI_BARU != 0)?"<a href='Sni_SK_Baru_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_BARU+"</a>":""+list.SNI_BARU),
                Convert.ToString((list.SNI_MEREVISI != 0)?"<a href='Sni_SK_Merevisi_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_MEREVISI+"</a>":""+list.SNI_MEREVISI),
                Convert.ToString((list.SNI_DIREVISI != 0)?"<a href='Sni_SK_Direvisi_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_DIREVISI+"</a>":""+list.SNI_DIREVISI),
                Convert.ToString((list.SNI_ABOLISI != 0)?"<a href='Sni_SK_Abolisi_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.SNI_ABOLISI+"</a>":""+list.SNI_ABOLISI),
                Convert.ToString((list.DOK_BARU != 0)?"<a href='Sni_SK_DOK_BARU_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.DOK_BARU+"</a>":""+list.DOK_BARU),
                Convert.ToString((list.DOK_REVISI != 0)?"<a href='Sni_SK_DOK_REVISI_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.DOK_REVISI+"</a>":""+list.DOK_REVISI),
                Convert.ToString((list.DOK_REVISI != 0)?"<a href='Sni_SK_DOK_ABOLISI_Tahun/"+list.SNI_SK_YEAR_NAME+"'>"+list.DOK_ABOLISI+"</a>":""+list.DOK_ABOLISI)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_Baru_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_Baru_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_SK_YEAR_NAME = " + thn + " AND PROPOSAL_JENIS_PERUMUSAN = 1";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_Merevisi_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_Merevisi_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;

            var tahun = Convert.ToString(thn);
            var where_clause = "SNI_SK_YEAR_NAME = '" + tahun + "' AND PROPOSAL_JENIS_PERUMUSAN = 2 AND SNI_SK_ID IS NOT NULL";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[]
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_Direvisi_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_Direvisi_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_SK_YEAR_NAME = " + thn + " AND PROPOSAL_JENIS_PERUMUSAN = 2 AND IS_DIREVISI = 1";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_Abolisi_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_Abolisi_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_SK_YEAR_NAME = " + thn + " AND PROPOSAL_JENIS_PERUMUSAN = 3";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_DOK_BARU_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_DOK_BARU_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_YEAR_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_SK_YEAR_NAME = " + thn + " AND PROPOSAL_JENIS_PERUMUSAN = 1 AND PROPOSAL_IS_BATAL = 1";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_YEAR_NAME = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_DOK_REVISI_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_DOK_REVISI_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_YEAR_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_SK_YEAR_NAME = " + thn + " AND PROPOSAL_JENIS_PERUMUSAN = 2 AND PROPOSAL_IS_BATAL = 1";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_YEAR_NAME = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Sni_SK_DOK_ABOLISI_Tahun(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["tahun"] = id;
            return View();
        }

        public ActionResult Sni_SK_DOK_ABOLISI_Tahun_list(DataTables param, int thn = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_SK_YEAR_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_SK_YEAR_NAME = " + thn + " AND PROPOSAL_JENIS_PERUMUSAN = 2 AND PROPOSAL_IS_BATAL = 1";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_SK_YEAR_NAME = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REKAP_SNI_BY_SK_DETAIL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            //var CountData = db.Database.SqlQuery<REKAP_SNI_BY_SK>("SELECT * FROM VIEW");
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REKAP_SNI_BY_SK_DETAIL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REKAP_SNI_BY_SK_DETAIL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString(list.SNI_NOMOR +" "+ list.SNI_JUDUL)
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        //SNI Revisi Mulai
        public ActionResult SNI_Revisi()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListSniRevisi(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("PROPOSAL_REV_MERIVISI_NOMOR");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_REVISI_PORTAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_REVISI_PORTAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI_REVISI_PORTAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(""+((list.SNI_TIDAK_BERLAKU != 1)?"<i class='fa fa-check-circle' style='color:green'></i>":"<i class='fa fa-times-circle' style='color:red'></i>")+" <a href='/SNI/DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR +"</a><br />"+ list.SNI_JUDUL),
                GetInfoSNIRevisi(list.PROPOSAL_REV_MERIVISI_NOMOR)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public static string GetInfoSNIRevisi(string srev = "")
        {

            string Output = "";
            using (var db = new SISPKEntities())
            {
                string[] newSrev = srev.Split(',');
                if (newSrev.Count() > 0)
                {
                    Output += "<ul style='padding-left:15px;'>";
                    foreach (string i in newSrev)
                    {
                        var newData = db.Database.SqlQuery<TRX_SNI>("SELECT * FROM TRX_SNI WHERE SNI_ID = '" + i + "'").SingleOrDefault();
                        Output += "<li><a href='/SNI/DetailSNI/" + i + "'>" + newData.SNI_NOMOR + "</a><br>" + newData.SNI_JUDUL + "</li>";
                    }
                    Output += "</ul>";
                }
            }

            return Output;
        }

        public ActionResult SNI_AcuanNormatif()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListSniAcuanNormatif(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_JUDUL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_ID_ACUAN_NORMATIF");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_ACUAN_NORMATIF_PORTAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_ACUAN_NORMATIF_PORTAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI_ACUAN_NORMATIF_PORTAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(""+((list.SNI_TIDAK_BERLAKU != 1)?"<i class='fa fa-check-circle' style='color:green'></i>":"<i class='fa fa-times-circle' style='color:red'></i>")+" <a href='DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR +"</a><br />"+ list.SNI_JUDUL),
                GetInfoAcuanNormatif(list.SNI_ID_ACUAN_NORMATIF)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public static string GetInfoAcuanNormatif(string acnor = "")
        {

            string Output = "";
            using (var db = new SISPKEntities())
            {
                string[] newacnor = acnor.Split(',');
                if (newacnor.Count() > 0)
                {
                    Output += "<ul style='padding-left:15px;'>";
                    foreach (string i in newacnor)
                    {
                        var newData = db.Database.SqlQuery<TRX_SNI>("SELECT * FROM TRX_SNI WHERE SNI_ID = '" + i + "'").SingleOrDefault();
                        Output += "<li><a href='/DetailSNI/" + i + "'>" + newData.SNI_NOMOR + "</a><br>" + newData.SNI_JUDUL + "</li>";
                    }
                    Output += "</ul>";
                }
            }


            return Output;
        }

        public ActionResult SNI_Adopsi()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListSniAdopsi(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("PROPOSAL_ADOPSI_TYPE");
            order_field.Add("PROPOSAL_ADOPSI_NOMOR_JUDUL");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_ADOPSI_PORTAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_ADOPSI_PORTAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI_ADOPSI_PORTAL>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(((list.SNI_TIDAK_BERLAKU != 1)?"<i class='fa fa-check-circle' style='color:green'></i>":"<i class='fa fa-times-circle' style='color:red'></i>")+" <a href='DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR +"</a><br />"+ list.SNI_JUDUL),
                Convert.ToString(((list.PROPOSAL_ADOPSI_TYPE == 1)?"Identik":"Modifikasi")),
                Convert.ToString(list.PROPOSAL_ADOPSI_NOMOR_JUDUL)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AcuanNormatifStandarAsing()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListSniAcuanStandarAsing(DataTables param)
        {
            //var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_ID_ACUAN_NORMATIF");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "";

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ANOR_STANDAR_ASING WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ANOR_STANDAR_ASING " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_ANOR_STANDAR_ASING>(inject_clause_select);

            //return Json(new { query = SelectedData }, JsonRequestBehavior.AllowGet);
            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(((list.SNI_TIDAK_BERLAKU != 1)?"<i class='fa fa-check-circle' style='color:green'></i>":"<i class='fa fa-times-circle' style='color:red'></i>")+" <a href='DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR +"</a><br />"+ list.SNI_JUDUL),
                GetInfoAcuanNormatifAsing(list.SNI_ID_ACUAN_NORMATIF)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public static string GetInfoAcuanNormatifAsing(string acnor = "")
        {

            string Output = "";
            using (var db = new SISPKEntities())
            {
                string[] newacnor = acnor.Split(',');
                if (newacnor.Count() > 0)
                {
                    Output += "<ul style='padding-left:15px;'>";
                    foreach (string i in newacnor)
                    {
                        var newData = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_ID = '" + i + "'").SingleOrDefault();
                        Output += "<li>" + newData.ACUAN_NON_SNI_JUDUL + "</li>";
                    }
                    Output += "</ul>";
                }
            }


            return Output;
        }
        [HttpGet]
        public ActionResult ListDataSNI(DataTables param, string nomor = "", int komtek = 0, string judul = "", int tahun = 0, int status = 0, int status_sni = 2, string ics = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            //order_field.Add("SNI_ID");
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_JUDUL_ENG");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_ICS_NAME");
            order_field.Add("SNI_TIDAK_BERLAKU");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "DESC" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            //var where_clause = "";
            string where_clause = "SNI_STATUS = 1 AND SNI_IS_PUBLISH = 1";
            if (nomor != "")
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " PROPOSAL_NO_SNI_PROPOSAL LIKE '%" + nomor + "%'";
            }
            if (komtek != 0)
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " PROPOSAL_KOMTEK_ID IN (SELECT KOMTEK_ID FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_STATUS = 1 START WITH KOMTEK_ID = '" + komtek + "' CONNECT BY KOMTEK_PARENT_CODE = PRIOR KOMTEK_CODE)";
            }
            if (judul != "")
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " LOWER(PROPOSAL_JUDUL_PNPS) LIKE LOWER('%" + judul + "%')";
            }
            if (tahun != 0)
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " PROPOSAL_YEAR = '" + tahun + "'";
            }
            if (status_sni != 2)
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " SNI_TIDAK_BERLAKU = '" + status_sni + "'";
            }
            if (status != 0)
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " PROPOSAL_JENIS_PERUMUSAN = " + status + "";
            }
            if (ics != "")
            {
                var GetIcs = db.Database.SqlQuery<TRX_PROPOSAL_ICS_REF>("SELECT T1.* FROM TRX_PROPOSAL_ICS_REF T1 INNER JOIN MASTER_ICS T2 ON T1.PROPOSAL_ICS_REF_ICS_ID = T2.ICS_ID WHERE T2.ICS_CODE = '" + ics + "' OR T2.ICS_PARENT_CODE LIKE '" + ics + "%'").ToList();
                if (GetIcs.Count > 0)
                {
                    var last = GetIcs.Last();
                    where_clause += ((where_clause != "") ? " AND (" : " ( ");
                    foreach (var newics in GetIcs)
                    {
                        if (newics.Equals(last))
                        {
                            where_clause += " PROPOSAL_ID = " + newics.PROPOSAL_ICS_REF_PROPOSAL_ID;
                        }
                        else
                        {
                            where_clause += " PROPOSAL_ID = " + newics.PROPOSAL_ICS_REF_PROPOSAL_ID +"  OR ";
                        }

                    }
                    where_clause += " ) ";
                }

            }

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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);
            var link = (from t in portaldb.SYS_LINK where t.LINK_IS_USE == 1 select t).SingleOrDefault();

            var no = ((start == 0) ? 1 : start + 1);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.SNI_ID),
                Convert.ToString(no++),
                Convert.ToString("<img border='0' width='15' height='15' src='../Assets/Image_plus/accept-icon.jpg'> <a href='DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR+"</a>"),
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString(list.SNI_JUDUL_ENG),
                Convert.ToString("<a href='../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>"),  
                Convert.ToString(list.PROPOSAL_ICS_NAME),
                Convert.ToString((list.SNI_TIDAK_BERLAKU == 1)?"Sudah tidak berlaku":"Masih Berlaku"),
                Convert.ToString((Session["USER_ID"] != null && list.DSNI_DOC_FILE_NAME != "SNI" && list.SNI_TIDAK_BERLAKU == 0 && list.PROPOSAL_IS_BATAL == 0)?"<center><a href='"+link.LINK_NAME+"Download/Files?fid="+list.PROPOSAL_ID+"&token_key="+Session["TOKEN_KEY"]+"&uid="+Session["USER_ID"]+"'><img border='0' width='15' height='15' alt='download pdf' src='http://sisni.bsn.go.id//static/images/pdf.ico'></a></center>":"-") 
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray(),
                inject_clause_select
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult List_sni_komtek(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            //order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("INSTANSI_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_STATUS = 1 ";



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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);



            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<img border='0' width='15' height='15' src='http://sisni.bsn.go.id//static/images/accept-icon.jpg'> <a href='DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR+"</a>"),
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString("-"),
                Convert.ToString("<a href='../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>"),  
                Convert.ToString(list.PROPOSAL_ICS_NAME),
                Convert.ToString("Masih Berlaku"),
                Convert.ToString("<center><a href='"+list.DSNI_DOC_LINK+"'><img border='0' width='15' height='15' alt='download pdf' src='http://sisni.bsn.go.id//static/images/pdf.ico'></a></center>") 
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ICS_Detail(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics"] = (from a in db.VIEW_ICS where a.ICS_ID == id select a).SingleOrDefault();
            return View();
        }

        public ActionResult List_ics_sni(DataTables param, int id = 0)
        {
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            //order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("ICS_CODE");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            var where_clause = "SNI_STATUS = 1 AND ICS_ID = " + id;



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
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ICS_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ICS_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_ICS_SNI>(inject_clause_select);
            var no = ((start == 0) ? 1 : start + 1);


            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString("<img border='0' width='15' height='15' src='http://sisni.bsn.go.id//static/images/accept-icon.jpg'> <a href='../DetailSNI/"+list.SNI_ID+"'>"+list.SNI_NOMOR+"</a>"),
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString(list.SNI_JUDUL_ENG),
                Convert.ToString("<center><a href='../../PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a></center>"),
                Convert.ToString("<center><a href='/SNI/ics_detail/"+list.ICS_ID+"'>"+list.ICS_CODE+"</a></center>") 
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ICS_Detail_list(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            ViewData["ics_id"] = id;
            ViewData["data_ics"] = (from a in db.VIEW_ICS where a.ICS_ID == id && a.ICS_STATUS == 1 select a).SingleOrDefault();
            return View();
        }
    }
}