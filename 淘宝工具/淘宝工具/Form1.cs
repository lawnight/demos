using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

using AP = HtmlAgilityPack;
using System.Net;
using System.IO;


namespace 淘宝工具
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DataTable dataList;
        
        public void ShowDataInListView(DataTable dt, ListView lst)
        {
            lst.Clear();
            dataList = dt;
            lst.AllowColumnReorder = true;//用户可以调整列的位置
            listView1.GridLines = true;//表格是否显示网格线
            listView1.FullRowSelect = true;//是否选中整行
            listView1.View = View.Details;//设置显示方式
            listView1.Scrollable = true;//是否自动显示滚动条

            int RowCount, ColCount, i, j;
            DataRow dr = null;

            if (dt == null) return;
            RowCount = dt.Rows.Count;
            ColCount = dt.Columns.Count;

            //添加index
            lst.Columns.Add("index", 20, HorizontalAlignment.Left);
            //添加列标题名
            for (i = 0; i < ColCount; i++)
            {
                lst.Columns.Add(dt.Columns[i].Caption.Trim(), 100, HorizontalAlignment.Left);
            }
           

            //添加内容
            for (i = 0; i < RowCount; i++)
            {
                dr = dt.Rows[i];
                ListViewItem item = new ListViewItem();
                item.Text = (i + 1).ToString();
                for (j = 0; j < ColCount; j++)
                {
                    item.SubItems.Add((string)dr[j].ToString().Trim());
                }
                lst.Items.Add(item);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ExcelOperate excel = new ExcelOperate();
            DataTable dt = excel.GetExcelData(null);
            //显示
            ShowDataInListView(dt, this.listView1);
            return;
            
            //分析
            int rowCount = 0;
            foreach (DataRow row in dt.Rows)
            {
                rowCount++;
                string des = (String)row["description"];
                string cid = (String)row["cid"];
                string title = (String)row["title"];

                AP.HtmlDocument doc = new AP.HtmlDocument();
                doc.LoadHtml(des);

                List<string> imgList = new List<string>();

                string root= @"D:\";

                if (title.Length < 6)
                {
                    continue;
                }

                string path = root+title.Substring(title.Length - 6, 5);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);


                int imgCount=0;
                //循环下载图片
                foreach (AP.HtmlNode item in doc.DocumentNode.SelectNodes("//img"))
                {
                    HtmlAgilityPack.HtmlAttribute att = item.Attributes["src"];
                    imgList.Add(att.Value);
                    imgCount++;

                    //文件名

                    string name = string.Format("{0}-{1}.jpg",rowCount,imgCount);
                    string fileName = path +@"\"+ name;
                    //下载图片
                    WebRequest request = WebRequest.Create(att.Value);
                    WebResponse response = request.GetResponse();
                    Stream imgStream = response.GetResponseStream();

                    FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(file);

                    int length =(int)response.ContentLength;
                  
                    byte[] buff = new byte[length];
                    imgStream.Read(buff, 0, length);
                    writer.Write(buff);
                    file.Write(buff, 0, length);

                    //修改html
                    att.Value = fileName;                    

                    //释放资源
                    writer.Close();
                    writer.Dispose();
                    imgStream.Close();
                    imgStream.Dispose();
                    response.Close();
                }

                string alterhtml = doc.DocumentNode.OuterHtml;
                webBrowser1.DocumentText = alterhtml;//string.Format("<html><body>{0}</body></html>", des);
                //修改excel
                int ColuIndex= row.Table.Columns["description"].Ordinal;

                excel.SetExcelData(rowCount, ColuIndex, alterhtml);


               // MessageBox.Show(string.Format("img count {0}", imgList.Count));
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (listView1.SelectedItems.Count > 0)
            //{
            //    ListViewItem item = listView1.SelectedItems[0];
            //    int rowCount = item.Index + 1;
            //    DataRow row = dataList.Rows[rowCount];
            //    string des = (string)row["description"];
            //    ShowDetail form = new ShowDetail();
            //    form.SetString(des);
            //    form.Show();              
            //}
          
        }
    }


}
