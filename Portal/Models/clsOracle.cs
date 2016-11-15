using System;
using Oracle;
namespace Portal.Models
{
    public class clsOracle
    {
        private System.Data.OracleClient.OracleConnection connOracle;
        private System.Data.OracleClient.OracleDataReader rstOracle;
        private System.Data.OracleClient.OracleCommand sqlCommandOracle;
        private System.Data.OracleClient.OracleTransaction txn;
        private System.Data.OracleClient.OracleLob clob;

        public clsOracle()
        {
            string p_conn_db = "Data Source=127.0.0.1/ORCL;User ID=C##BSN_SISPK;PASSWORD=Bismillah;";
            connOracle = new System.Data.OracleClient.OracleConnection(p_conn_db);
            connOracle.Open();
        }

        public void InsertRecord(string SQLStatement)
        {
            if (SQLStatement.Length > 0)
            {
                if (connOracle.State.ToString().Equals("Open"))
                {
                    sqlCommandOracle =
                      new System.Data.OracleClient.OracleCommand(SQLStatement, connOracle);
                    sqlCommandOracle.ExecuteScalar();
                }
            }
        }

        public void InsertCLOB(string SQLStatement, string str)
        {
            if (SQLStatement.Length > 0)
            {
                if (connOracle.State.ToString().Equals("Open"))
                {
                    byte[] newvalue = System.Text.Encoding.Unicode.GetBytes(str);
                    sqlCommandOracle =
                      new System.Data.OracleClient.OracleCommand(SQLStatement, connOracle);
                    rstOracle = sqlCommandOracle.ExecuteReader();
                    rstOracle.Read();
                    txn = connOracle.BeginTransaction();
                    clob = rstOracle.GetOracleLob(0);
                    clob.Write(newvalue, 0, newvalue.Length);
                    txn.Commit();
                }
            }
        }
        public void CloseDatabase()
        {
            connOracle.Close();
            connOracle.Dispose();
        }   

    }
}
