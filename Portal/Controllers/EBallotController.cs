using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Helpers;
using SISPK.Models;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Oracle;
using Oracle.DataAccess.Client;

namespace Portal.Controllers
{
    public class EBallotController : Controller
    {
        //
        // GET: /EBallot/

        private int moduleId = 27;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DJPPS() {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult LHAS() {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult list_DJPPS(DataTables param) {

            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("KOMTEK_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "POLLING_STATUS = 1 AND POLLING_IS_KUORUM = 0 AND POLLING_TYPE = 7 AND POLLING_MONITORING_TYPE = 'Sedang Berlangsung'";

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
                search_clause += " OR PROPOSAL_JUDUL_PNPS = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_POLLING WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_POLLING " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_POLLING>(inject_clause_select);
            var userid = Convert.ToInt32(Session["USER_ID"]);
            var result = from EBA in SelectedData
                         select new string[] 
            { 
                Convert.ToString("Nomor : <span style='color:green'>" +EBA.PROPOSAL_NO_SNI_PROPOSAL +"</span><br />"+"Judul : "+EBA.PROPOSAL_JUDUL_PNPS+" <br />Komite Teknis : <b>"+EBA.KOMTEK_CODE+" - "+EBA.KOMTEK_NAME+" </b><br> ICS : <b>"+EBA.PROPOSAL_ICS_NAME+"</b>"),
                Convert.ToString(EBA.POLLING_FULL_DATE_NAME.Replace("<br>", " ") +"<br> ["+ EBA.POLLING_MONITORING_NAME +"]"),                
                Convert.ToString(EBA.PROPOSAL_RUANG_LINGKUP),          
                Convert.ToString("<a href='JajakPendapat/"+EBA.POLLING_ID+"' ><i class='fa fa-comments'></i>"+EBA.POLLING_JML_PARTISIPAN+"</a>")

            };
            

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult list_LHAS(DataTables param)
        {

            var default_order = "PROPOSAL_NO_SNI_PROPOSAL";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("KOMTEK_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = " POLLING_STATUS = 1 AND POLLING_TYPE = 7";

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
                search_clause += " OR PROPOSAL_JUDUL_PNPS = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_POLLING WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_POLLING " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_POLLING>(inject_clause_select);
            var userid = Convert.ToInt32(Session["USER_ID"]);
            var result = from EBA in SelectedData
                         select new string[] 
            { 
                Convert.ToString("Nomor : <span style='color:green'>" +EBA.PROPOSAL_NO_SNI_PROPOSAL +"</span><br />"+"Judul : "+EBA.PROPOSAL_JUDUL_PNPS+" <br />Komite Teknis : <b>"+EBA.KOMTEK_CODE+" - "+EBA.KOMTEK_NAME+" </b><br> ICS : <b>"+EBA.PROPOSAL_ICS_NAME+"</b>"),
                Convert.ToString(EBA.POLLING_FULL_DATE_NAME.Replace("<br>", " ") +"<br> ["+ EBA.POLLING_MONITORING_NAME +"]"),                
                Convert.ToString(EBA.PROPOSAL_RUANG_LINGKUP),          
                Convert.ToString((EBA.POLLING_MONITORING_TYPE == "Sedang Berlangsung")?"<a href='JajakPendapat/"+EBA.POLLING_ID+"' ><i class='fa fa-comments'></i> "+EBA.POLLING_JML_PARTISIPAN+"</a>":"<i class='fa fa-comments'></i> "+EBA.POLLING_JML_PARTISIPAN)

            };


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListLHAS(DataTables param)
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult LaporanJejakPendapat(int id = 0)
        {
            
            if (Session["USER_ID"] != null)
            {           
                ViewData["moduleId"] = moduleId;
                var DataProposal = (from poll in db.VIEW_POLLING where poll.POLLING_ID == id select poll).SingleOrDefault();
                ViewData["DataProposal"] = DataProposal;
                var UserId = Convert.ToInt32(Session["USER_ID"]);
                ViewData["poll_us"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = "+id+" AND POLLING_DETAIL_CREATE_BY =" + UserId).SingleOrDefault();
                ViewData["JML_POLLING"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + id).SingleOrDefault();
                //return Json(new { query = "SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = "+id+" AND POLLING_DETAIL_CREATE_BY =" + UserId }, JsonRequestBehavior.AllowGet);
                ViewData["polling"] = (from poll in db.VIEW_POLLING where poll.POLLING_ID == id select poll).SingleOrDefault();
                //ViewData["jp_list"] = (from poll in db.VIEW_POLLING_DETAIL where poll.POLLING_DETAIL_POLLING_ID == id orderby poll.POLLING_DETAIL_PASAL ascending select poll).ToList();
                ViewData["jp_list"] = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = '" + id + "' ORDER BY POLLING_DETAIL_PASAL ASC, POLLING_DETAIL_CREATE_BY ASC, POLLING_DETAIL_OPTION ASC").ToList();
                return View();
            }
            return RedirectToAction("../auth/index");
        }
        public ActionResult JejakPendapat(int id =0)
        {
            
            if (Session["USER_ID"] != null)
            {           
                ViewData["moduleId"] = moduleId;
                var DataProposal = (from poll in db.VIEW_POLLING where poll.POLLING_ID == id select poll).SingleOrDefault();
                ViewData["DataProposal"] = DataProposal;
                var UserId = Convert.ToInt32(Session["USER_ID"]);
                ViewData["poll_us"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = "+id+" AND POLLING_DETAIL_CREATE_BY =" + UserId).SingleOrDefault();
                ViewData["JML_POLLING"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + id).SingleOrDefault();
                //return Json(new { query = "SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = "+id+" AND POLLING_DETAIL_CREATE_BY =" + UserId }, JsonRequestBehavior.AllowGet);
                ViewData["polling"] = (from poll in db.VIEW_POLLING where poll.POLLING_ID == id select poll).SingleOrDefault();
                //ViewData["jp_list"] = (from poll in db.VIEW_POLLING_DETAIL where poll.POLLING_DETAIL_POLLING_ID == id orderby poll.POLLING_DETAIL_PASAL ascending select poll).ToList();
                ViewData["jp_list"] = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = '" + id + "' AND POLLING_DETAIL_CREATE_BY = " + UserId + " ORDER BY POLLING_DETAIL_PASAL ASC, POLLING_DETAIL_CREATE_BY ASC, POLLING_DETAIL_OPTION ASC").ToList();
                ViewData["Error"] = "";
                var isError = @TempData["isError"];

                if (isError != null)
                {
                    ViewData["Error"] = @TempData["MessageError"];
                }
                return View();
            }
            return RedirectToAction("../auth/index");
        }

        public ActionResult JajakPendapat(int id = 0)
        {

            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                var DataProposal = (from poll in db.VIEW_POLLING where poll.POLLING_ID == id select poll).SingleOrDefault();
                ViewData["DataProposal"] = DataProposal;
                var UserId = Convert.ToInt32(Session["USER_ID"]);
                ViewData["poll_us"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + id + " AND POLLING_DETAIL_CREATE_BY =" + UserId).SingleOrDefault();
                ViewData["JML_POLLING"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + id).SingleOrDefault();
                //return Json(new { query = "SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = "+id+" AND POLLING_DETAIL_CREATE_BY =" + UserId }, JsonRequestBehavior.AllowGet);
                ViewData["polling"] = (from poll in db.VIEW_POLLING where poll.POLLING_ID == id select poll).SingleOrDefault();
                //ViewData["jp_list"] = (from poll in db.VIEW_POLLING_DETAIL where poll.POLLING_DETAIL_POLLING_ID == id orderby poll.POLLING_DETAIL_PASAL ascending select poll).ToList();
                ViewData["jp_list"] = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = '" + id + "' AND POLLING_DETAIL_CREATE_BY = " + UserId + " ORDER BY POLLING_DETAIL_PASAL ASC, POLLING_DETAIL_CREATE_BY ASC, POLLING_DETAIL_OPTION ASC").ToList();
                ViewData["Error"] = "";
                var isError = @TempData["isError"];

                if (isError != null)
                {
                    ViewData["Error"] = @TempData["MessageError"];
                }
                return View();
            }
            return RedirectToAction("../auth/index");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult JajakPendapat(TRX_POLLING_DETAILS input, string jawaban = "")
        {
           if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
                {
                    var MsgError = "Jawaban Captcha Salah / Kosong";
                    TempData["isError"] = 1;
                    TempData["MessageError"] = MsgError;
                    return RedirectToAction("JajakPendapat/" + input.POLLING_DETAIL_POLLING_ID);
                }
                else
                {
                    var GetIP = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 12").FirstOrDefault();
                    var GetUser = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 13").FirstOrDefault();
                    var GetPassword = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 14").FirstOrDefault();
                    using (OracleConnection con = new OracleConnection("Data Source=" + GetIP.CONFIG_VALUE + ";User ID=" + GetUser.CONFIG_VALUE + ";PASSWORD=" + GetPassword.CONFIG_VALUE + ";"))
                    {
                        con.Open();

                        using (OracleCommand cmd = new OracleCommand())
                        {

                            var UserId = Session["USER_ID"];
                            var logcode = MixHelper.GetLogCode();
                            int lastid = MixHelper.GetSequence("TRX_POLLING_DETAILS");
                            var datenow = MixHelper.ConvertDateNow();

                            var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_OPTION,POLLING_DETAIL_REASON,POLLING_DETAIL_PASAL,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS,POLLING_DETAIL_INPUT_TYPE";

                            var fvalue = "'" + lastid + "', " +
                                        "'" + input.POLLING_DETAIL_POLLING_ID + "', " +
                                        "'" + input.POLLING_DETAIL_OPTION + "', " +
                                        ":parameter , " +
                                        "'" + input.POLLING_DETAIL_PASAL + "', " +
                                        "'" + UserId + "', " +
                                         datenow + ", " +
                                        "1," +
                                        "2";
                            cmd.Connection = con;
                            cmd.CommandType = System.Data.CommandType.Text;

                            //return Json(new { query = "INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                            cmd.CommandText = "INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES ('" + lastid + "','" + input.POLLING_DETAIL_POLLING_ID + "','" + input.POLLING_DETAIL_OPTION + "',:parameter,'" + input.POLLING_DETAIL_PASAL + "','" + UserId + "'," +
                                         datenow + ",1,2)";

                            OracleParameter oracleParameterClob = new OracleParameter();
                            oracleParameterClob.OracleDbType = OracleDbType.Clob;
                            //1 million long string
                            oracleParameterClob.Value = input.POLLING_DETAIL_REASON;


                            cmd.Parameters.Add(oracleParameterClob);

                            cmd.ExecuteNonQuery();

                            var polling = (from poll in db.TRX_POLLING where poll.POLLING_ID == input.POLLING_DETAIL_POLLING_ID select poll).SingleOrDefault();

                            //var setuju = polling.POLLING_SETUJU;
                            //var setuju_new = setuju + 1;

                            //var tdk_setuju = polling.POLLING_TDK_SETUJU;
                            //var tdk_setuju_new = tdk_setuju + 1;

                            var jml_partisipan = polling.POLLING_JML_PARTISIPAN + 1;

                            //var persen_setuju_new = setuju_new / jml_partisipan * 100;
                            //var persen_tdk_setuju = tdk_setuju_new / jml_partisipan * 100;            

                            var update = "";
                            update = "POLLING_JML_PARTISIPAN = " + jml_partisipan + "'";
                            var where_clause = " WHERE  POLLING_ID =" + input.POLLING_DETAIL_POLLING_ID;
                            db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_JML_PARTISIPAN = " + jml_partisipan + " WHERE POLLING_ID = " + input.POLLING_DETAIL_POLLING_ID);
                            //String objek = fvalue.Replace("'", "-");
                            //MixHelper.InsertLog(logcode, objek, 1);
                            TempData["Notifikasi"] = 1;
                            TempData["NotifikasiText"] = "Terima kasih, pendapat anda berhasil di simpan.";
                        }

                        con.Close();
                        return RedirectToAction("JajakPendapat/" + input.POLLING_DETAIL_POLLING_ID);
                    }
                }
            
        }

        public ActionResult EditJejakPendapat(int id = 0)
        {

            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                var datapoling = (from poll in db.VIEW_POLLING_DETAIL where poll.POLLING_DETAIL_ID == id select poll).SingleOrDefault();
                if(datapoling == null){

                }
                ViewData["DataProposal"] = (from poll in db.VIEW_POLLING where poll.POLLING_ID == datapoling.POLLING_DETAIL_POLLING_ID select poll).SingleOrDefault();
                ViewData["JML_POLLING"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + datapoling.POLLING_DETAIL_POLLING_ID).SingleOrDefault();
                ViewData["polling"] = datapoling;
                ViewData["jp_list"] = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = '" + datapoling.POLLING_DETAIL_POLLING_ID + "' ORDER BY POLLING_DETAIL_PASAL ASC, POLLING_DETAIL_CREATE_BY ASC, POLLING_DETAIL_OPTION ASC").ToList();
                ViewData["Error"] = "";
                var isError = @TempData["isError"];

                if (isError != null)
                {
                    ViewData["Error"] = @TempData["MessageError"];
                }
                return View();
            }
            return RedirectToAction("../auth/index");
        }

        public ActionResult EditJajakPendapat(int id = 0)
        {

            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                var datapoling = (from poll in db.VIEW_POLLING_DETAIL where poll.POLLING_DETAIL_ID == id select poll).SingleOrDefault();
                if (datapoling == null)
                {

                }
                var UserId = Convert.ToInt32(Session["USER_ID"]);
                ViewData["DataProposal"] = (from poll in db.VIEW_POLLING where poll.POLLING_ID == datapoling.POLLING_DETAIL_POLLING_ID select poll).SingleOrDefault();
                ViewData["JML_POLLING"] = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + datapoling.POLLING_DETAIL_POLLING_ID).SingleOrDefault();
                ViewData["polling"] = datapoling;
                ViewData["jp_list"] = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = '" + id + "' AND POLLING_DETAIL_CREATE_BY = " + UserId + " ORDER BY POLLING_DETAIL_PASAL ASC, POLLING_DETAIL_CREATE_BY ASC, POLLING_DETAIL_OPTION ASC").ToList();
                ViewData["Error"] = "";
                var isError = @TempData["isError"];

                if (isError != null)
                {
                    ViewData["Error"] = @TempData["MessageError"];
                }
                return View();
            }
            return RedirectToAction("../auth/index");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditJajakPendapat(TRX_POLLING_DETAILS input, string jawaban = "")
        {
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban Captcha Salah / Kosong";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
                return RedirectToAction("JajakPendapat/" + input.POLLING_DETAIL_POLLING_ID);
            }
            else
            {
                var GetIP = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 12").FirstOrDefault();
                var GetUser = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 13").FirstOrDefault();
                var GetPassword = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 14").FirstOrDefault();
                using (OracleConnection con = new OracleConnection("Data Source=" + GetIP.CONFIG_VALUE + ";User ID=" + GetUser.CONFIG_VALUE + ";PASSWORD=" + GetPassword.CONFIG_VALUE + ";"))
                {
                    con.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        var UserId = Session["USER_ID"];
                        var logcode = MixHelper.GetLogCode();
                        var datenow = MixHelper.ConvertDateNow();
                        var updatequery = "UPDATE TRX_POLLING_DETAILS SET " +
                                    "POLLING_DETAIL_REASON = :parameter, " +
                                    "POLLING_DETAIL_UPDATE_BY = '" + UserId + "', " +
                                    "POLLING_DETAIL_UPDATE_DATE = " + datenow +
                                    " WHERE POLLING_DETAIL_ID = '" + input.POLLING_DETAIL_ID + "'";
                        //return Json(new{sas =updatequery },JsonRequestBehavior.AllowGet);
                        //db.Database.ExecuteSqlCommand(updatequery);
                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.CommandText = updatequery;

                        OracleParameter oracleParameterClob = new OracleParameter();
                        oracleParameterClob.OracleDbType = OracleDbType.Clob;
                        //1 million long string
                        oracleParameterClob.Value = input.POLLING_DETAIL_REASON;


                        cmd.Parameters.Add(oracleParameterClob);

                        cmd.ExecuteNonQuery();

                        TempData["Notifikasi"] = 1;
                        TempData["NotifikasiText"] = "Terima kasih, pendapat anda berhasil di simpan.";
                    }

                    con.Close();
                    return RedirectToAction("JajakPendapat/" + input.POLLING_DETAIL_POLLING_ID);
                }
            }
        }


        public ActionResult cekdata(TRX_POLLING_DETAILS form)
        {
            int status = 0;
            var UserId = Session["USER_ID"];
            status = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + form.POLLING_DETAIL_POLLING_ID + " AND POLLING_DETAIL_PASAL = " + form.POLLING_DETAIL_PASAL + " AND POLLING_DETAIL_OPTION = " + form.POLLING_DETAIL_OPTION + " AND POLLING_DETAIL_CREATE_BY =" + UserId).SingleOrDefault();
            return Json(new { status = status, query = "SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + form.POLLING_DETAIL_POLLING_ID + " AND POLLING_DETAIL_PASAL = " + form.POLLING_DETAIL_PASAL + " AND POLLING_DETAIL_OPTION = " + form.POLLING_DETAIL_OPTION + " AND POLLING_DETAIL_CREATE_BY =" + UserId }, JsonRequestBehavior.AllowGet);
        }       

    }
}
