using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SindaSoft.ActiveRecordInspector
{
    public partial class ErrorLog : Form
    {
        public string error_log;

        public ErrorLog()
        {
            InitializeComponent();
        }

        private void ErrorLog_Load(object sender, EventArgs e)
        {
            tbErrorTxt.Dock = DockStyle.Fill;
            tbErrorTxt.Text = error_log;
            tbErrorTxt.WordWrap = false;
        }
    }
}
