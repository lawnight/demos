using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 淘宝工具
{
    public partial class ShowDetail : Form
    {
        public ShowDetail()
        {
            InitializeComponent();
        }

        public void SetString(string str)
        {
            textBox1.Text = str;
        }
    }
}
