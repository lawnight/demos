using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//新添加命名空间  
using System.Net;  
using System.IO;
using System.Text.RegularExpressions;  


namespace WebBrowserCode
{
    public partial class Form1 : Form
    {

        string Url = @"http://hmall.jd.com/view_search-157854-1001862-1001862-0-5-0-0-1-2-24.html?isGlobalSearch=0";

        public Form1()
        {
            InitializeComponent();
        }
      
        private void button1_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(textBox1.Text.Trim());         //显示网页
        }
      
        //定义num记录listBox2中获取到的图片URL个数
        public int num = 0;
        //点击"获取"按钮
        private void button2_Click(object sender, EventArgs e)
        {
            HtmlElement html = webBrowser1.Document.Body;      //定义HTML元素
            string str = html.OuterHtml;                       //获取当前元素的HTML代码
            MatchCollection matches;                           //定义正则表达式匹配集合
            //清空
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            //获取
            try
            {          
                //正则表达式获取<a href></a>内容url
                matches = Regex.Matches(str, "<a href=\"([^\"]*?)\".*?>(.*?)</a>", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    listBox1.Items.Add(match.Value.ToString());     
                }
                //正则表达式获取<img src=>图片url
                matches = Regex.Matches(str, @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    listBox2.Items.Add(match.Value.ToString());
                }
                //记录图片总数
                num = listBox2.Items.Count;
                
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message);    //异常处理
            }
        }

        //点击"下载"实现下载图片
        private void button3_Click(object sender, EventArgs e)
        {
            string imgsrc = string.Empty;             //定义
            //循环下载
            for (int j = 0; j < num; j++)
            {
                string content = listBox2.Items[j].ToString();    //获取图片url
                Regex reg = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
                MatchCollection mc = reg.Matches(content);        //设定要查找的字符串
                foreach (Match m in mc)
                {                
                    try
                    {
                        WebRequest request = WebRequest.Create(m.Groups["src"].Value);    //图片src内容
                        WebResponse response = request.GetResponse();
                        //文件流获取图片操作
                        Stream reader = response.GetResponseStream();
                        string path = "E://" + j.ToString() + ".jpg";                     //图片路径命名 
                        FileStream writer = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                        byte[] buff = new byte[512];
                        int c = 0;                                                        //实际读取的字节数   
                        while ((c = reader.Read(buff, 0, buff.Length)) > 0)
                        {
                            writer.Write(buff, 0, c);
                        }
                        //释放资源
                        writer.Close();
                        writer.Dispose();
                        reader.Close();
                        reader.Dispose();
                        response.Close();
                        //下载成功
                        listBox2.Items.Add(path + ":图片保存成功!"); 
                    }
                    catch (Exception msg)
                    {
                        MessageBox.Show(msg.Message);
                    }
                }
            }
        }
                
        /// <summary> 
        /// 取得HTML中所有图片的 URL
        /// </summary> 
        /// <param name="sHtmlText">HTML代码</param> 
        /// <returns>图片的URL列表</returns> 
        public static string[] GetHtmlImageUrlList(string sHtmlText)
        {
            // 定义正则表达式用来匹配 img 标签 
            Regex regImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);

            // 搜索匹配的字符串 
            MatchCollection matches = regImg.Matches(sHtmlText);
            int i = 0;
            string[] sUrlList = new string[matches.Count];

            // 取得匹配项列表 
            foreach (Match match in matches)
            {
                sUrlList[i++] = match.Groups["imgUrl"].Value;
            }
            return sUrlList;
        }


        /// <summary>
        /// 获得图片的路径并存放
        /// </summary>
        /// <param name="M_Content">要检索的内容</param>
        /// <returns>IList</returns>
        public static IList<string> GetPicPath(string M_Content)
        {
            IList<string> im = new List<string>();//定义一个泛型字符类
            Regex reg = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(M_Content); //设定要查找的字符串
            foreach (Match m in mc)
            {
                im.Add(m.Groups["src"].Value);
            }
            return im;
        }

        /// <summary>
        /// 获取价格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(Url);
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                HtmlDocument doc = webBrowser1.Document;
                foreach (HtmlElement item in doc.GetElementsByTagName("div"))
                {                
                    if (item.GetAttribute("classname") == "jGoodsInfo")
                    {

                    }
                } 
            }           
        }

        


    }
}
