using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Portal.Models;
using Portal.Helpers;
using SISPK.Models;
using System.Security.Cryptography;


namespace Portal.Controllers
{
    public class MainController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        private SISPKEntities portaldb = new SISPKEntities();
        private PortalBsnEntities portal = new PortalBsnEntities();

        private int moduleId = 1;
        // GET: /Main/

        public ActionResult Home()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["link"] = (from a in portal.SYS_LINK where a.LINK_IS_USE == 1 select a).SingleOrDefault(); 
            ViewData["hot_news"] = db.Database.SqlQuery<VIEW_NEWS>("select * from ( select * from VIEW_NEWS VN order by VN.NEWS_CREATE_DATE desc ) where ROWNUM <= 3");
            ViewData["sni_valuasi"] = db.Database.SqlQuery<TRX_SNI_VALUATIONS>("select * from (select * from TRX_SNI_VALUATIONS TSV ORDER BY TSV.SNIVAL_ID desc) WHERE ROWNUM <= 6");
            var jml_komtek = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM MASTER_KOMITE_TEKNIS MKT WHERE KOMTEK_STATUS = 1 AND KOMTEK_PARENT_CODE = '0'").SingleOrDefault();
            ViewData["JML_KOMTEK"] = jml_komtek;
            var jml_subkomtek = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM MASTER_KOMITE_TEKNIS MKT WHERE MKT.KOMTEK_STATUS = 1 AND MKT.KOMTEK_PARENT_CODE != '0'").SingleOrDefault();
            ViewData["JML_SUBKOMTEK"] = jml_subkomtek;
            var jml_anggkomtek = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM VIEW_ANGGOTA MKT WHERE MKT.KOMTEK_ANGGOTA_STATUS = 1 AND KOMTEK_PARENT_CODE = '0'").SingleOrDefault();
            ViewData["JML_ANGGSUBKOMTEK"] = jml_anggkomtek;
            var jml_subanggkomtek = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM VIEW_ANGGOTA MKT WHERE MKT.KOMTEK_ANGGOTA_STATUS = 1 AND KOMTEK_PARENT_CODE != '0'").SingleOrDefault();
            ViewData["JML_SUBANGGSUBKOMTEK"] = jml_subanggkomtek;
            var jml_usulan = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TRX_PROPOSAL MKT WHERE MKT.PROPOSAL_STATUS = 0").SingleOrDefault();
            ViewData["JML_USULAN"] = jml_usulan;
            var jml_RSNI = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TRX_PROPOSAL MKT WHERE MKT.PROPOSAL_STATUS != 0 AND MKT.PROPOSAL_STATUS != 11 AND MKT.PROPOSAL_STATUS != 1 AND MKT.PROPOSAL_STATUS != 2 AND MKT.PROPOSAL_STATUS != 3 AND MKT.PROPOSAL_STATUS != 4").SingleOrDefault();
            ViewData["JML_RSNI"] = jml_RSNI;
            var jml_SNI = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TRX_PROPOSAL MKT WHERE MKT.PROPOSAL_STATUS = 11").SingleOrDefault();
            ViewData["JML_SNI"] = jml_SNI;
            var jml_MTPS = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM VIEW_POLLING MKT WHERE POLLING_TYPE = 2 AND POLLING_MONITORING_TYPE != 'Sudah Lewat'").SingleOrDefault();
            ViewData["jml_MTPS"] = jml_MTPS;
            var jml_JP = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM VIEW_POLLING MKT WHERE POLLING_STATUS = 1 AND POLLING_TYPE = 7 AND POLLING_MONITORING_TYPE != 'Sudah Lewat'").SingleOrDefault();
            ViewData["jml_JP"] = jml_JP;
            ViewData["profile"] = db.Database.SqlQuery<PORTAL_PROFILE>("select * from PORTAL_PROFILE where rownum = 1").SingleOrDefault();

            ViewData["slider"] = (from a in db.PORTAL_SLIDER where a.SLIDER_IMAGE_IS_USE == 1 select a).ToList();
            ViewData["hit_counter"] = db.Database.SqlQuery<VIEW_HIT_COUNTERS>("select * from VIEW_HIT_COUNTERS where COUNTERID = 1").SingleOrDefault();
            //return Json(new { hit = ViewData["hit_counter"] }, JsonRequestBehavior.AllowGet);
            //var ANGKAKOMTEK = db.Database.SqlQuery<MASTER_KOMITE_TEKNIS>("select count(MKT.KOMTEK_ID) AS JML_KOMTEK from MASTER_KOMITE_TEKNIS MKT WHERE MKT.KOMTEK_PARENT_CODE = '0' AND MKT.KOMTEK_STATUS = 1");
            //var angkakomtek = (from angka_komtek in portaldb.VIEW_KOMTEK_ANGKA select angka_komtek).SingleOrDefault();
            //ViewData["angkakomtek"] = angkakomtek;
           
