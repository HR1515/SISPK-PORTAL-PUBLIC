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
    public class RSNIController : Controller
    {
        private int moduleId = 14;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PerumusanRsni()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["kt"] = (from a in db.MASTER_KOMITE_TEKNIS where a.KOMTEK_STATUS == 1 orderby a.KOMTEK_CODE select a).ToList();
            ViewData["ics"] = (from a in db.MASTER_ICS where a.ICS_STATUS == 1 orderby a.ICS_CODE select a).ToList();
            return View();
        }
        [HttpGet]
        public ActionResult ListRsni(DataTables param, string nomor = "", int komtek = 0, string judul = "", int tahun = 0, int status = 0, string tahapan = "", string ics = "")
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_TAHAPAN";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_TAHAPAN");
            order_field.Add("PROPOSAL_STATUS_NAME");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_YEAR");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_RUANG_LINGKUP");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");



            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;

            string where_clause = "";
            if (nomor != "")
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " PROPOSAL_NO_SNI_PROPOSAL LIKE '%" + nomor + "%'";
            }
            if (komtek != 0)
            {
                //where_clause += ((where_clause != "") ? "AND" : "") + " PROPOSAL_KOMTEK_ID = '" + komtek + "'";
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
            if (status != 0)
            {
                where_clause += ((where_clause != "") ? " AND " : "") + " PROPOSAL_JENIS_PERUMUSAN = '" + status + "'";
            }
            if (ics != "")
            {
                var GetIcs = db.Database.SqlQuery<MASTER_ICS>("SELECT * FROM MASTER_ICS WHERE ICS_CODE LIKE  '" + ics + "%'").ToList();
                if (GetIcs.Count > 0)
                {
                    var last = GetIcs.Last();
                    where_clause += ((where_clause != "") ? " AND (" : " ( ");
                    foreach (var newics in GetIcs)
                    {
                        if (newics.Equals(last))
                        {
                            where_clause += " PROPOSAL_ICS LIKE '%" + newics.ICS_ID + "%'";
                        }
                        else
                        {
                            where_clause += " PROPOSAL_ICS LIKE '%" + newics.ICS_ID + "%' OR ";
                        }
                        
                    }
                    where_clause += " ) ";
                }

            }
            if (tahapan != "")
            {
                where_clause += ((where_clause != "") ? "AND" : "") + " PROPOSAL_TAHAPAN = '" + tahapan + "'";
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
                //string nomor ="",int komtek = 0,string judul ="",int tahun = 0,string status = "",string tahapan = "",int ics = 0

                search_clause += " OR PROPOSAL_TAHAPAN = '" + tahapan + "')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_RSNI_PORTAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_RSNI_PORTAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_RSNI_PORTAL>(inject_clause_select);

            var no = ((start == 0) ? 1 : start + 1);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++),
                Convert.ToString(list.PROPOSAL_TAHAPAN),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString("<a href='/PanitiaTeknis/Detailkomtek/"+list.PROPOSAL_KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>"),
                Convert.ToString(list.PROPOSAL_YEAR),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'><a href='/RSNI/DetailRSNI/"+list.PROPOSAL_ID+"'>"+ list.PROPOSAL_JUDUL_PNPS+"</a></span>"),
                Convert.ToString(list.PROPOSAL_JUDUL_PNPS_ENG),
                Convert.ToString(list.PROPOSAL_ICS_NAME),
                Convert.ToString(list.PROPOSAL_RUANG_LINGKUP),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME)
            };
            return Json(new
            {
                search,
                inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
            //return Json(new
            //{
            //    inject_clause_select
            //}, JsonRequestBehavior.AllowGet);
        }

        public static string GetListICS(string ics = "")
        {

            string Output = "";
            using (var db = new SISPKEntities())
            {
                string[] newIcs = ics.Split(',');
                if (newIcs.Count() > 0)
                {
                    foreach (string i in newIcs)
                    {
                        var newData = db.Database.SqlQuery<MASTER_ICS>("SELECT * FROM MASTER_ICS WHERE ICS_ID = '" + i + "'").SingleOrDefault();
                        Output += "<center>" + newData.ICS_CODE + "</center><br />";
                    }
                }
            }


            return Output;
        }

        public ActionResult DetailRSNI(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            ViewData["DataProposal"] = DataProposal;
            var mon = (from a in db.TRX_MONITORING where a.MONITORING_PROPOSAL_ID == id select a).SingleOrDefault();
            ViewData["monitoring"] = mon;
            return View();
        }

        public ActionResult MonitoringRSNI()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListMonitoringRsni(DataTables param)
        {

            var default_order = "PROPOSAL_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_ID");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_RUANG_LINGKUP");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROGRESS");
            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS < 11 AND (PROPOSAL_IS_BATAL <> 1 OR PROPOSAL_IS_BATAL IS NULL)";

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
                search_clause += " OR PROPOSAL_CREATE_DATE_NAME = '%" + search + "%')";
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

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME), 
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center><ul class='progress'><li class='usulan_0'>Usulan</li><li class='usulan_2'>MTPS</li><li class='usulan_3'><a href='/Pengajuan/Usulan'>PNPS</a></li><li class='usulan_4'><a href='/Perumusan/RSNI1'>RSNI1</a></li><li class='usulan_5'><a href='/Perumusan/RSNI2'>RSNI2</a></li><li class='usulan_6'><a href='/Perumusan/RSNI3'>RSNI3</a></li><li class='usulan_7'><a href='/Perumusan/RSNI4'>RSNI4</a></li><li class='usulan_8'><a href='/Perumusan/RSNI5'>RSNI5</a></li><li class='usulan_9'><a href='/Perumusan/RSNI6'>RSNI6</a></li><li class='usulan_10'><a href='/Perumusan/RASNI'>RASNI</a></li><li class='usulan_11'><a href='/SNI/SNIList'>SNI</a></li></ul></center>"),
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==0 && list.PROPOSAL_APPROVAL_STATUS == 1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            var sni = from cust in SelectedData
                      select new
                      {
                          PROPOSAL_ID = cust.PROPOSAL_ID,
                          PROPOSAL_PNPS_CODE = cust.PROPOSAL_PNPS_CODE,
                          PROPOSAL_JENIS_PERUMUSAN_NAME = cust.PROPOSAL_JENIS_PERUMUSAN_NAME,
                          PROPOSAL_JUDUL_PNPS = "<a href='/RSNI/DetailRSNI/" + cust.PROPOSAL_ID + "'>" + cust.PROPOSAL_JUDUL_PNPS + "</a>",
                          PROPOSAL_RUANG_LINGKUP = cust.PROPOSAL_RUANG_LINGKUP,
                          KOMTEK_CODE = cust.KOMTEK_CODE,
                          KOMTEK_NAME = cust.KOMTEK_NAME,
                          KOMTEK_FULLNAME = cust.KOMTEK_CODE + " " + cust.KOMTEK_NAME,
                          PROGRESS = cust.PROGRESS.Replace("/Pengajuan/Usulan", "javascript:void(0)").Replace("/Perumusan/RSNI1", "javascript:void(0)").Replace("/Perumusan/RSNI2", "javascript:void(0)").Replace("/Perumusan/RSNI3", "javascript:void(0)").Replace("/Perumusan/RSNI4", "javascript:void(0)").Replace("/Perumusan/RSNI5", "javascript:void(0)").Replace("/Perumusan/RSNI6", "javascript:void(0)").Replace("/Perumusan/RASNI", "javascript:void(0)").Replace("/SNI/SNIList", "javascript:void(0)")
                      };
            return Json(new
            {
                //wew = inject_clause_select,
                draw = param.sEcho,
                recordsTotal = CountData,
                recordsFiltered = CountData,
                data = sni
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
