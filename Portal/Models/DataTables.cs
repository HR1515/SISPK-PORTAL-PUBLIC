using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SISPK.Models
{
    public class DataTables
    {
        public string iSortCol_0 { get; set; }
        public string sSortDir_0 { get; set; }
        public string callback { get; set; }
        /// <summary>
        /// Request sequence number sent by DataTable,
        /// same value must be returned in response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Text used for filtering
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Number of records that should be shown in table
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// First record that should be shown(used for paging)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Number of columns in table
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortingCols { get; set; }


        /// <summary>
        /// Comma separated list of column names
        /// </summary>
        public string sColumns { get; set; }
    }
    public class KomposisiKomtek
    {
        public Nullable<decimal> KOMTEK_ANGGOTA_STAKEHOLDER { get; set; }
        public string STAKEHOLDER { get; set; }
        public Nullable<decimal> KOMTEK_ANGGOTA_KOMTEK_ID { get; set; }
        public Nullable<decimal> JML { get; set; }
        public Nullable<decimal> PERSENTASE { get; set; }
    }
    //public class VIEW_POLLING
    //{
    //    public Nullable<decimal> POLLING_PROPOSAL_ID { get; set; }
    //    public Nullable<decimal> POLLING_TYPE { get; set; }
    //    public Nullable<decimal> SETUJU_PERSEN { get; set; }
    //    public Nullable<decimal> TIDAK_SETUJU_PERSEN { get; set; }
    //    public Nullable<decimal> JML_SETUJU { get; set; }
    //    public Nullable<decimal> JML_TIDAK_SETUJU { get; set; }
    //}
}