using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using System.Threading;
using System.Windows.Forms;


namespace 淘宝工具
{
    public class ExcelOperate
    {
        private Stopwatch wath = new Stopwatch();

        private Excel.Application app = new Excel.Application();

        Excel.Workbook workbook = null;

        System.Data.DataTable dt = new System.Data.DataTable();

        //读取一个data的副本
        public System.Data.DataTable GetExcelData(string excelFilePath)
        {
            excelFilePath = @"e:\88.csv";
         
            object oMissiong = System.Reflection.Missing.Value;
           
            wath.Start();

            if (app == null)
            {
                return null;
            }
            try
            {
                workbook = app.Workbooks.Add(excelFilePath);

                //将数据读入到DataTable中——Start
                Excel.Sheets sheets = workbook.Worksheets;
                Excel.Worksheet worksheet = (Excel.Worksheet)sheets.get_Item(1);//读取第一张表
                if (worksheet == null)
                    return null;

                string cellContent;
                int iRowCount = worksheet.UsedRange.Rows.Count;
                int iColCount = worksheet.UsedRange.Columns.Count;
                Excel.Range range;

                //第一行 版本
                DataColumn dc;

                int iRow = 1;
                int iCol = 1;
                range = (Excel.Range)worksheet.Cells[1, 1];


                //第二列 是列头
                iRow = 2;
                range = (Excel.Range)worksheet.Cells[iRow, iCol];
                while (range.Text.ToString().Trim() != "")
                {
                    dc = new DataColumn();
                    dc.DataType = System.Type.GetType("System.String");
                    dc.ColumnName = range.Text.ToString().Trim();
                    dt.Columns.Add(dc);
                    range = (Excel.Range)worksheet.Cells[iRow, ++iCol];
                }
                
                //End

                for (iRow = 4; iRow <= iRowCount; iRow++)
                {
                    DataRow dr = dt.NewRow();

                    for (iCol = 1; iCol <= iColCount; iCol++)
                    {
                        range = (Excel.Range)worksheet.Cells[iRow, iCol];

                        cellContent = (range.Value2 == null) ? "" : range.Value2.ToString();                      

                        dr[iCol - 1] = cellContent;
                    }

                    dt.Rows.Add(dr);
                }

                wath.Stop();
                TimeSpan ts = wath.Elapsed;

                //将数据读入到DataTable中——End

                return dt;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
            finally
            {
                workbook.Close(false, oMissiong, oMissiong);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                workbook = null;
                app.Workbooks.Close();
                app.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
                app = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void SetExcelData(int rowCount,int coluCount,string value)
        {
            //修改excel
            Excel.Sheets sheets = workbook.Worksheets;
            Excel.Worksheet worksheet = (Excel.Worksheet)sheets.get_Item(1);//读取第一张表

             rowCount += 3;
             //coluCount = ;

            Excel.Range cell = (Excel.Range)worksheet.Cells[rowCount, coluCount];
            cell.Value2 = value;
        }   

    }
}
