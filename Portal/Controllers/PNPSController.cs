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
using Aspose.Pdf;
using Aspose.Words;
using Aspose.Words.Tables;
using Aspose.Words.Drawing;

namespace Portal.Controllers
{
    public class PNPSController : Controller
    {
        private int moduleId = 9;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();

        public ActionResult UsulanPNPS()
        {
            ViewData["moduleId"] = moduleId;
            if (Session["USER_ID"] != null)
            {
                return RedirectToAction("TambahUsulanPNPS");
            }
            else {
                return RedirectToAction("../auth/index");
            }
        }
        public static String GetInfo(string wew = "")
        {
            var bulan = "";
            return bulan;
        }
        public ActionResult ListUsulanPNPS(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_RUANG_LINGKUP");
            order_field.Add("PROPOSAL_YEAR");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("PROPOSAL_STATUS_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS = 0";

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
            //return Json(new
            //{
            //    query = inject_clause_select,
            //    query2 = "SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count

            //}, JsonRequestBehavior.AllowGet);
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.PROPOSAL_CREATE_DATE),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'><a href='/PNPS/DetilUsulanPNPS/"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</a></span>"),
                Convert.ToString(list.PROPOSAL_RUANG_LINGKUP),
                Convert.ToString("<center>"+list.PROPOSAL_YEAR+"</center>"),          
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                //Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TambahUsulanPNPS()
        {
            if (Session["USER_ID"] != null)
            {
                var link = (from t in portaldb.SYS_LINK where t.LINK_IS_USE == 1 && t.LINK_ID == 1 select t).SingleOrDefault();
                ViewData["link"] = link;
                ViewData["moduleId"] = moduleId;
                ViewData["User"] = db.SYS_USER_PUBLIC.SqlQuery("SELECT * FROM SYS_USER_PUBLIC WHERE USER_PUBLIC_ID = '" + Session["USER_PUBLIC_ID"] + "'").SingleOrDefault();
                return View();
            }
            else
            {
                return RedirectToAction("../auth/index");
            }
           
        }

