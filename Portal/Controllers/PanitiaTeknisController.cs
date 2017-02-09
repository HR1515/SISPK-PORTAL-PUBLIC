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
    public class PanitiaTeknisController : Controller
    {
        //
        // GET: /PanitiTeknis/
        private int moduleId = 2;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult UsulanPanTek()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListUsulanPantek(DataTables param)
        {
            var default_order = "KOMTEK_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PARENT_NAME");
            order_field.Add("KOMTEK_SEKRETARIAT");
            order_field.Add("KOMTEK_ADDRESS");
            order_field.Add("KOMTEK_PHONE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "KOMTEK_STATUS = 0 AND KOMTEK_PARENT_CODE = '0'";

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
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_KOMTEK_TEMP WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_KOMTEK_TEMP " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_KOMTEK_TEMP>(inject_clause_select);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.KOMTEK_CODE), 
                Convert.ToString("<a href='/PanitiaTeknis/DetilUsulanPantek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_NAME+"</a>"), 
                Convert.ToString(list.KOMTEK_SEKRETARIAT),
                Convert.ToString(list.KOMTEK_ADDRESS),                
                Convert.ToString(list.KOMTEK_PHONE)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TambahUsulanPanTek()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["listICS"] = (from t in db.VIEW_ICS where t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult TambahUsulanPanTek(FormCollection m_komtek)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("T_MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();
            
            var parent = "0";
            string path = Server.MapPath("~/Upload/UsulanKomtek/");
            HttpPostedFileBase file_att = Request.Files["lampirananggota"];
            var file_name_att = "";
            if (file_att != null)
            {
                string lampirananggotapath = file_att.FileName;
                if (lampirananggotapath.Trim() != "")
                {
                    lampirananggotapath = Path.GetFileNameWithoutExtension(file_att.FileName);
                    string fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "LAMPIRAN_ANGGOTA_" + lastid + fileExtension;
                    string filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }
            }
            var fname = "T_KOMTEK_ID,T_KOMTEK_PARENT_CODE,T_KOMTEK_NAME,T_KOMTEK_SEKRETARIAT,T_KOMTEK_ADDRESS,T_KOMTEK_PHONE,T_KOMTEK_FAX,T_KOMTEK_EMAIL,T_KOMTEK_DESCRIPTION,T_KOMTEK_CREATE_BY,T_KOMTEK_CREATE_DATE,T_KOMTEK_LOG_CODE,T_KOMTEK_ATTACHMENT,T_KOMTEK_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + parent + "', " +
                        "'" + m_komtek["T_KOMTEK_NAME"] + "'," +
                        "'" + m_komtek["T_KOMTEK_SEKRETARIAT"] + "'," +
                        "'" + m_komtek["T_KOMTEK_ADDRESS"] + "'," +
                        "'" + m_komtek["T_KOMTEK_PHONE"] + "'," +
                        "'" + m_komtek["T_KOMTEK_FAX"] + "'," +
                        "'" + m_komtek["T_KOMTEK_EMAIL"] + "'," +
                        "'" + m_komtek["T_KOMTEK_DESCRIPTION"] + "'," +
                        "'" + UserId + "'," +
                         datenow + "," +
                         "'" + logcode + "'," +
                         "'" + file_name_att + "'," +
                        "0";
            db.Database.ExecuteSqlCommand("INSERT INTO T_MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            //MixHelper.InsertLog(logcode, objek, 1);
            var komtek_ics_id = m_komtek["ics"];
            if (komtek_ics_id != null)
            {   
                string[] vals = komtek_ics_id.Split(',');
                for (int n = 0; n < vals.Length; n++)
                {
                    int lastid_mki = MixHelper.GetSequence("T_MASTER_KOMTEK_ICS");
                    string query_update = "INSERT INTO T_MASTER_KOMTEK_ICS (T_KOMTEK_ICS_ID,T_KOMTEK_ICS_KOMTEK_ID,T_KOMTEK_ICS_ICS_ID,T_KOMTEK_ICS_CREATE_BY,T_KOMTEK_ICS_CREATE_DATE,T_KOMTEK_ICS_STATUS,T_KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + vals[n] + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                    db.Database.ExecuteSqlCommand(query_update);
                   
                }

            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data berhasil ditambahkan, usulan anda akan di kaji oleh admin. terima kasih atas masukan nya.";
            return RedirectToAction("UsulanPanTek");
        }


        public ActionResult DetilUsulanPantek(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            VIEW_KOMTEK_TEMP data = db.VIEW_KOMTEK_TEMP.Find(id);
            if(data == null){
                RedirectToAction("UsulanPantek");
            }
            ViewData["DataPantek"] = data;
            return View(data);
        }

        public ActionResult UsulanSubPanTek()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListUsulanSubPantek(DataTables param)
        {
            var default_order = "KOMTEK_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");
            order_field.Add("PARENT_NAME");
            order_field.Add("KOMTEK_SEKRETARIAT");
            order_field.Add("KOMTEK_ADDRESS");
            order_field.Add("KOMTEK_PHONE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "KOMTEK_STATUS = 0 AND KOMTEK_PARENT_CODE != '0'";

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
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_KOMTEK_TEMP WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_KOMTEK_TEMP " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_KOMTEK_TEMP>(inject_clause_select);
            
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.KOMTEK_CODE), 
                Convert.ToString("<a href='/PanitiaTeknis/DetilUsulanSubPantek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_NAME+"</a>"),                 
                Convert.ToString(list.PARENT_CODE + ". " + list.PARENT_NAME),
                Convert.ToString(list.KOMTEK_SEKRETARIAT),
                Convert.ToString(list.KOMTEK_ADDRESS),                
                Convert.ToString(list.KOMTEK_PHONE)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TambahUsulanSubPanTek()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["listICS"] = (from t in db.VIEW_ICS where t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            ViewData["listKomtek"] = (from t in db.VIEW_KOMTEK where t.KOMTEK_STATUS == 1 && t.KOMTEK_PARENT_CODE == "0" orderby t.KOMTEK_CODE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult TambahUsulanSubPanTek(FormCollection m_komtek)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("T_MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();

            var parent = m_komtek["T_KOMTEK_PARENT_CODE"];
            string path = Server.MapPath("~/Upload/UsulanKomtek/");
            HttpPostedFileBase file_att = Request.Files["lampirananggota"];
            var file_name_att = "";
            if (file_att != null)
            {
                string lampirananggotapath = file_att.FileName;
                if (lampirananggotapath.Trim() != "")
                {
                    lampirananggotapath = Path.GetFileNameWithoutExtension(file_att.FileName);
                    string fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "LAMPIRAN_ANGGOTA_" + lastid + fileExtension;
                    string filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }
            }
            var fname = "T_KOMTEK_ID,T_KOMTEK_PARENT_CODE,T_KOMTEK_NAME,T_KOMTEK_SEKRETARIAT,T_KOMTEK_ADDRESS,T_KOMTEK_PHONE,T_KOMTEK_FAX,T_KOMTEK_EMAIL,T_KOMTEK_DESCRIPTION,T_KOMTEK_CREATE_BY,T_KOMTEK_CREATE_DATE,T_KOMTEK_LOG_CODE,T_KOMTEK_ATTACHMENT,T_KOMTEK_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + parent + "', " +
                        "'" + m_komtek["T_KOMTEK_NAME"] + "'," +
                        "'" + m_komtek["T_KOMTEK_SEKRETARIAT"] + "'," +
                        "'" + m_komtek["T_KOMTEK_ADDRESS"] + "'," +
                        "'" + m_komtek["T_KOMTEK_PHONE"] + "'," +
                        "'" + m_komtek["T_KOMTEK_FAX"] + "'," +
                        "'" + m_komtek["T_KOMTEK_EMAIL"] + "'," +
                        "'" + m_komtek["T_KOMTEK_DESCRIPTION"] + "'," +
                        "'" + UserId + "'," +
                         datenow + "," +
                         "'" + logcode + "'," +
                         "'" + file_name_att + "'," +
                        "0";
            db.Database.ExecuteSqlCommand("INSERT INTO T_MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            //MixHelper.InsertLog(logcode, objek, 1);
            var komtek_ics_id = m_komtek["ics"];
            if (komtek_ics_id != null)
            {
                string[] vals = komtek_ics_id.Split(',');
                for (int n = 0; n < vals.Length; n++)
                {
                    int lastid_mki = MixHelper.GetSequence("T_MASTER_KOMTEK_ICS");
                    string query_update = "INSERT INTO T_MASTER_KOMTEK_ICS (T_KOMTEK_ICS_ID,T_KOMTEK_ICS_KOMTEK_ID,T_KOMTEK_ICS_ICS_ID,T_KOMTEK_ICS_CREATE_BY,T_KOMTEK_ICS_CREATE_DATE,T_KOMTEK_ICS_STATUS,T_KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + vals[n] + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                    db.Database.ExecuteSqlCommand(query_update);

                }

            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data berhasil ditambahkan, usulan anda akan di kaji oleh admin. terima kasih atas masukan nya.";
            return RedirectToAction("UsulanSubPanTek");
        }

        public ActionResult DetilUsulanSubPantek(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            VIEW_KOMTEK_TEMP data = db.VIEW_KOMTEK_TEMP.Find(id);
            if (data == null)
            {
                RedirectToAction("UsulanPantek");
            }
            ViewData["DataPantek"] = data;
            return View(data);
        }

        public ActionResult PanTek()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListPantek(DataTables param)
        {
            var default_order = "KOMTEK_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");
            order_field.Add("KOMTEK_SEKRETARIAT");
            order_field.Add("KOMTEK_ADDRESS");
            order_field.Add("KOMTEK_PHONE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "KOMTEK_STATUS = 1 AND KOMTEK_PARENT_CODE = '0'";

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
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_KOMTEK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_KOMTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_KOMTEK>(inject_clause_select);
            
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center><a href='/PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a></center>"), 
                Convert.ToString(list.KOMTEK_NAME), 
                Convert.ToString(list.KOMTEK_SEKRETARIAT),
                Convert.ToString(list.KOMTEK_ADDRESS),                
                Convert.ToString(list.KOMTEK_PHONE)
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetilPantek(int id = 0) {
            ViewData["moduleId"] = moduleId;
            var DataPantek = (from pantek in db.MASTER_KOMITE_TEKNIS where pantek.KOMTEK_ID == id select pantek).SingleOrDefault();
            ViewData["DataPantek"] = DataPantek;
            var DataIcs = (from ics in db.VIEW_KOMTEK_ICS where ics.KOMTEK_ICS_KOMTEK_ID == id select ics).ToList();
            ViewData["DataIcs"] = DataIcs;
            var DataAnggota = (from ics in db.VIEW_ANGGOTA where ics.KOMTEK_ANGGOTA_KOMTEK_ID == id select ics).ToList();
            ViewData["DataAnggota"] = DataAnggota;
            var komposis_anggota = db.Database.SqlQuery<KomposisiKomtek>("SELECT CAST(VA.KOMTEK_ANGGOTA_STAKEHOLDER AS DECIMAL) AS KOMTEK_ANGGOTA_STAKEHOLDER,VA.STAKEHOLDER,CAST(VA.KOMTEK_ANGGOTA_KOMTEK_ID AS DECIMAL) AS KOMTEK_ANGGOTA_KOMTEK_ID, CAST(COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER) AS DECIMAL) AS JML, CAST(ROUND((COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER)/(SELECT COUNT(VAS.KOMTEK_ANGGOTA_STAKEHOLDER) FROM VIEW_ANGGOTA VAS WHERE VAS.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + ")* 100),2) AS DECIMAL) AS PERSENTASE FROM VIEW_ANGGOTA VA WHERE VA.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " GROUP BY VA.KOMTEK_ANGGOTA_STAKEHOLDER, VA.STAKEHOLDER, VA.KOMTEK_ANGGOTA_KOMTEK_ID").ToList();
            ViewData["Komp_Ang"] = komposis_anggota;
            return View();
        }

        public ActionResult Detailkomtek(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var DataPantek = (from pantek in db.MASTER_KOMITE_TEKNIS where pantek.KOMTEK_ID == id select pantek).SingleOrDefault();
            ViewData["DataPantek"] = DataPantek;
            var DataIcs = (from ics in db.VIEW_KOMTEK_ICS where ics.KOMTEK_ICS_KOMTEK_ID == id select ics).ToList();
            ViewData["DataIcs"] = DataIcs;
            var DataAnggota = (from ics in db.VIEW_ANGGOTA where ics.KOMTEK_ANGGOTA_KOMTEK_ID == id && ics.KOMTEK_ANGGOTA_STATUS == 1 && ics.JABATAN != "Sekretariat" orderby ics.KOMTEK_ANGGOTA_JABATAN ascending select ics).ToList();
            ViewData["DataAnggota"] = DataAnggota;
            var komposis_anggota = db.Database.SqlQuery<KomposisiKomtek>("SELECT CAST(VA.KOMTEK_ANGGOTA_STAKEHOLDER AS DECIMAL) AS KOMTEK_ANGGOTA_STAKEHOLDER,VA.STAKEHOLDER,CAST(VA.KOMTEK_ANGGOTA_KOMTEK_ID AS DECIMAL) AS KOMTEK_ANGGOTA_KOMTEK_ID, CAST(COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER) AS DECIMAL) AS JML, CAST(ROUND((COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER)/(SELECT COUNT(VAS.KOMTEK_ANGGOTA_STAKEHOLDER) FROM VIEW_ANGGOTA VAS WHERE VAS.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " AND VAS.JABATAN != 'Sekretariat')* 100),2) AS DECIMAL) AS PERSENTASE FROM VIEW_ANGGOTA VA WHERE VA.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " AND VA.JABATAN != 'Sekretariat' AND VA.KOMTEK_ANGGOTA_STAKEHOLDER IS NOT NULL GROUP BY VA.KOMTEK_ANGGOTA_STAKEHOLDER, VA.STAKEHOLDER, VA.KOMTEK_ANGGOTA_KOMTEK_ID").ToList();
            ViewData["Komp_Ang"] = komposis_anggota;
            var title = (DataPantek.KOMTEK_PARENT_CODE != "0") ? "Sub Komite Teknis" : "Komite Teknis";
            ViewData["title"] = title;
            return View();
        }
                
        public ActionResult SubPanTek()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult DetailSubPanTek(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var DataPantek = (from pantek in db.MASTER_KOMITE_TEKNIS where pantek.KOMTEK_ID == id select pantek).SingleOrDefault();
            ViewData["DataPantek"] = DataPantek;
            var DataIcs = (from ics in db.VIEW_KOMTEK_ICS where ics.KOMTEK_ICS_KOMTEK_ID == id select ics).ToList();
            ViewData["DataIcs"] = DataIcs;
            var DataAnggota = (from ics in db.VIEW_ANGGOTA where ics.KOMTEK_ANGGOTA_KOMTEK_ID == id select ics).ToList();
            ViewData["DataAnggota"] = DataAnggota;
            var komposis_anggota = db.Database.SqlQuery<KomposisiKomtek>("SELECT CAST(VA.KOMTEK_ANGGOTA_STAKEHOLDER AS DECIMAL) AS KOMTEK_ANGGOTA_STAKEHOLDER,VA.STAKEHOLDER,CAST(VA.KOMTEK_ANGGOTA_KOMTEK_ID AS DECIMAL) AS KOMTEK_ANGGOTA_KOMTEK_ID, CAST(COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER) AS DECIMAL) AS JML, CAST(ROUND((COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER)/(SELECT COUNT(VAS.KOMTEK_ANGGOTA_STAKEHOLDER) FROM VIEW_ANGGOTA VAS WHERE VAS.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + ")* 100),2) AS DECIMAL) AS PERSENTASE FROM VIEW_ANGGOTA VA WHERE VA.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " GROUP BY VA.KOMTEK_ANGGOTA_STAKEHOLDER, VA.STAKEHOLDER, VA.KOMTEK_ANGGOTA_KOMTEK_ID").ToList();
            ViewData["Komp_Ang"] = komposis_anggota;
            return View();
        }

        public ActionResult ListSubPantek(DataTables param)
        {
            var default_order = "KOMTEK_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SUB_KOMTEK_CODE");
            order_field.Add("SUB_KOMTEK");
            order_field.Add("KOMTEK_SEKRETARIAT");
            order_field.Add("KOMTEK_ADDRESS");
            order_field.Add("KOMTEK_PHONE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "KOMTEK_STATUS = 1 AND KOMTEK_PARENT_CODE != '0'";

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
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SUBKOMTEK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SUBKOMTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SUBKOMTEK>(inject_clause_select);
            
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center><a href='/PanitiaTeknis/Detailkomtek/"+list.KOMTEK_ID+"'>"+list.SUB_KOMTEK_CODE+"</a></center>"), 
                Convert.ToString(list.SUB_KOMTEK), 
                Convert.ToString(list.KOMTEK_SEKRETARIAT),
                Convert.ToString(list.KOMTEK_ADDRESS),                
                Convert.ToString(list.KOMTEK_PHONE)
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
