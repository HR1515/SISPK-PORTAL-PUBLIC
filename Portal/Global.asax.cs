using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Portal.Models;
using SISPK.Models;

namespace Portal
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private SISPKEntities db = new SISPKEntities();
        protected void Application_Start()
        {
            var Key = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 11").FirstOrDefault();
            Aspose.Words.License wordsLicense = new Aspose.Words.License();

            wordsLicense.SetLicense(@"" + Key.CONFIG_VALUE);


            Aspose.Pdf.License pdfLicense = new Aspose.Pdf.License();

            pdfLicense.SetLicense(@"" + Key.CONFIG_VALUE);


            //AreaRegistration.RegisterAllAreas();
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);


        }
        //protected void Application_Start()
        //{
        //    AreaRegistration.RegisterAllAreas();

        //    WebApiConfig.Register(GlobalConfiguration.Configuration);
        //    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        //    RouteConfig.RegisterRoutes(RouteTable.Routes);
        //    BundleConfig.RegisterBundles(BundleTable.Bundles);
        //    AuthConfig.RegisterAuth();
        //}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string VisitorsIPAddr = string.Empty;
            if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                VisitorsIPAddr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
            {
                VisitorsIPAddr = HttpContext.Current.Request.UserHostAddress;
            }

            string ip = HttpContext.Current.Request.UserHostAddress;
            string url = Request.Url.ToString();
            string useragent = Request.Headers["User-Agent"];
            int HCid = db.Database.SqlQuery<int>("SELECT SEQ_SYS_HIT_COUNTERS.NEXTVAL FROM DUAL").SingleOrDefault();
            var fname = "HC_ID," +
                        "HC_IP," +
                        "HC_LINK," +
                        "HC_USER_AGENT," +
                        "HC_DATE," +
                        "HC_STATUS";
            var fvalue = "'" + HCid + "', " +
                        "'" + VisitorsIPAddr + "', " +
                        "'" + url.Replace("'", "") + "', " +
                        "'" + useragent + "', " +
                        "SYSDATE, " +
                        "'1'";           
            db.Database.ExecuteSqlCommand("INSERT INTO SYS_HIT_COUNTERS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
        }

    }
}