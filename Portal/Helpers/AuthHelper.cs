using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Portal.Models;
using System.Text;
using System.Security.Cryptography;

namespace Portal.Helpers
{
    public class AuthHelper
    {
        private PortalBsnEntities db = new PortalBsnEntities();
        public static int punya_sub(int id)
        {
            using (var db = new PortalBsnEntities())
            {
                var USER_ACCESS_ID = Convert.ToInt32(System.Web.HttpContext.Current.Session["USER_ACCESS_ID"]);
                var jml = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SYS_MENU_PORTAL WHERE MENU_PARENT_ID=" + id).SingleOrDefault();
                return jml;
            }
        }
        public static string buat_menu(int id = 1)
        {
            using (var db = new PortalBsnEntities())
            {
                
                var menu = ""; // inisialisasi awal
                var USER_ACCESS_ID = Convert.ToInt32(System.Web.HttpContext.Current.Session["USER_ACCESS_ID"]);
                var hasil = db.Database.SqlQuery<SYS_MENU_PORTAL>("SELECT * FROM SYS_MENU_PORTAL WHERE MENU_PARENT_ID = 0 AND MENU_POSITION = 1 AND MENU_STATUS = 1").ToList();

                foreach (var res in hasil)
                {
                    var isaktif = "";
                    if (id == Convert.ToInt32(res.MENU_ID)) { 
                        isaktif = "active"; 
                    }
                    var cek = punya_sub(Convert.ToInt32(res.MENU_ID));
                    if (cek > 0)
                    {
                        menu += "<li class='dropdown "+ isaktif +"'>" +
                         "<a class='dropdown-toggle limenu' data-toggle='dropdown' data-target='' href='" + res.MENU_URL + "' >" + res.MENU_NAME + "</a>" +
                         "<ul class='dropdown-menu'>";
                        menu += buat_anak_menu(Convert.ToInt32(res.MENU_ID));
                        menu += "</ul></li>";
                    }
                    else
                    {
                        menu += "<li class='" + isaktif + "'>" +
                                   "<a class='limenu' href='" + res.MENU_URL + "' >" + res.MENU_NAME + "</a>" +
                               "</li>";
                    }
                }
                return menu;
            }

        }

        public static string buat_anak_menu(int parent = 1)
        {
            using (var db = new PortalBsnEntities())
            {
                var anakmenu = "";
                
                var hasil = db.Database.SqlQuery<SYS_MENU_PORTAL>("SELECT * FROM SYS_MENU_PORTAL WHERE MENU_PARENT_ID = "+parent + " AND MENU_POSITION = 1 AND MENU_STATUS = 1" ).ToList();
                foreach (var res in hasil)
                {
                    anakmenu += "<li><a href='" + res.MENU_URL + "' class='enclose'>" + res.MENU_NAME + "</a></li>";
                    
                }
                return anakmenu;
            }
           
        }

        public static string buat_menu_seinduk(int parent = 1)
        {
            using (var db = new PortalBsnEntities())
            {
                var anakmenu = "";
                string segment = "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["controller"] + "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["action"];
                var hasil = db.Database.SqlQuery<SYS_MENU_PORTAL>("SELECT * FROM SYS_MENU_PORTAL WHERE MENU_PARENT_ID = " + parent + " AND MENU_POSITION = 1 AND MENU_STATUS = 1 AND MENU_URL ! = '" + segment + "'").ToList();
                foreach (var res in hasil)
                {
                    anakmenu += "<li class='list-group-item clearfix'><a href='" + res.MENU_URL + "' class='enclose'><i class='fa fa-angle-right'></i>" + res.MENU_NAME+ "</a></li>";
                    
                }
                return anakmenu;
            }
           
        }

        public static string GenPassword(string input)
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

