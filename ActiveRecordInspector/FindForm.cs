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
    public partial class FindForm : Form
    {
        public MainForm parent;

        List<HtmlElement> retval = new List<HtmlElement>();
        int itemIdx = -1;

        public FindForm()
        {
            InitializeComponent();
            btnFindNext.Enabled = false;
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (itemIdx >= 0)
                retval[itemIdx].Style = null;

            retval.Clear();
            int cnt = parent.SearchHtmlElement(parent.GetRootHtmlElement(), tbText2Find.Text, retval, cbIgnoreCase.Checked);
            lblFindStatus.Text = cnt.ToString() + " item(s) find";
            itemIdx = -1;

            if (cnt > 0)
            {
                btnFindNext.Enabled = true;
                btnFindNext_Click(sender, e);
            }
            else
                btnFindNext.Enabled = false;
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if(itemIdx >= 0)
                retval[itemIdx].Style = null;

            itemIdx++;

            if (itemIdx + 1 > retval.Count)
                itemIdx = 0;

            retval[itemIdx].ScrollIntoView(true);
            retval[itemIdx].Style = "background:orange;";

            lblFindStatus.Text = retval.Count.ToString() + " item(s) find\nShow " + (itemIdx+1).ToString() + " of " + retval.Count.ToString();

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FindForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(itemIdx >=0)
                retval[itemIdx].Style = null;
        }

    }
}
