namespace SISPK.Models
{
    using System;
    using System.Collections.Generic;

    public partial class VIEW_REKAP_SNI_BY_SK
    {
        public string SNI_SK_YEAR_NAME { get; set; }
        public decimal SNI_BARU { get; set; }
        public decimal SNI_MEREVISI { get; set; }
        public decimal SNI_DIREVISI { get; set; }
        public decimal SNI_ABOLISI { get; set; }
        public decimal DOK_BARU { get; set; }
        public decimal DOK_REVISI { get; set; }
        public decimal DOK_ABOLISI { get; set; }
    }
    public partial class VIEW_HIT_COUNTERS
    {
        public decimal COUNTERID { get; set; }
        public decimal HCTODAYLOAD { get; set; }
        public decimal HCTODAYIP { get; set; }
        public decimal HCWEEKLOAD { get; set; }
        public decimal HCWEEKIP { get; set; }
        public decimal HCMONTHLOAD { get; set; }
        public decimal HCMONTHIP { get; set; }
        public decimal HCYEARLOAD { get; set; }
        public decimal HCYEARIP { get; set; }
        public decimal HCTOTAL { get; set; }
        public decimal HCONLNE { get; set; }
    }

}
