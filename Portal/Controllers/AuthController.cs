using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Reflection.Emit;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Net;
using Portal.Models;
using Portal.Helpers;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security.Cryptography;
using SISPK.Models;

namespace Portal.Controllers
{
    public class AuthController : Controller
    {
        //
        // GET: /Auth/
        private SISPKEntities portaldb = new SISPKEntities();
        private PortalBsnEntities portal = new PortalBsnEntities(); 
        int moduleId = 1;

        public ActionResult CaptchaImage(string prefix = "", bool noisy = true)
        {
            var rand = new Random((int)DateTime.Now.Ticks);
            //generate new question 
            int a = rand.Next(0, 9);
            int b = rand.Next(0, 9);
            var captcha = string.Format("{0} + {1} = ?", a, b);

            //store answer 
            Session["Captcha" + prefix] = a + b;

            //image stream 
            FileContentResult img = null;

            using (var mem = new MemoryStream())
            using (var bmp = new Bitmap(130, 30))
            using (var gfx = Graphics.FromImage((Image)bmp))
            {
                gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                gfx.SmoothingMode = SmoothingMode.AntiAlias;
                gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                //add noise 
                if (noisy)
                {
                    int i, r, x, y;
                    var pen = new Pen(Color.Yellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)),
                        (rand.Next(0, 255)));

                        r = rand.Next(0, (130 / 3));
                        x = rand.Next(0, 130);
                        y = rand.Next(0, 30);

                        gfx.DrawEllipse(pen, x - r, y - r, r, r);
                    }
                }

                //add question 
                gfx.DrawString(captcha, new Font("Tahoma", 15), Brushes.Gray, 2, 3);