            return View();
        }

        public ActionResult Register()
        {
            ViewData["moduleId"] = moduleId;
            var DataProvinsi = (from provinsi in portaldb.VIEW_WILAYAH_PROVINSI where provinsi.WILAYAH_PARENT_ID == 0 && provinsi.WILAYAH_STATUS == 1 orderby provinsi.WILAYAH_NAMA ascending select provinsi).ToList();
            ViewData["Provinsi"] = DataProvinsi;
            var isError = @TempData["isError"];
            if (isError != null)
            {
                ViewData["Error"] = @TempData["MessageError"];
            }
            return View();
        }

        [HttpPost]
        public ActionResult Register(SYS_USER_PUBLIC sysuser_public, SYS_USER sysuser, string jawaban = "", string browser = "")
        {
            var DATAUSER = new VIEW_USERS_PUBLIC();
            string username = sysuser.USER_NAME;
            //var DATAUSER = portaldb.Database.SqlQuery<decimal>("SELECT COUNT(*) FROM  VIEW_USERS_PUBLIC WHERE  USER_NAME = '" + username + "' AND USER_STATUS = 1 AND ACCESS_STATUS = 1").SingleOrDefault(); ;
            DATAUSER = (from it in portaldb.VIEW_USERS_PUBLIC where it.USER_NAME == username && it.USER_STATUS == 1 && it.ACCESS_STATUS == 1 select it).FirstOrDefault();
            //return Content(""+DATAUSER);
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban Captcha salah";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
                return RedirectToAction("Register");
            }
            else
            {
                if (DATAUSER != null)
                {
                    var MsgError = "Username Sudah Terdaftar";
                    TempData["isError"] = 1;
                    TempData["MessageError"] = MsgError;
                    return RedirectToAction("Register");
                }
                else
                {
                    var logcodePublic = MixHelper.GetLogCode();
                    var logcodeUser = MixHelper.GetLogCode();
                    int lastidUserPublic = MixHelper.GetSequence("SYS_USER_PUBLIC");
                    int lastIdUser = MixHelper.GetSequence("SYS_USER");
                    var datenow = MixHelper.ConvertDateNow();

                    //For Data User Public
                    var KodeActivasi = GenPassword(sysuser_public.USER_PUBLIC_KTPSIM);
                    var fNamePublic = "USER_PUBLIC_ID,USER_PUBLIC_KTPSIM,USER_PUBLIC_NOKK,USER_PUBLIC_NAMA_LENGKAP,USER_PUBLIC_EMAIL,USER_PUBLIC_TELPON,USER_PUBLIC_STAKEHOLDER,USER_PUBLIC_PROVINSI_ID,USER_PUBLIC_KOTAKAB_ID,USER_PUBLIC_CREATE_BY,USER_PUBLIC_CREATE_DATE,USER_PUBLIC_UPDATE_BY,USER_PUBLIC_UPDATE_DATE,USER_PUBLIC_TOKEN_KEY,USER_PUBLIC_ACTIVATION_KEY,USER_PUBLIC_LOG_CODE,USER_PUBLIC_STATUS,USER_PUBLIC_LINK_ACTIVATION";
                    var fValuePublic = "'" + lastidUserPublic + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_KTPSIM + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_NOKK + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_NAMA_LENGKAP + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_EMAIL + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_TELPON + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_STAKEHOLDER + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_PROVINSI_ID + "', " +
                                        "'" + sysuser_public.USER_PUBLIC_KOTAKAB_ID + "', " +
                                        "'" + lastidUserPublic + "'," +
                                        datenow + "," +
                                        "''," +
                                        "''," +
                                        "'" + RandomPassHelper.Generate(20) + "'," +
                                        "'" + KodeActivasi + "'," +
                                        "'" + logcodePublic + "'," +
                                        "1," +
                                        "'" + @Request.Url.GetLeftPart(UriPartial.Authority) + "/main/useraktivasi/" + KodeActivasi + "'";
                    //Insert to Tabel SYS_USER_PUBLIC
                    var insertPublic = portaldb.Database.ExecuteSqlCommand("INSERT INTO SYS_USER_PUBLIC (" + fNamePublic + ") VALUES (" + fValuePublic.Replace("''", "NULL") + ")");
                    String objekPublic = fValuePublic.Replace("'", "-");

                    //Insert Data User Public to Log SYS_LOG
                    MixHelper.InsertLogReg(logcodePublic, objekPublic, 1);
                    string pass = AuthHelper.GenPassword(sysuser.USER_PASSWORD);
                    //string pass = AuthHelper.GenPassword("sispk");
                    //For Data User 
                    var fNameUser = "USER_ID,USER_ACCESS_ID,USER_NAME,USER_PASSWORD,USER_IS_ONLINE,USER_LAST_LOGIN,USER_CREATE_BY,USER_CREATE_DATE,USER_UPDATE_BY,USER_UPDATE_DATE,USER_STATUS,USER_LOG_CODE,USER_TYPE_ID,USER_REF_ID";
                    var fValueUser = "'" + lastIdUser + "', " +
                                "4, " +
                                "'" + sysuser.USER_NAME + "', " +
                                "'" + pass + "', " +
                                "''," +
                                "''," +
                                "'" + lastIdUser + "'," +
                                datenow + "," +
                                "''," +
                                "''," +
                                "1, " +
                                "'" + logcodeUser + "'," +
                                "3, " +
                                "'" + lastidUserPublic + "'";
                    //Insert to Tabel SYS_USER_PUBLIC

                    //return Json(new { query = "INSERT INTO SYS_USER (" + fNameUser + ") VALUES (" + fValueUser.Replace("''", "NULL") + ")" });
                    var insertUser = portaldb.Database.ExecuteSqlCommand("INSERT INTO SYS_USER (" + fNameUser + ") VALUES (" + fValueUser.Replace("''", "NULL") + ")");
                    String objekUser = fValueUser.Replace("'", "-");

                    //Insert Data User to Log SYS_LOG
                    MixHelper.InsertLogReg(logcodeUser, objekUser, 1);

                    //Send Account Activation to Email
                    var email = (from t in db.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();

                    SendMailHelper.MailUsername = email.EMAIL_NAME;     //"aleh.mail@gmail.com";
                    SendMailHelper.MailPassword = email.EMAIL_PASSWORD; //"r4h45143uy";

                    SendMailHelper mailer = new SendMailHelper();
                    mailer.ToEmail = sysuser_public.USER_PUBLIC_EMAIL;
                    mailer.Subject = "Registrasi Member Baru - Sistem Informasi SNI";
                    var isiEmail = "Terimakasih telah Melakukan Registrasi pada sistem kami. <br />";
                    isiEmail += "Username   : " + sysuser.USER_NAME + "<br />";
                    isiEmail += "Password   : " + sysuser.USER_PASSWORD + "<br />";
                    //isiEmail += "Detail aktivasi akun anda sedang Menunggu Persetujuan administrator <br />";
                    //isiEmail += "Apabila data akun anda disetujui oleh administrator maka anda akan menerima email aktivasi <br />";
                    isiEmail += "Sekarang Anda bisa melakukan login di Sistem Informasi SNI <br />";
                    isiEmail += "Demikian Informasi yang kami sampaikan, atas kerjasamanya kami ucapkan terimakasih. <br />";
                    isiEmail += "<span style='text-align:right;font-weight:bold;margin-top:20px;'>Web Administrator</span>";

                    mailer.Body = isiEmail;
                    mailer.IsHtml = true;
                    mailer.Send();

                    TempData["MailMember"] = sysuser_public.USER_PUBLIC_EMAIL;
                    return RedirectToAction("RegSukses");
                }
                
            }
        }

        [HttpPost]
        public JsonResult GetKotaKab(int id = 0)
        {
            var list = (from kotakab in portaldb.VIEW_WILAYAH_KABUPATEN where kotakab.WILAYAH_PARENT_ID == id orderby kotakab.WILAYAH_NAMA ascending select kotakab).ToList();
            var result = from lists in list
                         select new string[] 
           { 
               Convert.ToString("<option value='"+lists.WILAYAH_ID+"'>" +lists.WILAYAH_NAMA+"</option>")
           };
            return Json(new
            {
                message = 1,
                value = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RegProses(SYS_USER_PUBLIC sysuser_public, SYS_USER sysuser, string jawaban = "", string browser = "")
        {
            return View();
        }

        //[HttpPost]
        public ActionResult RegSukses()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["EmailPublic"] = @TempData["MailMember"];
            return View();
        }

        [ValidateInput(false)]
        public ActionResult UserAktivasi(String id="")
        {
            ViewData["moduleId"] = moduleId;
            var cekCountUserByCodeActivate  = portaldb.SYS_USER_PUBLIC.SqlQuery("SELECT * FROM SYS_USER_PUBLIC WHERE USER_PUBLIC_ACTIVATION_KEY = '" + id + "'  AND USER_PUBLIC_STATUS = 0 ").Count();

            //return Json(new { test = cekCountUserByCodeActivate }, JsonRequestBehavior.AllowGet);

            //var  userPublic = 
            var getDateRegByCodeActivate = (from userpublic in portaldb.SYS_USER_PUBLIC where userpublic.USER_PUBLIC_ACTIVATION_KEY == id select userpublic).SingleOrDefault();

            var getUIdRegByUsrRefId = (from usersys in portaldb.SYS_USER where usersys.USER_REF_ID == getDateRegByCodeActivate.USER_PUBLIC_ID && usersys.USER_TYPE_ID == 3 select usersys).SingleOrDefault();

            var TimetoString = ((DateTime)getDateRegByCodeActivate.USER_PUBLIC_CREATE_DATE).ToString("yyyy-MM-dd HH:mm:ss");
            DateTime dt2 = DateTime.ParseExact(TimetoString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            TimeSpan timeSince = DateTime.Now.Subtract(dt2);

            var totalJamAktivasi = (int) timeSince.TotalHours;

            ViewData["Message"] = "";
            //ViewData["Message"] = cekCountUserByCodeActivate + " - " + totalJamAktivasi;

            if (cekCountUserByCodeActivate == 1 && totalJamAktivasi <= 24)
            {
                var datenow = MixHelper.ConvertDateNow();
                var logcode = MixHelper.GetLogCode();
                var fupdate1 = "USER_STATUS = '1'," +
                                "USER_UPDATE_BY = '" + getUIdRegByUsrRefId.USER_ID + "'," +
                                "USER_UPDATE_DATE = " + datenow;
                var fupdate2 = "USER_PUBLIC_STATUS = '1'," +
                                "USER_PUBLIC_UPDATE_BY = '" + getUIdRegByUsrRefId.USER_ID + "'," +
                                "USER_PUBLIC_UPDATE_DATE = " + datenow;
                portaldb.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + fupdate1 + " WHERE USER_ID = " + getUIdRegByUsrRefId.USER_ID);
                portaldb.Database.ExecuteSqlCommand("UPDATE SYS_USER_PUBLIC SET " + fupdate2 + " WHERE USER_PUBLIC_ID = " + getDateRegByCodeActivate.USER_PUBLIC_ID);

                String objek = fupdate2.Replace("'", "-");
                MixHelper.InsertLogActivate(logcode, (int)getUIdRegByUsrRefId.USER_ID, (int)getUIdRegByUsrRefId.USER_ACCESS_ID, objek, 2);
                ViewData["Note"] = "note-success";
                ViewData["Message"] = "Akun anda berhasil diaktivasi, silahkan login dengan menggunakan username dan password yang sudah kami kirimkan ke email";
                //TempData["NotifSukses"] = "Akun anda berhasil diaktivasi, silahkan login dengan menggunakan username dan password yang sudah kami kirimkan ke email";
                return RedirectToAction("index","auth");
            }
            else
            {
                ViewData["Note"] = "note-danger";
                ViewData["Message"] = "Maaf, Kode Aktivasi anda sudah kadaluarsa karena anda tidak melakukan aktivasi akun anda selama 24 jam. Silahkan menghubungi Team Support kami untuk mengaktivasi akun anda.";
            }
            return View();
        }

        public string GenPassword(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        //public void KirimEmail()
        //{

        //    SendMailHelper.MailUsername = "aleh.mail@gmail.com";
        //    SendMailHelper.MailPassword = "r4h45143uy";

        //    SendMailHelper mailer = new SendMailHelper();
        //    mailer.ToEmail = "abdi.aleh@gmail.com";
        //    mailer.Subject = "Verify your email id";
        //    mailer.Body = "Thanks for Registering your account.<br> please verify your email id by clicking the link <br> <a href='youraccount.com/verifycode=12323232'>verify</a>";
        //    mailer.IsHtml = true;
        //    mailer.Send();
        //}
        
    }
}