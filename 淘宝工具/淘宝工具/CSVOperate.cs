using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace 淘宝工具
{
    class CSVOperate
    {
        public DataTable Tdata = new DataTable();//0行开始

        private string version = "version 1.00";

        char Separate = '\t';

        private int RowCount = 0;

        private int ColumnCount = 0;

        string FilePath;

       public CSVOperate(string CSVFilePath)
        {
            CSVFilePath = @"e:\111.csv";
            FilePath = CSVFilePath;
        }

          //读取一个data的副本
        public DataTable GetCSVData()
        {
            FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            long start = Environment.TickCount;

            List<List<string>> data = GetCSVData(file);

            long end = Environment.TickCount;

            System.Diagnostics.Debug.WriteLine("读文件 {0}ms",end-start);


            start = Environment.TickCount;

            //第一列 版本号
            version = data[1][0];

            int iRowCount = data.Count;//全部数据的row
            int iColCount = data[2].Count;

            //第二列 列名  //第三列 列标题
            int iRow = 2;
            List<string> row2 = data[iRow];
            List<string> row3 = data[iRow + 1];
            for (int i = 0; i < row2.Count;i++ )
            {
                DataColumn dc = new DataColumn();
                dc.DataType = System.Type.GetType("System.String");
                dc.ColumnName = row2[i];
                dc.Caption = row3[i];
                Tdata.Columns.Add(dc);
            }       

            //第四列 数据         
            
            for ( iRow = 4; iRow < iRowCount; iRow++)
            {
                DataRow dr = Tdata.NewRow();

                for (int iCol = 0; iCol < iColCount; iCol++)
                {
                    List<string> rowS=data[iRow];
                    if (iCol < rowS.Count)
                    {
                        dr[iCol] = rowS[iCol];
                    }
                    else
                    {
                        dr[iCol] = "";
                    }                  
                }

                Tdata.Rows.Add(dr);
            }

            end = Environment.TickCount;
            System.Diagnostics.Debug.WriteLine("写table文件 {0}ms", end - start);

            RowCount = Tdata.Rows.Count;
            ColumnCount = Tdata.Columns.Count;

            file.Close();

            return Tdata;
        }

        //解析CSV为string 数组
        private List<List<string>> GetCSVData(Stream stream)
        {
            List<List<string>> data = new List<List<string>>();
            data.Add(new List<string>());//加入空行 下表从1开始

            StreamReader reader = new StreamReader(stream);
            string steamString = reader.ReadToEnd();
         
            bool inMark = false;//在分号里面

            StringBuilder item = new StringBuilder();
            List<string> line = new List<string>();

            char lastChar = ' ';

            char[] temp = steamString.ToCharArray();

            int length=temp.Length;

            for (int i = 0; i <length ; i++)
            {              
                char currentChar = (char)temp[i];               

                if (inMark)
                {
                    if (currentChar == '\"')
                    {
                        char nextChar = (char)temp[i+1];
                        if (nextChar == '\"')
                        {
                            item.Append(currentChar);
                            i++;
                        }
                        else
                        {
                            inMark = false;
                        }
                    }
                    else
                    {
                        item.Append(currentChar);
                    }                   
                }
                else
                {
                    //新item
                    if (currentChar == Separate)
                    {
                        line.Add(item.ToString());
                        item = new StringBuilder();
                    }
                    else if (currentChar == '\"')//进入引号
                    {
                        inMark = true;
                    }
                    else if (currentChar == '\r')
                    {
                       
                    }
                    else if (currentChar == '\n')
                    {
                        if (item.Length >0)
                        {
                            line.Add(item.ToString());                          
                            item = new StringBuilder();
                        }
                        data.Add(line);
                        line = new List<string>();
                    }
                    else
                    {
                        item.Append(currentChar);
                    }
                }
                lastChar=currentChar;                
            }
            return data;
        }

        //全部重写吧
        public void SetCSVData(int iRow,string Colu,string value)
        {
            try
            {
                if (Tdata.Rows[iRow][Colu] != null)
                {
                    Tdata.Rows[iRow][Colu] = value;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }           
        }

        //dataTable变成CSV
        public void PushData()
        {
            FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Write);

            CSVStreamWriter writer = new CSVStreamWriter(file,Encoding.Unicode);

            //写版本号
            writer.Write(version);
            writer.Write('\r');
            writer.Write('\n');

            //写列头
            foreach (DataColumn colu in Tdata.Columns)
            {
                writer.Write(colu.ColumnName);
                writer.Write('\t');                
            }
            writer.Write('\r');
            writer.Write('\n');

            //写名字
            foreach (DataColumn colu in Tdata.Columns)
            {
                writer.Write(colu.Caption);
                writer.Write('\t');
            }
            writer.Write('\r');
            writer.Write('\n');

            foreach (DataRow row in Tdata.Rows)
            {
                for (int i = 0; i < row.Table.Columns.Count;i++ )
                {
                    writer.Write(row[i]);
                    writer.Write('\t');
                }
                writer.Write('\r');
                writer.Write('\n');
            }

            writer.Flush();
            file.SetLength(file.Position);
            
            file.Close();
        }
    }
}
