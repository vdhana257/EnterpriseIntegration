using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Drawing;

namespace TestPublisherHelper
{

    public class MyTraceListener : TraceListener
    {

        private RichTextBox output;

        public MyTraceListener(RichTextBox output)
        {
            this.Name = "Trace";
            this.output = output;
        }


        public override void Write(string message)
        {

            Action append = delegate () {
                output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString()));
                output.AppendText(message+Environment.NewLine);
            };
            if (output.InvokeRequired)
            {
                output.BeginInvoke(append);
            }
            else
            {
                append();
            }

        }

        public override void Write(string message,string cat)
        {

            Action append = delegate () {
                if (cat == "Red")
                {
                    int startIndex = output.TextLength;
                    output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString()));
                    output.AppendText(message);
                    int endIndex = output.TextLength;
                    output.Select(startIndex, endIndex - startIndex);
                    output.SelectionColor = Color.Red;
                }
                else if (cat == "Green")
                {
                    int startIndex = output.TextLength;
                    output.AppendText(string.Format("[{0}] ", DateTime.Now.ToString()));
                    output.AppendText(message);
                    int endIndex = output.TextLength;
                    output.Select(startIndex, endIndex - startIndex);
                    output.SelectionColor = Color.Green;
                }
            };
            if (output.InvokeRequired)
            {
                output.BeginInvoke(append);
            }
            else
            {
                append();
            }

        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
            output.ScrollToCaret();
            output.Refresh();
        }

        public override void WriteLine(string message, string cat)
        {
            if (cat == "Red")
            {
                Write(message + Environment.NewLine,cat);
                output.ScrollToCaret();
                output.Refresh();
            }
            else if(cat == "Green")
            {
                Write(message + Environment.NewLine,cat);
                output.ScrollToCaret();
                output.Refresh();
            }
        }
    }
}