        public ActionResult Test()
        {
            string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/");
            return Json(new { pathnya = path }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TambahUsulanPNPS(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            var PROPOSAL_CODE = db.Database.SqlQuery<String>("SELECT TO_CHAR (SYSDATE, 'YYYYMMDD-') || ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '000' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 2 THEN '00' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255)) WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 3 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) PROPOSAL_CODE FROM TRX_PROPOSAL WHERE TO_CHAR (SYSDATE, 'MM-DD-YYYY') = TO_CHAR (PROPOSAL_CREATE_DATE,'MM-DD-YYYY')").SingleOrDefault();

            var DataPath1 = (from path in db.SYS_CONFIG where path.CONFIG_ID == 15 select path).SingleOrDefault();
            string pathnya = DataPath1.CONFIG_VALUE+"/Upload/Dokumen/HAK_PATEN/";

            //string pathnya = Server.MapPath("~/Upload/Dokumen/HAK_PATEN/");
            HttpPostedFileBase file_paten = Request.Files["PROPOSAL_HAK_PATEN_LOCATION"];
            var file_name_paten = "";
            var filePath_paten = "";
            var fileExtension_paten = "";
            if (file_paten != null)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(pathnya))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(pathnya);
                }
                string lampiranregulasipath = file_paten.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_paten.FileName);
                    fileExtension_paten = Path.GetExtension(file_paten.FileName);
                    file_name_paten = "HAK_PATEN_ID_PROPOSAL_" + LASTID + fileExtension_paten;
                    filePath_paten = pathnya + file_name_paten.Replace(" ", "_");
                    file_paten.SaveAs(filePath_paten);
                }
            }

            var fname = "";
            var fvalue = "";

            if (INPUT.PROPOSAL_HAK_PATEN_LOCATION != null)
            {
                fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_RETEK_ID,PROPOSAL_LPK_ID,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_TIM_NAMA,PROPOSAL_TIM_ALAMAT,PROPOSAL_TIM_PHONE,PROPOSAL_TIM_EMAIL,PROPOSAL_TIM_FAX,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_JENIS_ADOPSI,PROPOSAL_METODE_ADOPSI,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_RALAT_SNI_ID,PROPOSAL_AMD_SNI_ID,PROPOSAL_IS_URGENT,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE,PROPOSAL_MANFAAT_PENERAPAN,PROPOSAL_IS_ORG_MENDUKUNG,PROPOSAL_IS_DUPLIKASI_DESC,PROPOSAL_HAK_PATEN_LOCATION,PROPOSAL_HAK_PATEN_NAME,PROPOSAL_CODE";
                fvalue = "'" + LASTID + "', " +
                    "2, " +
                    "'" + INPUT.PROPOSAL_RETEK_ID + "', " +
                    "'" + INPUT.PROPOSAL_LPK_ID + "', " +
                    "'" + INPUT.PROPOSAL_YEAR + "', " +
                    "NULL, " +
                    "'" + INPUT.PROPOSAL_TIM_NAMA + "', " +
                    "'" + INPUT.PROPOSAL_TIM_ALAMAT + "', " +
                    "'" + INPUT.PROPOSAL_TIM_PHONE + "', " +
                    "'" + INPUT.PROPOSAL_TIM_EMAIL + "', " +
                    "'" + INPUT.PROPOSAL_TIM_FAX + "', " +
                    "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                    "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                    "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                    "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                    "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                    "'" + INPUT.PROPOSAL_JALUR + "', " +
                    "'" + INPUT.PROPOSAL_JENIS_ADOPSI + "', " +
                    "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                    "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                    "'" + INPUT.PROPOSAL_RALAT_SNI_ID + "', " +
                    "'" + INPUT.PROPOSAL_AMD_SNI_ID + "', " +
                    "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                    "'" + INPUT.PROPOSAL_PASAL + "', " +
                    "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                    "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                    "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                    "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                    "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                    "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                    "'" + USER_ID + "', " +
                    DATENOW + "," +
                    "'0', " +
                    "'1', " +
                    "'" + LOGCODE + "'," +
                    "'" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
                    "'" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
                    "'" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
                    "'/Upload/Dokumen/HAK_PATEN/'," +
                    "'" + file_name_paten.Replace(" ", "_") + "'," +
                    "'" + PROPOSAL_CODE + "'";
            }
            else
            {
                fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_RETEK_ID,PROPOSAL_LPK_ID,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_TIM_NAMA,PROPOSAL_TIM_ALAMAT,PROPOSAL_TIM_PHONE,PROPOSAL_TIM_EMAIL,PROPOSAL_TIM_FAX,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_JENIS_ADOPSI,PROPOSAL_METODE_ADOPSI,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_RALAT_SNI_ID,PROPOSAL_AMD_SNI_ID,PROPOSAL_IS_URGENT,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE,PROPOSAL_MANFAAT_PENERAPAN,PROPOSAL_IS_ORG_MENDUKUNG,PROPOSAL_IS_DUPLIKASI_DESC,PROPOSAL_CODE";
                fvalue = "'" + LASTID + "', " +
                    "2, " +
                    "'" + INPUT.PROPOSAL_RETEK_ID + "', " +
                    "'" + INPUT.PROPOSAL_LPK_ID + "', " +
                    "'" + INPUT.PROPOSAL_YEAR + "', " +
                    "NULL, " +
                    "'" + INPUT.PROPOSAL_TIM_NAMA + "', " +
                    "'" + INPUT.PROPOSAL_TIM_ALAMAT + "', " +
                    "'" + INPUT.PROPOSAL_TIM_PHONE + "', " +
                    "'" + INPUT.PROPOSAL_TIM_EMAIL + "', " +
                    "'" + INPUT.PROPOSAL_TIM_FAX + "', " +
                    "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                    "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                    "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                    "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                    "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                    "'" + INPUT.PROPOSAL_JALUR + "', " +
                    "'" + INPUT.PROPOSAL_JENIS_ADOPSI + "', " +
                    "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                    "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                    "'" + INPUT.PROPOSAL_RALAT_SNI_ID + "', " +
                    "'" + INPUT.PROPOSAL_AMD_SNI_ID + "', " +
                    "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                    "'" + INPUT.PROPOSAL_PASAL + "', " +
                    "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                    "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                    "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                    "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                    "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                    "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                    "'" + USER_ID + "', " +
                    DATENOW + "," +
                    "'0', " +
                    "'1', " +
                    "'" + LOGCODE + "'," +
                    "'" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
                    "'" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
                    "'" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
                    "'" + PROPOSAL_CODE + "'";
            }

                
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            var tester = "INSERT INTO TRX_PROPOSAL_FIXER (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")";
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + LASTID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + LASTID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_PROPOSAL_REF_NON_SNI = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_STATUS = 1 AND LOWER(ACUAN_NON_SNI_JUDUL) = '" + DATA_NON_SNI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_PROPOSAL_REF_NON_SNI != null)
                    {

                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + CEK_PROPOSAL_REF_NON_SNI.ACUAN_NON_SNI_ID + "','" + DATA_NON_SNI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + DATA_NON_SNI_VAL + "')");
                    }

                }
            }
            if (BIBLIOGRAFI != null)
            {
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_BIBLIOGRAFI = db.Database.SqlQuery<MASTER_BIBLIOGRAFI>("SELECT * FROM MASTER_BIBLIOGRAFI WHERE BIBLIOGRAFI_STATUS = 1 AND LOWER(BIBLIOGRAFI_JUDUL) = '" + BIBLIOGRAFI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_BIBLIOGRAFI != null)
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + CEK_BIBLIOGRAFI.BIBLIOGRAFI_ID + "','" + BIBLIOGRAFI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    }
                }
            }
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == LASTID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
            var PROPOSAL_ID = LASTID;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            var DataPath = (from path in db.SYS_CONFIG where path.CONFIG_ID == 15 select path).SingleOrDefault();
            var Pathnya = DataPath.CONFIG_VALUE;
            if (INPUT.PROPOSAL_IS_ORG_MENDUKUNG == 1)
            {
                HttpPostedFileBase file = Request.Files["PROPOSAL_DUKUNGAN_FILE_PATH"];
                if (file.ContentLength > 0)
                {
                    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                    Directory.CreateDirectory(Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER);
                    string path = Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/";
                    Stream stremdokumen = file.InputStream;
                    byte[] appData = new byte[file.ContentLength + 1];
                    stremdokumen.Read(appData, 0, file.ContentLength);
                    string Extension = Path.GetExtension(file.FileName);
                    if (Extension.ToLower() == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                        string filePathpdf = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                        string filePathxml = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                        pdf.Save(@"" + filePathxml);
                        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                    "'10', " +
                                    "'29', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Bukti Dukungan Usulan" + "', " +
                                    "'Bukti Dukungan Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                    "'0', " +
                                    "'" + USER_ID + "', " +
                                    DATENOW + "," +
                                    "'1', " +
                                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                    }
                }
            }
            HttpPostedFileBase file2 = Request.Files["PROPOSAL_LAMPIRAN_FILE_PATH"];
            if (file2.ContentLength > 0)
            {
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER);
                string path = Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/";
                Stream stremdokumen = file2.InputStream;
                byte[] appData = new byte[file2.ContentLength + 1];
                stremdokumen.Read(appData, 0, file2.ContentLength);
                string Extension = Path.GetExtension(file2.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'30', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Pendukung Usulan" + "', " +
                                "'Lampiran Pendukung Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase file3 = Request.Files["PROPOSAL_SURAT_PENGAJUAN_PNPS"];
            if (file3.ContentLength > 0)
            {
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER);
                string path = Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/";
                Stream stremdokumen = file3.InputStream;
                byte[] appData = new byte[file3.ContentLength + 1];
                stremdokumen.Read(appData, 0, file3.ContentLength);
                string Extension = Path.GetExtension(file3.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'32', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Surat Pengajuan PNPS" + "', " +
                                "'Lampiran Surat Pengajuan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase file4 = Request.Files["PROPOSAL_OUTLINE_RSNI"];
            if (file4.ContentLength > 0)
            {
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER);
                string path = Pathnya + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/";
                Stream stremdokumen = file4.InputStream;
                byte[] appData = new byte[file4.ContentLength + 1];
                stremdokumen.Read(appData, 0, file4.ContentLength);
                string Extension = Path.GetExtension(file4.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'36', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Outline RSNI" + "', " +
                                "'Lampiran Outline RSNI dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            //return Json(new { query = "INSERT INTO TRANSACTION_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("DetilUsulanPNPS/" + LASTID);
        }

        //[HttpPost]
        public ActionResult TambahUsulanPNPS_Backup_22_jul_2016(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, int[] PROPOSAL_REF_NON_SNI, int[] BIBLIOGRAFI)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();

            var fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_TIM_NAMA,PROPOSAL_TIM_ALAMAT,PROPOSAL_TIM_PHONE,PROPOSAL_TIM_EMAIL,PROPOSAL_TIM_FAX,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_JENIS_ADOPSI,PROPOSAL_METODE_ADOPSI,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_RALAT_SNI_ID,PROPOSAL_AMD_SNI_ID,PROPOSAL_IS_URGENT,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE";
            var fvalue = "'" + LASTID + "', " +
                        "'" + INPUT.PROPOSAL_TYPE + "', " +
                        "'" + INPUT.PROPOSAL_YEAR + "', " +
                        "NULL, " +
                        "'" + INPUT.PROPOSAL_TIM_NAMA + "', " +
                        "'" + INPUT.PROPOSAL_TIM_ALAMAT + "', " +
                        "'" + INPUT.PROPOSAL_TIM_PHONE + "', " +
                        "'" + INPUT.PROPOSAL_TIM_EMAIL + "', " +
                        "'" + INPUT.PROPOSAL_TIM_FAX + "', " +
                        "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                        "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                        "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                        "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                        "'" + INPUT.PROPOSAL_JALUR + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_RALAT_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_AMD_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                        "'" + INPUT.PROPOSAL_PASAL + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                        "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                        "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                        "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                        "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'0', " +
                        "'1', " +
                        "'" + LOGCODE + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            var tester = "INSERT INTO TRX_PROPOSAL_FIXER (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")";
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + LASTID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + LASTID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + DATA_NON_SNI_VAL + "')");
                }
            }
            if (BIBLIOGRAFI != null)
            {
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                }
            }

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            //return Json(new { query = "INSERT INTO TRANSACTION_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("UsulanPNPS");
        }

        public ActionResult DetilUsulanPNPS(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var link = (from links in portaldb.SYS_LINK where links.LINK_IS_USE == 1 && links.LINK_STATUS == 1 select links).SingleOrDefault();
            ViewData["link"] = link.LINK_NAME;
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            ViewData["Outline"] = Outline;
            return View();
        }

        public ActionResult InformasiPNPS()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult ListInformasiPNPS(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_RUANG_LINGKUP");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_YEAR");
            order_field.Add("PROPOSAL_STATUS_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS = 3";

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
                Convert.ToString(list.PROPOSAL_CREATE_DATE),           
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'><a href='/PNPS/DetilPNPS/"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_NO_SNI_PROPOSAL + " " + list.PROPOSAL_JUDUL_PNPS+"</a></span>"),
                Convert.ToString(list.PROPOSAL_RUANG_LINGKUP),
                Convert.ToString("<a href='/PanitiaTeknis/DetilPantek/"+list.KOMTEK_ID+"'>"+list.KOMTEK_CODE+"</a>"),
                Convert.ToString("<center>"+list.PROPOSAL_YEAR+"</center>"),     
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                //Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DetilPNPS_backup(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            return View();
        }

        public ActionResult DetilPNPS(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var link = (from links in portaldb.SYS_LINK where links.LINK_IS_USE == 1 && links.LINK_STATUS == 1 select links).SingleOrDefault();
            var link1 = (from t in portaldb.SYS_LINK where t.LINK_IS_USE == 1 && t.LINK_ID == 1 select t).SingleOrDefault();
            ViewData["link1"] = link1;
            ViewData["link"] = link.LINK_NAME;
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            return View();
        }

        public ActionResult MTPS()
        {
            ViewData["moduleId"] = moduleId;
            return View();
        }

        public ActionResult list_MTPS(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "POLLING_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("POLLING_FULL_DATE_NAME");
            order_field.Add("POLLING_MONITORING_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "POLLING_TYPE = 2 AND POLLING_IS_KUORUM = 0 AND POLLING_MONITORING_TYPE != 'Sudah Lewat'";

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_POLLING WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_POLLING " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_POLLING>(inject_clause_select);

            int id = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(id++),
                Convert.ToString("<a href='/PNPS/Detail_Usulan_PNPS/"+list.PROPOSAL_ID+"' class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</a>"),
                Convert.ToString("<center>"+list.POLLING_FULL_DATE_NAME+"</center>"),
                Convert.ToString(list.POLLING_MONITORING_NAME),
                Convert.ToString("<a href='/PNPS/CommentMTPS/"+list.POLLING_ID+"'><i class='fa fa-comments'></i> "+list.POLLING_JML_PARTISIPAN+"</a>")
                //Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
            };
            return Json(new
            {
                //a=SelectedData
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CommentMTPS(int id = 0)
        {
            if (Session["USER_ID"] != null)
            {
                int user_id = Convert.ToInt32(Session["USER_ID"]);
                ViewData["moduleId"] = moduleId;
                ViewData["POLLING_ID"] = id;
                ViewData["data_mtps"] = (from t in db.VIEW_POLLING where t.POLLING_ID == id select t).SingleOrDefault();
                var GetPath = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 15").FirstOrDefault();
                ViewData["pathnya"] = GetPath.CONFIG_VALUE;
                //ViewData["list_poll_detail"] = (from t in db.VIEW_POLLING_DETAIL where t.POLLING_DETAIL_POLLING_ID == id && t.POLLING_DETAIL_CREATE_BY == user_id && t.POLLING_TYPE == "2" orderby t.POLLING_DETAIL_CREATE_DATE descending select t).ToList();
                ViewData["list_poll_detail"] = (from t in db.VIEW_POLLING_DETAIL where t.POLLING_DETAIL_POLLING_ID == id orderby t.POLLING_DETAIL_CREATE_DATE descending select t).ToList();
                var link = (from t in portaldb.SYS_LINK where t.LINK_IS_USE == 1 && t.LINK_ID == 1 select t).SingleOrDefault();
                ViewData["link"] = link;
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
        public ActionResult CommentMTPS(TRX_POLLING_DETAILS input, VIEW_POLLING  VP, string isian = "", string jawaban = "")
        {
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban Captcha Salah / Kosong";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
                return RedirectToAction("CommentMTPS/" + input.POLLING_DETAIL_POLLING_ID);
            }
            else
            {
                var GetIP = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 12").FirstOrDefault();
                var GetUser = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 13").FirstOrDefault();
                var GetPassword = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 14").FirstOrDefault();
                var GetPath = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 15").FirstOrDefault();
                var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");

                string path = "";
                string filePathpdf = "";

                HttpPostedFileBase file4 = Request.Files["POLLING_FILE"];
                if (file4.ContentLength > 0)
                {
                    Directory.CreateDirectory(GetPath.CONFIG_VALUE + "/Upload/DokPolling");
                    path = GetPath.CONFIG_VALUE + "/Upload/DokPolling/";
                    Stream stremdokumen = file4.InputStream;
                    byte[] appData = new byte[file4.ContentLength + 1];
                    stremdokumen.Read(appData, 0, file4.ContentLength);
                    string Extension = Path.GetExtension(file4.FileName);
                    if (Extension.ToLower() == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                        //Aspose.Words.Document docx = new Aspose.Words.Document(stremdokumen);
                        filePathpdf = path + "POLLING_" + VP.PROPOSAL_ID + "_" + TGL_SEKARANG + ".pdf";
                        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    }
                }

                using (OracleConnection con = new OracleConnection("Data Source=" + GetIP.CONFIG_VALUE + ";User ID=" + GetUser.CONFIG_VALUE + ";PASSWORD=" + GetPassword.CONFIG_VALUE + ";"))
                {
                    con.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        var pathnya = "/Upload/DokPolling/POLLING_" + VP.PROPOSAL_ID + "_" + TGL_SEKARANG + ".pdf";                       

                        var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
                        var UserId = Session["USER_ID"];
                        var logcode = MixHelper.GetLogCode();
                        int lastid = MixHelper.GetSequence("TRX_POLLING_DETAILS");
                        var datenow = MixHelper.ConvertDateNow();
                        var year_now = DateTime.Now.Year;
                        //var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_REASON,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS,POLLING_DETAIL_INPUT_TYPE";
                        var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_REASON,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS,POLLING_DETAIL_FILE_PATH,POLLING_DETAIL_INPUT_TYPE";
                        var fvalue = "'" + lastid + "', " +
                                    "'" + input.POLLING_DETAIL_POLLING_ID + "'," +
                                    ":parameter, " +
                                    UserId + ", " +
                                    datenow + "," +
                                    "1," +
                                    "'" + pathnya + "'," +
                                    "2";

                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.CommandText = " INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES ('" + lastid + "','" + input.POLLING_DETAIL_POLLING_ID + "',:parameter," + UserId + "," + datenow + ",1,'" + pathnya + "',2) ";

                        OracleParameter oracleParameterClob = new OracleParameter();
                        oracleParameterClob.OracleDbType = OracleDbType.Clob;
                        //1 million long string
                        oracleParameterClob.Value = input.POLLING_DETAIL_REASON;                      

                        cmd.Parameters.Add(oracleParameterClob);

                        cmd.ExecuteNonQuery();
                        db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING TP SET TP.POLLING_JML_PARTISIPAN = (TP.POLLING_JML_PARTISIPAN + 1) WHERE TP.POLLING_ID =" + input.POLLING_DETAIL_POLLING_ID);
                        TempData["Notifikasi"] = 1;
                        TempData["NotifikasiText"] = "Terima kasih, pendapat anda berhasil di simpan.";
                    }

                    con.Close();

                    return RedirectToAction("CommentMTPS/" + input.POLLING_DETAIL_POLLING_ID);
                }

                //var JUMLAH_PNPS_CODE = db.Database.SqlQuery<String>("SELECT (CASE WHEN LENGTH(TO_CHAR(COUNT(PROPOSAL_ID) + 1)) = 1 THEN '0'||TO_CHAR(COUNT(PROPOSAL_ID) + 1) ELSE TO_CHAR(COUNT(PROPOSAL_ID) + 1) END) AS Jumlah FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID).SingleOrDefault();
                //var PROPOSAL_PNPS_CODE = year_now + "." + USER_KOMTEK_CODE + "." + JUMLAH_PNPS_CODE;

            }

            //var wew = db.Database.SqlQuery<TEST>("SELECT * FROM TEST WHERE TEST_ID = 1").SingleOrDefault();

            //return Json(new { wew, isian }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost, ValidateInput(false)]
        public ActionResult CommentMTPS2(TRX_POLLING_DETAILS input, string jawaban = "")
        {

            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban Captcha Salah / Kosong";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
                return RedirectToAction("CommentMTPS/" + input.POLLING_DETAIL_POLLING_ID);
            }
            else
            {
                var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
                //var USER_KOMTEK_CODE = Session["KOMTEK_CODE"];
                var UserId = Session["USER_ID"];
                var logcode = MixHelper.GetLogCode();
                int lastid = MixHelper.GetSequence("TRX_POLLING_DETAILS");
                var datenow = MixHelper.ConvertDateNow();
                var year_now = DateTime.Now.Year;
                //var JUMLAH_PNPS_CODE = db.Database.SqlQuery<String>("SELECT (CASE WHEN LENGTH(TO_CHAR(COUNT(PROPOSAL_ID) + 1)) = 1 THEN '0'||TO_CHAR(COUNT(PROPOSAL_ID) + 1) ELSE TO_CHAR(COUNT(PROPOSAL_ID) + 1) END) AS Jumlah FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID).SingleOrDefault();
                //var PROPOSAL_PNPS_CODE = year_now + "." + USER_KOMTEK_CODE + "." + JUMLAH_PNPS_CODE;
                var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_REASON,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS,POLLING_DETAIL_INPUT_TYPE";
                var fvalue = "'" + lastid + "', " +
                            "'" + input.POLLING_DETAIL_POLLING_ID + "'," +
                            "'" + input.POLLING_DETAIL_REASON + "', " +
                            UserId + ", " +
                            datenow + "," +
                            "1," +
                            "2";
                //return Json(new { query = "INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING TP SET TP.POLLING_JML_PARTISIPAN = (TP.POLLING_JML_PARTISIPAN + 1) WHERE TP.POLLING_ID =" + input.POLLING_DETAIL_POLLING_ID);
                //String objek = fvalue.Replace("'", "-");
                //MixHelper.InsertLog(logcode, objek, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Terima kasih, pendapat anda berhasil di simpan.";
                //return Json(new { query = "INSERT INTO TRANSACTION_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                return RedirectToAction("CommentMTPS/" + input.POLLING_DETAIL_POLLING_ID);
            }

        }

        public ActionResult Editcommentmtps(int id = 0)
        {
            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                var datapoling = (from poll in db.VIEW_POLLING_DETAIL where poll.POLLING_DETAIL_ID == id && poll.POLLING_TYPE == "2" select poll).SingleOrDefault();
                ViewData["POLLING_ID"] = datapoling.POLLING_DETAIL_POLLING_ID;
                var GetPath = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 15").FirstOrDefault();
                ViewData["pathnya"] = GetPath.CONFIG_VALUE;
                ViewData["data_poll"] = datapoling;
                ViewData["data_mtps"] = (from poll in db.VIEW_POLLING where poll.POLLING_ID == datapoling.POLLING_DETAIL_POLLING_ID select poll).SingleOrDefault();
                var link = (from t in portaldb.SYS_LINK where t.LINK_IS_USE == 1 && t.LINK_ID == 1 select t).SingleOrDefault();
                ViewData["link"] = link;
                ViewData["list_poll_detail"] = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = '" + datapoling.POLLING_DETAIL_POLLING_ID + "' AND POLLING_TYPE = '2' ORDER BY POLLING_DETAIL_CREATE_DATE DESC, POLLING_DETAIL_CREATE_BY ASC, POLLING_DETAIL_OPTION ASC").ToList();
                return View();
            }
            return RedirectToAction("../auth/index");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Editcommentmtps(TRX_POLLING_DETAILS input, VIEW_PROPOSAL vp, string jawaban = "")
        {
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban Captcha Salah / Kosong";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
                return RedirectToAction("CommentMTPS/" + input.POLLING_DETAIL_POLLING_ID);
            }
            else
            {
                var GetIP = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 12").FirstOrDefault();
                var GetUser = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 13").FirstOrDefault();
                var GetPassword = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 14").FirstOrDefault();

                var GetPath = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 15").FirstOrDefault();
                var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");

                string path = "";
                string filePathpdf = "";

                HttpPostedFileBase file4 = Request.Files["file_polling"];
                if (file4.ContentLength > 0)
                {
                    Directory.CreateDirectory(GetPath.CONFIG_VALUE + "/Upload/DokPolling");
                    path = GetPath.CONFIG_VALUE + "/Upload/DokPolling/";
                    Stream stremdokumen = file4.InputStream;
                    byte[] appData = new byte[file4.ContentLength + 1];
                    stremdokumen.Read(appData, 0, file4.ContentLength);
                    string Extension = Path.GetExtension(file4.FileName);
                    if (Extension.ToLower() == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                        //Aspose.Words.Document docx = new Aspose.Words.Document(stremdokumen);
                        filePathpdf = path + "POLLING_" + vp.PROPOSAL_ID + "_" + TGL_SEKARANG + ".pdf";
                        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    }
                }

                using (OracleConnection con = new OracleConnection("Data Source=" + GetIP.CONFIG_VALUE + ";User ID=" + GetUser.CONFIG_VALUE + ";PASSWORD=" + GetPassword.CONFIG_VALUE + ";"))
                {
                    con.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
                        var UserId = Session["USER_ID"];
                        var logcode = MixHelper.GetLogCode();
                        var datenow = MixHelper.ConvertDateNow();
                        var year_now = DateTime.Now.Year;
                        var updatequery = "";
                        if (file4.ContentLength > 0)
                        {
                            updatequery = "UPDATE TRX_POLLING_DETAILS SET " +
                                            "POLLING_DETAIL_REASON = :parameter, " +
                                            "POLLING_DETAIL_FILE_PATH = '/Upload/DokPolling/POLLING_" + vp.PROPOSAL_ID + "_" + TGL_SEKARANG + ".pdf', " +
                                            "POLLING_DETAIL_UPDATE_BY = '" + UserId + "', " +
                                            "POLLING_DETAIL_UPDATE_DATE = " + datenow +
                                            "WHERE POLLING_DETAIL_ID = '" + input.POLLING_DETAIL_ID + "'";
                        }
                        else {
                            updatequery = "UPDATE TRX_POLLING_DETAILS SET " +
                                            "POLLING_DETAIL_REASON = :parameter, " +
                                            "POLLING_DETAIL_UPDATE_BY = '" + UserId + "', " +
                                            "POLLING_DETAIL_UPDATE_DATE = " + datenow +
                                            "WHERE POLLING_DETAIL_ID = '" + input.POLLING_DETAIL_ID + "'";
                        }

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
                        TempData["NotifikasiText"] = "Terima kasih, Komentar anda berhasil di simpan.";
                    }

                    con.Close();



                    return RedirectToAction("CommentMTPS/" + input.POLLING_DETAIL_POLLING_ID);
                }

            }
        }

        [HttpGet]
        public ActionResult FindSNI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_SELECT WHERE LOWER(VIEW_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_SNI_SELECT.TEXT ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_SELECT WHERE LOWER(VIEW_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_SNI_SELECT.TEXT ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.ID, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult FindRegulator(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REGULASI_TEKNIS_SELECT WHERE LOWER(VIEW_REGULASI_TEKNIS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_REGULASI_TEKNIS_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REGULASI_TEKNIS_SELECT WHERE LOWER(VIEW_REGULASI_TEKNIS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_REGULASI_TEKNIS_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_REGULASI_TEKNIS_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindLPK(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_LPK_SELECT WHERE LOWER(VIEW_LPK_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_LPK_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_LPK_SELECT WHERE LOWER(VIEW_LPK_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_LPK_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_LPK_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindNonSNI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindNonSNIAdopsi(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindBIBLIOGRAFI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_BIBLIOGRAFI_SELECT WHERE LOWER(VIEW_BIBLIOGRAFI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_BIBLIOGRAFI_SELECT.TEXT ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_BIBLIOGRAFI_SELECT WHERE LOWER(VIEW_BIBLIOGRAFI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_BIBLIOGRAFI_SELECT.TEXT ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Detail_Usulan_PNPS(int id = 0)
        {
            ViewData["moduleId"] = moduleId;
            //int DataProposal = "";
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var link = (from links in portaldb.SYS_LINK where links.LINK_IS_USE == 1 && links.LINK_STATUS == 1 select links).SingleOrDefault();
            var link1 = (from t in portaldb.SYS_LINK where t.LINK_IS_USE == 1 && t.LINK_ID == 1 select t).SingleOrDefault();
            ViewData["link1"] = link1;
            ViewData["link"] = link.LINK_NAME;
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            ViewData["Outline"] = Outline;
            return View();
            //return Content("isi : "+DataProposal.PROPOSAL_NO_SNI_PROPOSAL);
        }

        public ActionResult Update(int id) {
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["Komtek"] = DataKomtek;
            ViewData["ListKomtek"] = ListKomtek;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            ViewData["Outline"] = Outline;

            return View();
        }


    }
}
