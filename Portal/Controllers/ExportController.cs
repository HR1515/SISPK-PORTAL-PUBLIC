using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Aspose.Words;
using Portal.Models;
using Portal.Helpers;
using SISPK.Models;
using Aspose.Words.Tables;
using Aspose.Words.Drawing;

namespace Portal.Controllers
{
    public class ExportController : Controller
    {
        //private int moduleId = 14;
        private SISPKEntities db = new SISPKEntities();
        private PortalBsnEntities portaldb = new PortalBsnEntities();
        // GET: /Export/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PrintUsulanKomtek(string Type = "pdf")
        {
            
            var Data = db.Database.SqlQuery<VIEW_T_KOMTEK>("SELECT * FROM VIEW_T_KOMTEK WHERE T_KOMTEK_STATUS = 0 AND T_KOMTEK_PARENT_CODE = 0").ToList();

            Aspose.Words.Document doc = new Aspose.Words.Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            Table table = builder.StartTable();
            
            // Insert a few cells
            builder.InsertCell();
            table.PreferredWidth = PreferredWidth.FromPercent(100);
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(5);
            builder.Writeln("No");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(15);
            builder.Writeln("Kode KT");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(20);
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            builder.Writeln("Nama KT");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(20);
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            builder.Writeln("Sekretariat");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(25);
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            builder.Writeln("Alamat");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(15);
            builder.Writeln("Telepon/Fax");
            builder.EndRow();

            int no = 1 ;

            foreach (var list in Data) {
                builder.InsertCell();
                builder.Writeln(""+ no++);

                builder.InsertCell();
                builder.Writeln(""+list.T_KOMTEK_CODE);

                builder.InsertCell();
                builder.Writeln(""+ list.T_KOMTEK_NAME);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_SEKRETARIAT);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_ADDRESS);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_PHONE );
                builder.EndRow();
            }

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=Usulan_Komite_Teknis." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }       

        public ActionResult PrintUsulanSubKomtek(string Type = "docx")
        {
            var Data = db.Database.SqlQuery<VIEW_T_KOMTEK>("SELECT * FROM VIEW_T_KOMTEK WHERE T_KOMTEK_STATUS = 0 AND T_KOMTEK_PARENT_CODE != 0").ToList();

            Aspose.Words.Document doc = new Aspose.Words.Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            Table table = builder.StartTable();

            // Insert a few cells
            builder.InsertCell();
            table.PreferredWidth = PreferredWidth.FromPercent(100);
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(5);
            builder.Writeln("No");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(10);
            builder.Writeln("Kode KT");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(20);
            builder.Writeln("Nama KT");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(20);
            builder.Writeln("Sekretariat");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(30);
            builder.Writeln("Alamat");

            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPercent(15);
            builder.Writeln("Telepon/Fax");
            builder.EndRow();

            int no = 1;

            foreach (var list in Data)
            {
                builder.InsertCell();
                builder.Writeln("" + no++);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_CODE);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_NAME);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_SEKRETARIAT);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_ADDRESS);

                builder.InsertCell();
                builder.Writeln("" + list.T_KOMTEK_PHONE);
                builder.EndRow();
            }

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=Usulan_Komite_Teknis." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }
       
        public ActionResult PrintKomtek(string Type = "docx")
        {
            var Data = db.Database.SqlQuery<VIEW_KOMTEK>("SELECT * FROM VIEW_KOMTEK WHERE KOMTEK_STATUS = 1 ORDER BY KOMTEK_CODE ASC").ToList();

            //Aspose.Words.Document doc = new Aspose.Words.Document();
            //DocumentBuilder builder = new DocumentBuilder(doc);

            string dataDir = Server.MapPath("~/Format/");
            Stream stream = System.IO.File.OpenRead(dataDir + "komite_teknis_list.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);            

            DocumentBuilder builder = new DocumentBuilder(doc);
            Aspose.Words.Font font = builder.Font;
            font.Bold = false;
            font.Color = System.Drawing.Color.Black;
            font.Italic = false;
            font.Name = "Calibri";
            font.Size = 11;
            builder.MoveToDocumentEnd();
            //var DataKomtek = (from komtek in db.VIEW_ANGGOTA where komtek.KOMTEK_ANGGOTA_STATUS == 1 && komtek.KOMTEK_ANGGOTA_KOMTEK_ID == Data.KOMTEK_ID orderby komtek.KOMTEK_ANGGOTA_JABATAN select komtek).ToList();
            var no = 0;


            Table myTable = (Table)doc.GetChild(NodeType.Table, 0, true);
            Row myRow = myTable.LastRow;
            foreach (var list in Data)
            {
                no++;
                Row newRow = (Row)myRow.Clone(true);
                myTable.AppendChild(newRow);
                foreach (Cell cell in newRow)
                {
                    cell.FirstParagraph.ChildNodes.Clear();
                }
                builder.MoveToCell(0, no, 0, 0);
                builder.Write("" + no);
                builder.MoveToCell(0, no, 1, 0);
                builder.Write("" + list.KOMTEK_CODE);
                builder.MoveToCell(0, no, 2, 0);
                builder.Write("" + list.KOMTEK_NAME);
                builder.MoveToCell(0, no, 3, 0);
                builder.Write("" + list.KOMTEK_SEKRETARIAT);
                builder.MoveToCell(0, no, 4, 0);
                builder.Write("" + list.KOMTEK_ADDRESS);
                builder.MoveToCell(0, no, 5, 0);
                builder.Write("" + ((list.KOMTEK_PHONE != null) ? list.KOMTEK_PHONE : list.KOMTEK_FAX));
            }
            Row LastROW = myTable.LastRow;
            LastROW.Remove();

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=Komite_Teknis_List." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }

        public ActionResult PrintDetailKomtek(string Type = "pdf", int id = 0)
        {
            var DataPantek = (from pantek in db.MASTER_KOMITE_TEKNIS where pantek.KOMTEK_ID == id select pantek).SingleOrDefault();
          
            var DataIcs = (from ics in db.VIEW_KOMTEK_ICS where ics.KOMTEK_ICS_KOMTEK_ID == id orderby ics.ICS_CODE ascending select ics).ToList();

            var DataAnggota = (from ics in db.VIEW_ANGGOTA where ics.KOMTEK_ANGGOTA_KOMTEK_ID == id && ics.KOMTEK_ANGGOTA_STATUS == 1 && ics.JABATAN != "Sekretariat" orderby ics.KOMTEK_ANGGOTA_JABATAN ascending select ics).ToList();

            var komposis_anggota = db.Database.SqlQuery<KomposisiKomtek>("SELECT CAST(VA.KOMTEK_ANGGOTA_STAKEHOLDER AS DECIMAL) AS KOMTEK_ANGGOTA_STAKEHOLDER,VA.STAKEHOLDER,CAST(VA.KOMTEK_ANGGOTA_KOMTEK_ID AS DECIMAL) AS KOMTEK_ANGGOTA_KOMTEK_ID, CAST(COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER) AS DECIMAL) AS JML, CAST(ROUND((COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER)/(SELECT COUNT(VAS.KOMTEK_ANGGOTA_STAKEHOLDER) FROM VIEW_ANGGOTA VAS WHERE VAS.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " AND VAS.JABATAN != 'Sekretariat')* 100),2) AS DECIMAL) AS PERSENTASE FROM VIEW_ANGGOTA VA WHERE VA.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " AND VA.JABATAN != 'Sekretariat' AND VA.KOMTEK_ANGGOTA_STAKEHOLDER IS NOT NULL GROUP BY VA.KOMTEK_ANGGOTA_STAKEHOLDER, VA.STAKEHOLDER, VA.KOMTEK_ANGGOTA_KOMTEK_ID").ToList();
          
            //var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM KOMTE WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            string dataDir = Server.MapPath("~/Format/");
            Stream stream = System.IO.File.OpenRead(dataDir + "Komite_teknis.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            if (DataPantek != null)
            {
                //var DataKomtek = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + Data.KOMTEK_ID + " AND JABATAN = 'Ketua'").SingleOrDefault();
                doc.Range.Replace("a1", DataPantek.KOMTEK_CODE +"-"+ DataPantek.KOMTEK_NAME, false, true);
                doc.Range.Replace("a2", (DataPantek.KOMTEK_SEKRETARIAT == null) ? "-" : DataPantek.KOMTEK_SEKRETARIAT, false, true);
                doc.Range.Replace("a3", (DataPantek.KOMTEK_ADDRESS == null) ? "-" : DataPantek.KOMTEK_ADDRESS, false, true);
                doc.Range.Replace("a4", (DataPantek.KOMTEK_PHONE == null) ? "-" : DataPantek.KOMTEK_PHONE, false, true);
                doc.Range.Replace("a5", (DataPantek.KOMTEK_FAX == null) ? "-" : DataPantek.KOMTEK_FAX, false, true);
                doc.Range.Replace("a6", (DataPantek.KOMTEK_EMAIL == null) ? "-" : DataPantek.KOMTEK_EMAIL, false, true);
                doc.Range.Replace("a7", (DataPantek.KOMTEK_CONTACT_PERSON == null) ? "-" : DataPantek.KOMTEK_CONTACT_PERSON, false, true);
                doc.Range.Replace("a8", (DataPantek.KOMTEK_STATUS == 1 )?"Aktif":"Tidak Aktif", false, true);
                doc.Range.Replace("a9", (DataPantek.KOMTEK_KETERANGAN == null) ? "-" : DataPantek.KOMTEK_KETERANGAN, false, true);                

            }
                                  

            DocumentBuilder builder = new DocumentBuilder(doc);
            Aspose.Words.Font font = builder.Font;
            font.Bold = false;
            font.Color = System.Drawing.Color.Black;
            font.Italic = false;
            font.Name = "Calibri";
            font.Size = 11;
            builder.MoveToDocumentEnd();
            //var DataKomtek = (from komtek in db.VIEW_ANGGOTA where komtek.KOMTEK_ANGGOTA_STATUS == 1 && komtek.KOMTEK_ANGGOTA_KOMTEK_ID == Data.KOMTEK_ID orderby komtek.KOMTEK_ANGGOTA_JABATAN select komtek).ToList();
            var no = 0;


            Table myTable = (Table)doc.GetChild(NodeType.Table, 1, true);
            Row myRow = myTable.LastRow;
            foreach (var list in DataIcs)
            {
                no++;
                Row newRow = (Row)myRow.Clone(true);
                myTable.AppendChild(newRow);
                foreach (Cell cell in newRow)
                {
                    cell.FirstParagraph.ChildNodes.Clear();
                }
                builder.MoveToCell(1, no, 0, 0);
                builder.Write("" + no);
                builder.MoveToCell(1, no, 1, 0);
                builder.Write("" + list.ICS_CODE);
                builder.MoveToCell(1, no, 2, 0);
                builder.Write("" + list.ICS_NAME_IND);
                builder.MoveToCell(1, no, 3, 0);
                builder.Write("" + list.ICS_NAME);
            }
            Row LastROW = myTable.LastRow;
            LastROW.Remove();


            //DocumentBuilder builders = new DocumentBuilder(doc);
            //Aspose.Words.Font fonts = builders.Font;
            //font.Bold = false;
            //font.Color = System.Drawing.Color.Black;
            //font.Italic = false;
            //font.Name = "Calibri";
            //font.Size = 11;
            //builders.MoveToDocumentEnd();

            var nos = 0;

            Table table2 = (Table)doc.GetChild(NodeType.Table, 2, true);
            Row myRow2 = table2.LastRow;
            foreach (var list in DataAnggota)
            {
                nos++;
                Row newRow2 = (Row)myRow2.Clone(true);
                table2.AppendChild(newRow2);
                foreach (Cell cell2 in newRow2)
                {
                    cell2.FirstParagraph.ChildNodes.Clear();
                }
                builder.MoveToCell(2, nos, 0, 0);
                builder.Write("" + nos);
                builder.MoveToCell(2, nos, 1, 0);
                builder.Write("" + list.JABATAN);
                builder.MoveToCell(2, nos, 2, 0);
                builder.Write("" + list.STAKEHOLDER);
                builder.MoveToCell(2, nos, 3, 0);
                builder.Write("" + list.KOMTEK_ANGGOTA_NAMA);
                builder.MoveToCell(2, nos, 4, 0);
                builder.Write("" + list.KOMTEK_ANGGOTA_INSTANSI);
                builder.MoveToCell(2, nos, 5, 0);
                builder.Write("" + list.KOMTEK_ANGGOTA_ADDRESS);
            }
            Row LastROW2 = table2.LastRow;
            LastROW2.Remove();

           
            //DocumentBuilder builderss = new DocumentBuilder(doc);
            //Aspose.Words.Font fontss = builderss.Font;
            //font.Bold = false;
            //font.Color = System.Drawing.Color.Black;
            //font.Italic = false;
            //font.Name = "Calibri";
            //font.Size = 11;
            //builder.MoveToDocumentEnd();
            int noss = 1;
            int JumlahJamleh = 0;
            Table table3 = (Table)doc.GetChild(NodeType.Table, 3, true);
            Row myRow3 = table3.LastRow;
            foreach (var list in komposis_anggota)
            {
                noss++;
                Row newRow3 = (Row)myRow3.Clone(true);
                table3.AppendChild(newRow3);
                foreach (Cell cell in newRow3)
                {
                    cell.FirstParagraph.ChildNodes.Clear();
                }
                builder.MoveToCell(3, noss, 0, 0);
                builder.Write((noss-1).ToString());
                builder.MoveToCell(3, noss, 1, 0);
                builder.Write("" + list.STAKEHOLDER);
                builder.MoveToCell(3, noss, 2, 0);
                builder.Write("" + list.JML);
                builder.MoveToCell(3, noss, 3, 0);
                builder.Write("" + list.PERSENTASE +" %");

                JumlahJamleh += Convert.ToInt32(list.JML);
            }
            
            noss++;
            builder.MoveToCell(3, noss, 0, 0);
            builder.CellFormat.HorizontalMerge = CellMerge.First;
            builder.Write("Jumlah");
            builder.MoveToCell(3, noss, 1, 0);
            builder.CellFormat.HorizontalMerge = CellMerge.Previous;
            builder.MoveToCell(3, noss, 2, 0);
            builder.Write("" + JumlahJamleh);
            builder.MoveToCell(3, noss, 3, 0);
            builder.Write("" + 100);

            


            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=KOMITE_TEKNIS"+DataPantek.KOMTEK_CODE+"." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }

        public ActionResult PrintSubKomtek(string Type = "docx")
        {
            var Data = db.Database.SqlQuery<VIEW_SUBKOMTEK>("SELECT * FROM VIEW_SUBKOMTEK WHERE KOMTEK_STATUS = 1").ToList();

            string dataDir = Server.MapPath("~/Format/");
            Stream stream = System.IO.File.OpenRead(dataDir + "komite_teknis_list.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            doc.Range.Replace("KOMITE TEKNIS","SUB KOMITE TEKNIS", false, true);
  
            

            DocumentBuilder builder = new DocumentBuilder(doc);
            Aspose.Words.Font font = builder.Font;
            font.Bold = false;
            font.Color = System.Drawing.Color.Black;
            font.Italic = false;
            font.Name = "Calibri";
            font.Size = 11;
            builder.MoveToDocumentEnd();
            //var DataKomtek = (from komtek in db.VIEW_ANGGOTA where komtek.KOMTEK_ANGGOTA_STATUS == 1 && komtek.KOMTEK_ANGGOTA_KOMTEK_ID == Data.KOMTEK_ID orderby komtek.KOMTEK_ANGGOTA_JABATAN select komtek).ToList();
            var no = 0;


            Table myTable = (Table)doc.GetChild(NodeType.Table, 0, true);
            Row myRow = myTable.LastRow;
            foreach (var list in Data)
            {
                no++;
                Row newRow = (Row)myRow.Clone(true);
                myTable.AppendChild(newRow);
                foreach (Cell cell in newRow)
                {
                    cell.FirstParagraph.ChildNodes.Clear();
                }
                builder.MoveToCell(0, no, 0, 0);
                builder.Write("" + no);
                builder.MoveToCell(0, no, 1, 0);
                builder.Write("" + list.KOMTEK_CODE);
                builder.MoveToCell(0, no, 2, 0);
                builder.Write("" + list.SUB_KOMTEK);
                builder.MoveToCell(0, no, 3, 0);
                builder.Write("" + list.KOMTEK_SEKRETARIAT);
                builder.MoveToCell(0, no, 4, 0);
                builder.Write("" + list.KOMTEK_ADDRESS);
                builder.MoveToCell(0, no, 5, 0);
                builder.Write("" + ((list.KOMTEK_PHONE != null) ? list.KOMTEK_PHONE : list.KOMTEK_FAX));
            }
            Row LastROW = myTable.LastRow;
            LastROW.Remove();


            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=sub_Komite_Teknis." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }

        public ActionResult PrintDetailSubKomtek(string Type = "docx")
        {
            Aspose.Words.Document doc = new Aspose.Words.Document();

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=SubKomite_Teknis_." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }

        public string ConvertTanggal(DateTime tanggal, string tipe = "")
        {
            var res = "";
            var AngkaBulan = tanggal.ToString("MM");
            var NamaHariEng = tanggal.ToString("dddd");
            var AngkaHari = tanggal.ToString("dd");
            var Tahun = tanggal.ToString("yyyy");
            var Bulan = "";
            var Hari = "";
            switch (NamaHariEng)
            {
                case "Sunday":
                    Hari = "Minggu";
                    break;
                case "Monday":
                    Hari = "Senin";
                    break;
                case "Tuesday":
                    Hari = "Selasa";
                    break;
                case "Wednesday":
                    Hari = "Rabu";
                    break;
                case "Thursday":
                    Hari = "Kamis";
                    break;
                case "Friday":
                    Hari = "Jumat";
                    break;
                case "Saturday":
                    Hari = "Sabtu";
                    break;
                default:
                    return "";
            }
            switch (AngkaBulan)
            {
                case "01":
                    Bulan = "Januari";
                    break;
                case "02":
                    Bulan = "Februari";
                    break;
                case "03":
                    Bulan = "Maret";
                    break;
                case "04":
                    Bulan = "April";
                    break;
                case "05":
                    Bulan = "Mei";
                    break;
                case "06":
                    Bulan = "Juni";
                    break;
                case "07":
                    Bulan = "Juli";
                    break;
                case "08":
                    Bulan = "Agustus";
                    break;
                case "09":
                    Bulan = "September";
                    break;
                case "10":
                    Bulan = "Oktober";
                    break;
                case "11":
                    Bulan = "November";
                    break;
                case "12":
                    Bulan = "Desember";
                    break;
                default:
                    return "";
            }
            if (tipe == "full")
            {
                res = AngkaHari + " " + Bulan + " " + Tahun;
            }
            else if (tipe == "namabulan")
            {
                res = Bulan;
            }
            else if (tipe == "angkabulan")
            {
                res = AngkaBulan;
            }
            else if (tipe == "tahun")
            {
                res = Tahun;
            }
            else if (tipe == "namahari")
            {
                res = Hari;
            }
            else if (tipe == "angkahari")
            {
                res = AngkaHari;
            }
            return res;
        }

    }
}