        public static string GenActivationCode(string itemToHash)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(itemToHash)).Select(s => s.ToString("x2")));
        }
        //public static string Menu()
        //{
        //    using (var db = new SISPKEntities())
        //    {
        //        int UserPrivilegeGroupId = Convert.ToInt32(HttpContext.Current.Session["UserPrivilegeGroupId"]);
        //        if (UserPrivilegeGroupId != 0)
        //        {

        //            var menu = db.Database.SqlQuery<SYS_MENU>("SELECT T1.MENU_ID, T1.MENU_PARENT_ID, T1.MENU_URL, T1.MENU_NAME, T1.MENU_SORT, T1.MENU_ICON, T1.MENU_POSITION, T1.MENU_CREATE_BY, T1.MENU_CREATE_DATE, T1.MENU_UPDATE_BY, T1.MENU_UPDATE_DATE, T1.MENU_STATUS FROM SYS_MENU T1 LEFT JOIN SYS_MENU T2 ON T2.MENU_ID = T1.MENU_PARENT_ID WHERE T1.MENU_STATUS = 1 START WITH T1.MENU_PARENT_ID = 0 CONNECT BY PRIOR T1.MENU_ID = T1.MENU_PARENT_ID ORDER SIBLINGS BY T1.MENU_SORT,T2.MENU_SORT").ToList();

        //            var hasilmenu = "";
        //            foreach (var item in menu)
        //            {
        //                if (item.MENU_PARENT_ID == 0)
        //                {
        //                    hasilmenu += item.nama_menu;
        //                }
        //                //hasilmenu += item.nama_menu;
        //            }
        //            return String.Format(hasilmenu);
        //        }
        //        else
        //        {
        //            return String.Format("<li id='parent_1' class='active'><a href='javascript:void(0)'>Please Relogin</a></li>");
        //        }



        //    }
        //}

        //public static string UserInfo(string target)
        //{
        //    using (var db = new ojk_sipaoEntities())
        //    {
        //        int UserId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
        //        var hasil = "";
        //        if (UserId != 0)
        //        {
        //            var query_UserInfo = (from a in db.Master_sysUsers
        //                                  where a.UserId == UserId
        //                                  select a).First();

        //            if (target == "EmployeeImagePath")
        //            {
        //                var employee = query_UserInfo.Master_orgEmployees;
        //                var images = employee.EmployeeImagePath;
        //                if (images != null)
        //                {
        //                    hasil = Convert.ToString(query_UserInfo.Master_orgEmployees.EmployeeImagePath);
        //                }
        //                else
        //                {
        //                    hasil = Convert.ToString("");
        //                }
        //            }

        //        } return String.Format(hasil);

        //    }
        //}
        //public static string breadcrumb(string target)
        //{
        //    using (var db = new ojk_sipaoEntities())
        //    {
        //        var url = HttpContext.Current.Request.Url.AbsolutePath;
        //        var new_url = (new StringBuilder(url)).Replace("/Create", "").Replace("/Details", "").Replace("/Edit", "").Replace("/Delete", "").ToString();


        //        string segment = "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["tipe"] + "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
        //        var query = (from a in db.ojk_fix_breadcrumb
        //                     where a.MenuUrl == segment
        //                     select new
        //                     {
        //                         MenuId = a.MenuId,
        //                         MenuUrl = a.MenuUrl,
        //                         module_id = a.module_id,
        //                         breadcrumb = a.breadcrumb,
        //                         MenuName = a.MenuName
        //                     }).First();
        //        var hasil = "";
        //        if (target == "MenuId")
        //        {
        //            hasil = Convert.ToString(query.MenuId);
        //        }
        //        else if (target == "MenuUrl")
        //        {
        //            hasil = query.MenuUrl;
        //        }
        //        else if (target == "module_id")
        //        {
        //            hasil = Convert.ToString(query.module_id);
        //        }
        //        else if (target == "breadcrumb")
        //        {
        //            hasil = query.breadcrumb;
        //        }
        //        else if (target == "MenuName")
        //        {
        //            hasil = query.MenuName;
        //        }
        //        return String.Format(hasil);
        //    }

        //}
    }
}