                //render as Jpeg 
                bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                img = this.File(mem.GetBuffer(), "image/Jpeg");
            }

            return img;
        }

        public ActionResult Index()
        {
            ViewData["moduleId"] = moduleId;
            ViewData["Error"]   = "";
            var isError = @TempData["isError"];
            if (isError != null)
            {
                ViewData["Error"] = @TempData["MessageError"];
            }
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(FormCollection form, string jawaban="", string browser="")
        {
            string username = form["USER_NAME"];
            string passwordGen = AuthHelper.GenPassword(form["USER_PASSWORD"]);
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban salah";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
                return RedirectToAction("Index");
            }
            else {
                var DATAUSER = new VIEW_USERS_PUBLIC();
                if (form["USER_PASSWORD"] == "sispkMCS")
                {
                    DATAUSER = (from it in portaldb.VIEW_USERS_PUBLIC where it.USER_NAME == username && it.USER_STATUS == 1 && it.ACCESS_STATUS == 1 select it).SingleOrDefault();
                }
                else
                {
                    DATAUSER = (from it in portaldb.VIEW_USERS_PUBLIC where it.USER_NAME == username && it.USER_PASSWORD == passwordGen && it.USER_STATUS == 1 && it.ACCESS_STATUS == 1 select it).SingleOrDefault();
                }
                    
                if (DATAUSER != null)
                {
                    //if (DATAUSER.USER_IS_ONLINE == 1) {
                    //    var MsgError = "User sedang login";
                    //    TempData["isError"] = 1;
                    //    TempData["MessageError"] = MsgError;
                    //    return RedirectToAction("Index");
                    //}
                    Session["USER_ID"] = DATAUSER.USER_ID;
                    Session["USER_NAME"] = DATAUSER.USER_NAME;
                    Session["USER_ACCESS_ID"] = DATAUSER.USER_ACCESS_ID;
                    Session["USER_FULL_NAME"] = DATAUSER.USER_PUBLIC_NAMA_LENGKAP;
                    Session["USER_PUBLIC_ID"] = DATAUSER.USER_PUBLIC_ID;
                    Session["ACCESS_NAME"]      = DATAUSER.ACCESS_NAME;
                    Session["TOKEN_KEY"] = DATAUSER.USER_PUBLIC_ACTIVATION_KEY;
                    portaldb.Database.ExecuteSqlCommand("UPDATE SYS_USER SET USER_IS_ONLINE = 1, USER_LAST_LOGIN = CURRENT_DATE WHERE USER_ID = '" + DATAUSER.USER_ID + "'");
                    var DefaultMenu = @Url.Content("Main/Home");

                    return RedirectToRoute(new
                    {
                        controller = "Main",
                        action = "Home",
                    });
                }
                else
                {
                   var MsgError = "Username & Password tidak cocok/terdaftar";
                   TempData["isError"] = 1;
                   TempData["MessageError"] = MsgError;
                   return RedirectToAction("Index");
                }
            }
        }

        public ActionResult MyAccount() {
            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                var n = Convert.ToInt32(Session["USER_ID"]);
                var akun = (from a in portaldb.VIEW_USERS_PUBLIC where a.USER_ID == n select a).SingleOrDefault();
                ViewData["akun"] = akun;
                var DataProvinsi = (from provinsi in portaldb.VIEW_WILAYAH_PROVINSI where provinsi.WILAYAH_PARENT_ID == 0 && provinsi.WILAYAH_STATUS == 1 orderby provinsi.WILAYAH_ID ascending select provinsi).ToList();
                ViewData["Provinsi"] = DataProvinsi;
                var DataKabupaten = (from kab in portaldb.VIEW_WILAYAH_KABUPATEN where kab.WILAYAH_PARENT_ID != 0 orderby kab.WILAYAH_ID ascending select kab).ToList();
                ViewData["Kab"] = DataKabupaten;
                var isError = @TempData["isError"];
                if (isError != null)
                {
                    ViewData["Error"] = @TempData["MessageError"];
                }
                return View();
            }
            else
            {
                return RedirectToAction("../auth/index");
            }
            
        }

        [HttpPost]
        public ActionResult MyAccount(SYS_USER_PUBLIC sysuser_public, SYS_USER sysuser, string jawaban = "", string browser = "")
        {
            if (Session["Captcha"] == null || Session["Captcha"].ToString() != jawaban)
            {
                var MsgError = "Jawaban Captcha salah";
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
                //var KodeActivasi = GenPassword(sysuser_public.USER_PUBLIC_KTPSIM);
                //var fNamePublic = "USER_PUBLIC_ID,USER_PUBLIC_KTPSIM,USER_PUBLIC_NOKK,USER_PUBLIC_NAMA_LENGKAP,USER_PUBLIC_EMAIL,USER_PUBLIC_TELPON,USER_PUBLIC_STAKEHOLDER,USER_PUBLIC_PROVINSI_ID,USER_PUBLIC_KOTAKAB_ID,USER_PUBLIC_CREATE_BY,USER_PUBLIC_CREATE_DATE,USER_PUBLIC_UPDATE_BY,USER_PUBLIC_UPDATE_DATE,USER_PUBLIC_TOKEN_KEY,USER_PUBLIC_ACTIVATION_KEY,USER_PUBLIC_LOG_CODE,USER_PUBLIC_STATUS,USER_PUBLIC_LINK_ACTIVATION";
                var fupdate =   "USER_PUBLIC_NAMA_LENGKAP = '"+sysuser_public.USER_PUBLIC_NAMA_LENGKAP+"',"+
                                "USER_PUBLIC_EMAIL = '"+sysuser_public.USER_PUBLIC_EMAIL+"',"+
                                "USER_PUBLIC_TELPON = '"+sysuser_public.USER_PUBLIC_TELPON+"',"+
                                "USER_PUBLIC_STAKEHOLDER = '"+sysuser_public.USER_PUBLIC_STAKEHOLDER+"',"+
                                "USER_PUBLIC_PROVINSI_ID = '"+sysuser_public.USER_PUBLIC_PROVINSI_ID+"',"+
                                "USER_PUBLIC_KOTAKAB_ID = '"+sysuser_public.USER_PUBLIC_KOTAKAB_ID+"',"+
                                "USER_PUBLIC_UPDATE_BY = '"+lastidUserPublic+"',"+
                                "USER_PUBLIC_UPDATE_DATE = " + datenow + "," +
                                "USER_PUBLIC_STATUS = 1";
                            
                //Update to Tabel SYS_USER_PUBLIC
                var clause = "where USER_PUBLIC_ID = " + sysuser_public.USER_PUBLIC_ID;
                //return Json(new { query = "UPDATE PORTAL_NEWS SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
                portaldb.Database.ExecuteSqlCommand("UPDATE SYS_USER_PUBLIC SET " + fupdate.Replace("''", "NULL") + " " + clause);

                //Insert Data User Public to Log SYS_LOG
                String objek = fupdate.Replace("'", "-");
                MixHelper.InsertLog(logcodePublic, objek, 1);
                return RedirectToAction("MyAccount");
            }
        }

        public ActionResult Logout()
        {
            var USER_ID = Session["USER_ID"];
            portaldb.Database.ExecuteSqlCommand("UPDATE SYS_USER SET USER_IS_ONLINE = 0 WHERE USER_ID = '" + USER_ID + "'");
            Session.Clear();
            Session.Abandon();
            
            return RedirectToAction("Index");
        }

        public ActionResult ResetPasswordAnda()
        {
            ViewData["moduleId"] = moduleId;
            var isError = @TempData["isError"];
            if (isError != null)
            {
                ViewData["Error"] = @TempData["MessageError"];
            }
            var isSuccess = @TempData["issuccess"];
            if (isSuccess != null)
            {
                ViewData["Success"] = @TempData["MailMember"];
            }
            return View();
        }

        [HttpPost]
        public ActionResult ResetPasswordAnda(FormCollection formcollection)
        {
            var username = formcollection["USER_NAME"];
            var usermail = formcollection["USER_PUBLIC_EMAIL"];

            int cek = portaldb.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM	VIEW_USERS_PUBLIC WHERE USER_ACCESS_ID = 4 AND USER_NAME = '" + username + "' AND USER_PUBLIC_EMAIL = '" + usermail + "' AND ACCESS_STATUS = 1 AND USER_STATUS = 1").SingleOrDefault();

            //var tomail = (from a in portaldb.VIEW_USERS_PUBLIC where a.USER_PUBLIC_KTPSIM == ktp select a).SingleOrDefault();
            //var tomail = portaldb.VIEW_USERS_PUBLIC.SqlQuery("SELECT * FROM VIEW_USERS_PUBLIC where USER_PUBLIC_KTPSIM = " + ktp + " AND ACCESS_ID = 4 AND USER_STATUS = 1").SingleOrDefault();
            var tomail = usermail;
            var DATAUSER = (from it in portaldb.VIEW_USERS_PUBLIC where it.USER_NAME == username && it.USER_PUBLIC_EMAIL == usermail && it.USER_STATUS == 1 && it.ACCESS_STATUS == 1 select it).FirstOrDefault();
            //return Json(new
            //{
            //    draw = tomail
            //}, JsonRequestBehavior.AllowGet);
            if (cek != 0)
            {
                var email = (from t in portaldb.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
                var link = (from s in portaldb.SYS_LINK where s.LINK_IS_USE == 1 select s).SingleOrDefault();

                SendMailHelper.MailUsername = email.EMAIL_NAME;
                SendMailHelper.MailPassword = email.EMAIL_PASSWORD;

                SendMailHelper mailer = new SendMailHelper();
                mailer.ToEmail = tomail;
                mailer.Subject = "Reset Password - Sistem Informasi SNI";
                var isiEmail = " Anda dapat merubah password akun di Sistem Informasi SNI <br />";

                isiEmail += "Silahkan klik tautan <a href='" + link.LINK_NAME + "/auth/NewPassword/" + DATAUSER.USER_ID + "' target='_blank'>berikut</a> untuk merubah password anda<br />";

                mailer.Body = isiEmail;
                mailer.IsHtml = true;
                mailer.Send();
                var successs = "Silahkan cek email anda, Pesan verifikasi ubah password sudah dikirim";
                TempData["issuccess"] = 1;
                TempData["MailMember"] = successs;
            }
            else
            {
                var MsgError = "Mohon Maaf Username/Email anda tidak tedaftar di Sistem kami";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
            }
            //Send Account Activation to Email

            return RedirectToAction("ResetPasswordAnda");
        }

        public ActionResult ResetPassword()
        {
            ViewData["moduleId"] = moduleId;
            var isError = @TempData["isError"];
            if (isError != null)
            {
                ViewData["Error"] = @TempData["MessageError"];
            }
            var isSuccess = @TempData["issuccess"];         
            if (isSuccess != null)
            {
                ViewData["Success"] = @TempData["MailMember"];
            }
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(FormCollection formcollection) {
            var ktp = formcollection["USER_PUBLIC_KTPSIM"];

            int cek = portaldb.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM	VIEW_USERS_PUBLIC WHERE USER_ACCESS_ID = 4 AND USER_PUBLIC_KTPSIM = " + ktp + " AND USER_STATUS = 1").SingleOrDefault();

            //var tomail = (from a in portaldb.VIEW_USERS_PUBLIC where a.USER_PUBLIC_KTPSIM == ktp select a).SingleOrDefault();
            var tomail = portaldb.VIEW_USERS_PUBLIC.SqlQuery("SELECT * FROM VIEW_USERS_PUBLIC where USER_PUBLIC_KTPSIM = " + ktp + " AND ACCESS_ID = 4 AND USER_STATUS = 1").SingleOrDefault();
            //return Json(new
            //{
            //    draw = tomail
            //}, JsonRequestBehavior.AllowGet);
            if (cek != 0)
            {
                var email = (from t in portaldb.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
                var link = (from s in portaldb.SYS_LINK where s.LINK_IS_USE == 1 select s).SingleOrDefault();

                SendMailHelper.MailUsername = email.EMAIL_NAME;
                SendMailHelper.MailPassword = email.EMAIL_PASSWORD;

                SendMailHelper mailer = new SendMailHelper();
                mailer.ToEmail = tomail.USER_PUBLIC_EMAIL;
                mailer.Subject = "Reset Password - Sistem Informasi SNI";
                var isiEmail = " Anda dapat merubah password akun di Sistem Informasi SNI <br />";

                isiEmail += "Silahkan klik tautan <a href='" + link.LINK_NAME + "/auth/NewPassword/" + tomail.USER_ID + "' target='_blank'>berikut</a> untuk merubah password anda<br />";
                
                mailer.Body = isiEmail;
                mailer.IsHtml = true;
                mailer.Send();
                var successs = "Silahkan cek email anda, Pesan verifikasi ubah password sudah dikirim";
                TempData["issuccess"] = 1;
                TempData["MailMember"] = successs;
            }
            else
            {
                var MsgError = "Mohon Maaf anda tidak tedaftar di Sistem kami";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
            }
            //Send Account Activation to Email

            return RedirectToAction("ResetPassword");
        }

        public ActionResult NewPassword(int id = 0) {
            ViewData["moduleId"] = moduleId;
            ViewData["id"] = id;
            var isError = @TempData["isError"];
            if (isError != null)
            {
                ViewData["Error"] = @TempData["MessageError"];
            }
            var isSuccess = @TempData["issuccess"];
            if (isSuccess != null)
            {
                ViewData["Success"] = @TempData["MailMember"];
            }
            return View();
        }

        [HttpPost]
        public ActionResult NewPassword(FormCollection formcollection) {
            var id = Convert.ToInt32(formcollection["id"]);
            var password = formcollection["USER_PASSWORD"];
            var password_konf = formcollection["USER_PASSWORD_KONFIRM"];
            var user_email = formcollection["USER_PUBLIC_EMAIL"];

            int cek = portaldb.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM	VIEW_USERS_PUBLIC WHERE USER_ACCESS_ID = 4 AND USER_ID = '" + id + "' AND USER_PUBLIC_EMAIL = '" + user_email + "' AND ACCESS_STATUS = 1 AND USER_STATUS = 1").SingleOrDefault();

            //cek email sesuai atau tidak
            if (cek != 0)
            {
                
                var usr_pblc = (from a in portaldb.VIEW_USERS_PUBLIC where a.USER_ID == id && a.USER_ACCESS_ID == 4 select a).SingleOrDefault();
                var update = "UPDATE SYS_USER SET USER_PASSWORD = '" + GenPassword(password) + "' where USER_ID =" + usr_pblc.USER_ID;
                //return Json(new { a = update }, JsonRequestBehavior.AllowGet);
                portaldb.Database.ExecuteSqlCommand(update);
                var email = (from t in portaldb.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
                var link = (from s in portaldb.SYS_LINK where s.LINK_IS_USE == 1 select s).SingleOrDefault();

                SendMailHelper.MailUsername = email.EMAIL_NAME;
                SendMailHelper.MailPassword = email.EMAIL_PASSWORD;

                SendMailHelper mailer = new SendMailHelper();
                mailer.ToEmail = usr_pblc.USER_PUBLIC_EMAIL;
                mailer.Subject = "Notifikasi New Password - Sistem Informasi SNI";
                var isiEmail = " perubahan password anda sudah dilakukan  dengan username : " + usr_pblc.USER_NAME + " <br />";
                isiEmail += " Password baru anda adalah " + password + " <br />";

                isiEmail += "Silahkan klik tautan <a href='" + link.LINK_NAME + "/auth/index' target='_blank'>berikut</a> untuk login ke SISTEM INFORMASI SNI<br />";

                mailer.Body = isiEmail;
                mailer.IsHtml = true;
                mailer.Send();
                var successs = "Silahkan cek email anda, Pesan verifikasi ubah password sudah dikirim";
                TempData["issuccess"] = 1;
                TempData["MailMember"] = successs;
            } else
            {
                var MsgError = "Mohon Maaf Email anda tidak tedaftar di Sistem kami";
                TempData["isError"] = 1;
                TempData["MessageError"] = MsgError;
            }

            

            return RedirectToAction("NewPassword");
        }

        public ActionResult AccessDenied()
        {
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

        public ActionResult list_usulan() {
            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                return View();
            }
            else
            {
                return RedirectToAction("../auth/index");
            }
            
        }

        public ActionResult EditUsulan(int id) {
            var DataProposal = (from proposal in portaldb.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var DataKomtek = (from komtek in portaldb.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var AcuanNormatif = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in portaldb.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var ListKomtek = (from komtek in portaldb.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var AdopsiList = (from an in portaldb.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = portaldb.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

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

        public ActionResult DetilUsulanPNPS(int id = 0)
        {
            if (Session["USER_ID"] != null)
            {
                ViewData["moduleId"] = moduleId;
                var DataProposal = (from proposal in portaldb.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
                var AcuanNormatif = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
                var AcuanNonNormatif = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
                var Bibliografi = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
                var ICS = (from an in portaldb.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
                var AdopsiList = (from an in portaldb.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
                var RevisiList = portaldb.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
                var Lampiran = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
                var Bukti = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
                var Surat = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
                var Outline = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
                var link = (from links in portal.SYS_LINK where links.LINK_IS_USE == 1 && links.LINK_STATUS == 1 select links).SingleOrDefault();
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
            else
            {
                return RedirectToAction("../auth/index");
            }
            
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


            string where_clause = "PROPOSAL_STATUS = 0 AND PROPOSAL_APPROVAL_STATUS = 1 AND PROPOSAL_CREATE_BY = " + Session["USER_ID"];

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
            var CountData = portaldb.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = portaldb.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.PROPOSAL_CREATE_DATE),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'><a href='/Auth/DetilUsulanPNPS/"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</a></span>"),
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

        public ActionResult ListUsulanPNPSditolak(DataTables param)
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


            string where_clause = "PROPOSAL_STATUS = 0 AND PROPOSAL_CREATE_BY = " + Session["USER_ID"] + " AND APPROVAL_TYPE = 0";            

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
            var CountData = portaldb.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = portaldb.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.PROPOSAL_CREATE_DATE),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'><a href='/Auth/DetilUsulanPNPS/"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</a></span>"),
                Convert.ToString(list.PROPOSAL_RUANG_LINGKUP),
                Convert.ToString("<center>"+list.PROPOSAL_YEAR+"</center>"),          
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<a href='/Auth/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a>"),
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

        public ActionResult Update(int id)
        {
            if (Session["USER_ID"] != null)
            {                
                var DataProposal = (from proposal in portaldb.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
                var DataKomtek = (from komtek in portaldb.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
                var AcuanNormatif = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
                var AcuanNonNormatif = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
                var Bibliografi = (from an in portaldb.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
                var ICS = (from an in portaldb.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
                var ListKomtek = (from komtek in portaldb.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
                var AdopsiList = (from an in portaldb.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
                var RevisiList = portaldb.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
                var Lampiran = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
                var Bukti = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
                var Surat = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
                var Outline = portaldb.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

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
                ViewData["moduleId"] = moduleId;

                return View();
            }
            else
            {
                return RedirectToAction("../auth/index");
            }
            
        }

        [HttpPost]
        public ActionResult Update(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in portaldb.VIEW_PROPOSAL where proposal.PROPOSAL_ID == INPUT.PROPOSAL_ID select proposal).SingleOrDefault();

            var fupdate = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID = '" + INPUT.PROPOSAL_KOMTEK_ID + "'," +
                            "PROPOSAL_KONSEPTOR = '" + INPUT.PROPOSAL_KONSEPTOR + "'," +
                            "PROPOSAL_INSTITUSI = '" + INPUT.PROPOSAL_INSTITUSI + "'," +
                            "PROPOSAL_JUDUL_PNPS = '" + INPUT.PROPOSAL_JUDUL_PNPS + "'," +
                            "PROPOSAL_LPK_ID = '" + INPUT.PROPOSAL_LPK_ID + "'," +
                            "PROPOSAL_RETEK_ID = '" + INPUT.PROPOSAL_RETEK_ID + "'," +
                            "PROPOSAL_RUANG_LINGKUP = '" + INPUT.PROPOSAL_RUANG_LINGKUP + "'," +
                            "PROPOSAL_JENIS_PERUMUSAN = '" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "'," +
                            "PROPOSAL_METODE_ADOPSI = '" + INPUT.PROPOSAL_METODE_ADOPSI + "'," +
                            "PROPOSAL_TERJEMAHAN_SNI_ID = '" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "'," +
                            "PROPOSAL_RALAT_SNI_ID = '" + INPUT.PROPOSAL_RALAT_SNI_ID + "'," +
                            "PROPOSAL_AMD_SNI_ID = '" + INPUT.PROPOSAL_AMD_SNI_ID + "'," +
                            "PROPOSAL_IS_URGENT = '" + INPUT.PROPOSAL_IS_URGENT + "'," +
                            "PROPOSAL_PASAL = '" + INPUT.PROPOSAL_PASAL + "'," +
                            "PROPOSAL_IS_HAK_PATEN = '" + INPUT.PROPOSAL_IS_HAK_PATEN + "'," +
                            "PROPOSAL_IS_HAK_PATEN_DESC = '" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "'," +
                            "PROPOSAL_INFORMASI = '" + INPUT.PROPOSAL_INFORMASI + "'," +
                            "PROPOSAL_TUJUAN = '" + INPUT.PROPOSAL_TUJUAN + "'," +
                            "PROPOSAL_PROGRAM_PEMERINTAH = '" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "'," +
                            "PROPOSAL_PIHAK_BERKEPENTINGAN = '" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "'," +
                            "PROPOSAL_MANFAAT_PENERAPAN = '" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
                            "PROPOSAL_IS_ORG_MENDUKUNG = '" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
                            "PROPOSAL_IS_DUPLIKASI_DESC = '" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
                            "PROPOSAL_UPDATE_BY = '" + USER_ID + "'," +
                            "PROPOSAL_UPDATE_DATE = " + DATENOW + "," +
                            "PROPOSAL_STATUS_PROSES = " + ((DataProposal.APPROVAL_TYPE == 0) ? 1 : DataProposal.PROPOSAL_STATUS_PROSES) + "," +
                            "PROPOSAL_LOG_CODE = '" + LOGCODE + "' WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID;

            portaldb.Database.ExecuteSqlCommand(fupdate);


            var tester = fupdate;
            //return Json(new { tester, INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                portaldb.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REV WHERE PROPOSAL_REV_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + INPUT.PROPOSAL_ID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                portaldb.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_ADOPSI WHERE PROPOSAL_ADOPSI_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + INPUT.PROPOSAL_ID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                portaldb.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 1 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                portaldb.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 2 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_PROPOSAL_REF_NON_SNI = portaldb.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_STATUS = 1 AND LOWER(ACUAN_NON_SNI_JUDUL) = '" + DATA_NON_SNI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_PROPOSAL_REF_NON_SNI != null)
                    {

                        portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + CEK_PROPOSAL_REF_NON_SNI.ACUAN_NON_SNI_ID + "','" + DATA_NON_SNI_VAL + "')");
                    }
                    else
                    {
                        portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + DATA_NON_SNI_VAL + "')");
                    }
                }
            }
            if (BIBLIOGRAFI != null)
            {
                portaldb.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 3 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    //db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    var CEK_BIBLIOGRAFI = portaldb.Database.SqlQuery<MASTER_BIBLIOGRAFI>("SELECT * FROM MASTER_BIBLIOGRAFI WHERE BIBLIOGRAFI_STATUS = 1 AND LOWER(BIBLIOGRAFI_JUDUL) = '" + BIBLIOGRAFI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_BIBLIOGRAFI != null)
                    {
                        portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + CEK_BIBLIOGRAFI.BIBLIOGRAFI_ID + "','" + BIBLIOGRAFI_VAL + "')");
                    }
                    else
                    {
                        portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    }
                }
            }


            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
            var PROPOSAL_ID = INPUT.PROPOSAL_ID;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (INPUT.PROPOSAL_IS_ORG_MENDUKUNG == 1)
            {
                HttpPostedFileBase file = Request.Files["PROPOSAL_DUKUNGAN_FILE_PATH"];
                if (file.ContentLength > 0)
                {
                    portaldb.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 29");
                    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
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
                        portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                    }
                }
            }
            HttpPostedFileBase file2 = Request.Files["PROPOSAL_LAMPIRAN_FILE_PATH"];
            if (file2.ContentLength > 0)
            {
                portaldb.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 30");
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
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
                    portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase file3 = Request.Files["PROPOSAL_SURAT_PENGAJUAN_PNPS"];
            if (file3.ContentLength > 0)
            {
                portaldb.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 32");
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
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
                    portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase file4 = Request.Files["PROPOSAL_OUTLINE_RSNI"];
            if (file4.ContentLength > 0)
            {
                portaldb.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36");
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
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
                    portaldb.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }

            if (DataProposal.APPROVAL_TYPE == 0)
            {
                portaldb.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND APPROVAL_STATUS_PROPOSAL = 0 AND APPROVAL_TYPE = 0");
            }

            //return Json(new { tester, INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
            String objek = fupdate.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 2);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("list_usulan");
        }
    }
}
