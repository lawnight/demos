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
using System.Threading;


namespace 淘宝工具
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        DataTable dataList;

        Dictionary<string, string> ImgList = new Dictionary<string, string>();

        
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
            lst.Columns.Add("Id", 30, HorizontalAlignment.Left);
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

        private void WorkThread(object par)
        {
            DataTable dt = (DataTable)par;
            if (dt == null)
            {
                MessageBox.Show("请先读取数据");
                return;
            }
            //分析
            int rowCount = 0;

           

            foreach (DataRow row in dt.Rows)
            {
                
                string des = (String)row["description"];
                string cid = (String)row["cid"];
                string title = (String)row["title"];

                AP.HtmlDocument doc = new AP.HtmlDocument();
                doc.LoadHtml(des);               

                string root = @"D:\";

                if (title.Length < 6)
                {
                    continue;
                }

                string path = root;
                //string path = root + title.Substring(title.Length - 6, 5);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                int imgCount = 0;
                //得到需要下载图片
                foreach (AP.HtmlNode item in doc.DocumentNode.SelectNodes("//img"))
                {
                    HtmlAgilityPack.HtmlAttribute att = item.Attributes["src"];

                    if (att == null)
                    {
                        continue;
                    }
                    //downUrl
                    string DownUrl = att.Value;
                    //文件名
                    string name = string.Format("{0}-{1}.jpg", rowCount, imgCount);
                    string fileName = path + @"\" + name;
                    imgCount++;
                    ImgList.Add(fileName,att.Value);                   
                    //修改Url
                    att.Value = fileName;
                }
                string alterhtml = doc.DocumentNode.OuterHtml;
                //webBrowser1.DocumentText = alterhtml;
                CSVFile.SetCSVData(rowCount, "description", alterhtml);
                rowCount++;
            }
            //修改CSV
            CSVFile.PushData();
            //开始下载图片
            //DownImg();
        }    

        private void DownImg()
        {
            progressBar1.Maximum = ImgList.Count;
            progressBar1.Value = 0;
            foreach ( var item in ImgList)
            {               
                string fileName = item.Key;
                string  url= item.Value;
                WebClient webclient = new WebClient();
                webclient.DownloadFile(url, fileName);//修改成异步的
                progressBar1.Value++;
                /*
                WebRequest request = WebRequest.Create(url);                
                WebResponse response = request.GetResponse();                
                Stream imgStream = response.GetResponseStream();

                FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter writer = new StreamWriter(file);

                int length = (int)response.ContentLength;

                byte[] buff = new byte[length];
                imgStream.Read(buff, 0, length);
                writer.Write(buff);
                file.Write(buff, 0, length);
                //释放资源
                writer.Close();
                writer.Dispose();
                imgStream.Close();
                imgStream.Dispose();
                response.Close();*/               
            }
            ImgList.Clear();
        }

        private CSVOperate CSVFile;

        private DataTable CSVDataTable;       

        private void button1_Click_1(object sender, EventArgs e)
        {
            CSVFile = new CSVOperate(null);
            CSVDataTable = CSVFile.GetCSVData();
            //显示
            ShowDataInListView(CSVDataTable, this.listView1);

           
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

        private void button2_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;            
            Thread workThread = new Thread(new ParameterizedThreadStart(WorkThread));
            workThread.IsBackground = true;
            workThread.Start(CSVDataTable);
        }
    }


}
