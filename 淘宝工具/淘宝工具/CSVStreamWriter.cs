using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace 淘宝工具
{
    public class CSVStreamWriter : StreamWriter
    {

        public CSVStreamWriter(Stream stream, Encoding coder)
            : base(stream, coder)
        {
                      
        }

        public override void Write(string value)
        {
            //对分号的处理
            value = value.Replace("\"", "\"\"");

            //含有分隔符
            if (value.Contains('\t')||value.Contains('\n'))
            {
                value = string.Format("\"{0}\"", value);
            }           

            base.Write(value);
        }
    }
}
