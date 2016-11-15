using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Portal.Models;
using System.Text;

namespace Portal.Helpers
{
    public class MixHelper
    {
        private SISPKEntities db = new SISPKEntities();

        public static String GetLogCode()
        {
            using (var db = new SISPKEntities())
            {
                string LogId = "";
                string InitialCode = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                int NextNumber = 1;

                var GetLast = db.Database.SqlQuery<String>("SELECT MAX(LOG_CODE) AS CODE FROM SYS_LOGS WHERE LOG_CODE LIKE '" + InitialCode + "%'").SingleOrDefault();
                if (GetLast != null)
                {
                    LogId = GetLast.Replace(InitialCode, "");
                    int.TryParse(LogId, out NextNumber);
                    NextNumber = NextNumber + 1;
                    LogId = "";
                }
                LogId = InitialCode + NextNumber.ToString().PadLeft(8, '0');
                return LogId;
            }
        }
        public static String InsertLog(String code, String objek, int action)
        {
            using (var db = new SISPKEntities())
            {
                int id = db.Database.SqlQuery<int>("SELECT SEQ_SYS_LOGS.NEXTVAL FROM DUAL").SingleOrDefault();
                string UserId = HttpContext.Current.Session["USER_ID"].ToString();
                string AccesId = HttpContext.Current.Session["USER_ACCESS_ID"].ToString();
                var fname = "LOG_ID, LOG_CODE, LOG_USER, LOG_USER_TYPE, LOG_ACTION, LOG_OBJECT, LOG_DATE";
                var fvalue = "'" + id + "', " +
                             "'" + code + "', " +
                             "'" + UserId + "', " +
                             "'" + AccesId + "', " +
                             "'" + action + "', " +
                             "'" + objek + "', " +
                             "SYSDATE";
                db.Database.ExecuteSqlCommand("INSERT INTO SYS_LOGS (" + fname + ") VALUES (" + fvalue + ")");

                return "1";
            }
        }

        public static String InsertLogReg(String code, String objek, int action)
        {
            using (var db = new SISPKEntities())
            {
                int id = db.Database.SqlQuery<int>("SELECT SEQ_SYS_LOGS.NEXTVAL FROM DUAL").SingleOrDefault();
                string UserId = "0";
                string AccesId = "0";
                var fname = "LOG_ID, LOG_CODE, LOG_USER, LOG_USER_TYPE, LOG_ACTION, LOG_OBJECT, LOG_DATE";
                var fvalue = "'" + id + "', " +
                             "'" + code + "', " +
                             "'" + UserId + "', " +
                             "'" + AccesId + "', " +
                             "'" + action + "', " +
                             "'" + objek + "', " +
                             "SYSDATE";
                db.Database.ExecuteSqlCommand("INSERT INTO SYS_LOGS (" + fname + ") VALUES (" + fvalue + ")");

                return "1";

            }
        }

        public static String InsertLogActivate(String code, int userid, int accessid, String objek, int action)
        {
            using (var db = new SISPKEntities())
            {
                int id = db.Database.SqlQuery<int>("SELECT SEQ_SYS_LOGS.NEXTVAL FROM DUAL").SingleOrDefault();
                var fname = "LOG_ID, LOG_CODE, LOG_USER, LOG_USER_TYPE, LOG_ACTION, LOG_OBJECT, LOG_DATE";
                var fvalue = "'" + id + "', " +
                             "'" + code + "', " +
                             "'" + userid + "', " +
                             "'" + accessid + "', " +
                             "'" + action + "', " +
                             "'" + objek + "', " +
                             "SYSDATE";
                db.Database.ExecuteSqlCommand("INSERT INTO SYS_LOGS (" + fname + ") VALUES (" + fvalue + ")");

                return "2";

            }
        }

        public static int GetSequence(String _table)
        {
            using (var db = new SISPKEntities())
            {
                int id = db.Database.SqlQuery<int>("SELECT SEQ_" + _table + ".NEXTVAL FROM DUAL").SingleOrDefault();
                return id;
            }
        }

        public static String ConvertDateNow()
        {
            String datenow = "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            return datenow;
        }

        public static String TimeAgo(DateTime date)
        {
            TimeSpan timeSince = DateTime.Now.Subtract(date);
            if (timeSince.TotalMilliseconds < 1) return "not yet";
            if (timeSince.TotalMinutes < 1) return "just now";
            if (timeSince.TotalMinutes < 2) return "1 minute ago";
            if (timeSince.TotalMinutes < 60) return string.Format("{0} minutes ago", timeSince.Minutes);
            if (timeSince.TotalMinutes < 120) return "1 hour ago";
            if (timeSince.TotalHours < 24) return string.Format("{0} hours ago", timeSince.Hours);
            if (timeSince.TotalDays < 2) return "yesterday";
            if (timeSince.TotalDays < 7) return string.Format("{0} days ago", timeSince.Days);
            if (timeSince.TotalDays < 14) return "last week";
            if (timeSince.TotalDays < 21) return "2 weeks ago";
            if (timeSince.TotalDays < 28) return "3 weeks ago";
            if (timeSince.TotalDays < 60) return "last month";
            if (timeSince.TotalDays < 365) return string.Format("{0} months ago", Math.Round(timeSince.TotalDays / 30));
            if (timeSince.TotalDays < 730) return "last year"; //last but not least...
            return string.Format("{0} years ago", Math.Round(timeSince.TotalDays / 365));
        }

    }
}