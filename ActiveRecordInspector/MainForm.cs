using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SindaSoft.ActiveRecordInspector
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private Timer t = new Timer();
        private Inspector ar = null;
        private Dictionary<string, TreeNode> class2treenode;
        private string currentHtml = null;
        private string currentHtmlBody = null;

        public string path2investigate = Directory.GetCurrentDirectory();

        private void MainForm_Load(object sender, EventArgs e)
        {
            //splitContainer2.Dock = DockStyle.Fill;
            splitContainer1.Dock = DockStyle.Fill;
            webBrowser1.Dock = DockStyle.Fill;
            webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(webBrowser1_Navigating);

            StartInspection();
        }


        private void StartInspection()
        {
            toolStripStatusLabel1.Text = "Inspecting ... ";
            toolStripProgressBar1.Visible = true;

            t.Interval = 500;
            t.Tick += new EventHandler(t_Tick);
            t.Start();
        }

        void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Url.Fragment) && e.Url.Fragment.StartsWith("#"))
            {
                string clsName = e.Url.Fragment.Substring(1)
                                               .Replace("%3C", "<")
                                               .Replace("%3E", ">");

                if (listBox1.Items.Contains(clsName))
                    listBox1.SelectedItem = clsName;
            }
        }

        void t_Tick(object sender, EventArgs e)
        {
            t.Stop();
            Cursor.Current = Cursors.WaitCursor;

            class2treenode = new Dictionary<string, TreeNode>();

            ar = new Inspector(path2investigate);
            ar.OnProgress += new EventHandler(ar_OnProgress);
            ar.InitInspector();

            List<string> dummy = new List<string>(ar.arTypes.Keys);
            dummy.Sort();
            foreach (string s in dummy)
                listBox1.Items.Add(s);

            List<string> fnames = ar.arTypes.Values.Select(x => x.filename).Distinct().ToList();
            fnames.Sort();
            foreach (string fname in fnames)
            {
                List<ARSingleClassInfo> classes = ar.arTypes.Values.Where(x => x.filename.Equals(fname)).ToList();
                TreeNode tn = treeView1.Nodes.Add(Path.GetFileName(fname));

                dummy = classes.Select(x => x.type.Name.Replace("`1", "<>")).ToList();
                dummy.Sort();

                foreach (string s in dummy)
                    class2treenode[s] = tn.Nodes.Add(s);
            }

            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Text = "Inspected " + ar.arTypes.Count + " ActiveRecord classes in " + fnames.Count + " modules ("+ this.path2investigate + ")";

            showErrorLogToolStripMenuItem.Enabled = !String.IsNullOrEmpty(ar.error_log);

            Cursor.Current = Cursors.Default;
        }

        void ar_OnProgress(object sender, EventArgs e)
        {
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = ar.maxTypeInspected;
            toolStripProgressBar1.Value = ar.currentTypeInspected;

            toolStripStatusLabel1.Text = String.Format("Inspecting {0} %", 100 * ar.currentTypeInspected / ar.maxTypeInspected);
            System.Diagnostics.Debug.WriteLine(toolStripStatusLabel1);
            statusStrip1.Refresh();
            Application.DoEvents(); // Force update... 
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string si = listBox1.SelectedItem as string;
            if (si != null)
            {
                generateSingleClassReport(si);

                if (class2treenode.ContainsKey(si))
                    treeView1.SelectedNode = class2treenode[si];
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;
            if (tn.Nodes.Count == 0)
            {
                listBox1.SelectedItem = tn.Text;
            }
        }

        private void generateSingleClassReport(string si)
        {
            Cursor.Current = Cursors.WaitCursor;

            webBrowser1.DocumentText = "0";
            webBrowser1.Document.OpenNew(true);


            currentHtml = @"<html>
                                <style>
                                body { font-family:Consolas;  }
                                .vars td { padding: 8px; 	
                                           vertical-align: top; }
                                </style>
                                <body>";

            currentHtmlBody = "<table>";
            currentHtmlBody += "<tr><td>Class name:</td><td>" + si + "</td></tr>";
            currentHtmlBody += "<tr><td>Filename:</td><td>" + Path.GetFileName(ar.arTypes[si].filename) + "</td></tr>";
            currentHtmlBody += "<tr><td>Table name:</td><td>" + ar.arTypes[si].table_name + "</td></tr>";
            if (!String.IsNullOrEmpty(ar.arTypes[si].DiscriminatorColumn))
                currentHtmlBody += "<tr><td>Where</td><td>" + ar.arTypes[si].DiscriminatorColumn + "=" + ar.arTypes[si].DiscriminatorValue + "</td></tr>";

            currentHtmlBody += "</table><br><br>";

            currentHtmlBody += "<table class=vars>";

            currentHtmlBody += "<tr bgcolor=#CCCCCC>";
            currentHtmlBody += "<td>Type</td>";
            currentHtmlBody += "<td>Class property name</td>";
            currentHtmlBody += "<td>Database column</td>";
            currentHtmlBody += "<td>Comment</td>";
            currentHtmlBody += "</tr>";

            int cnt = 0;
            foreach (string k in ar.arTypes[si].columns.Keys)
            {
                currentHtmlBody += cnt % 2 == 0 ? "<tr>" : "<tr bgcolor=#EEEEEE>";
                cnt++;

                currentHtmlBody += "<td>";
                if (ar.arTypes[si].columns[k].linked_to_table != null && ar.arTypes[si].columns[k].linked_to_table.Length > 0)
                {
                    currentHtmlBody += typeof(Int32).Name + "  ";
                }
                else
                {
                    string propType;
                    if (ar.arTypes[si].columns[k].property_info.PropertyType.IsGenericType)
                        propType = ar.arTypes[si].columns[k].property_info.PropertyType.ToString();
                    else
                        propType = ar.arTypes[si].columns[k].property_info.PropertyType.Name;

                    propType = propType.Replace("System.Nullable`1[System.DateTime]", "DateTime?");
                    propType = propType.Replace("System.Nullable`1[System.Single]", "Single?");
                    propType = propType.Replace("System.Nullable`1[System.Int32]", "Int32?");

                    currentHtmlBody += propType;
                }

                currentHtmlBody += "</td><td>";
                currentHtmlBody += k;
                currentHtmlBody += "</td><td>";

                currentHtmlBody += ar.arTypes[si].columns[k].column_name;
                currentHtmlBody += "</td><td>";

                if (ar.arTypes[si].columns[k].isPrimaryKey)
                    currentHtmlBody += "<i>Primary key</i>";
                else if (ar.arTypes[si].columns[k].linked_to_table!=null && ar.arTypes[si].columns[k].linked_to_table.Length > 0)
                    currentHtmlBody += " <i>Linked to table</i> " + ar.arTypes[si].columns[k].linked_to_table;

                currentHtmlBody += "</td>";

                currentHtmlBody += "\n";
                //if(ar.arTypes[si].columns[k].XmlDoc.Length > 0)
                //    label1.Text += "                                      " + ar.arTypes[si].columns[k].XmlDoc + "\n";

                currentHtmlBody += "</tr>";
            }
            currentHtmlBody += "</table>";
            currentHtmlBody += "<br>";
            currentHtmlBody += "<br>";


            string[] arr = ar.arTypes[si].derived.Select(x => x.Name.Replace("`1", "<>")).ToArray();
            if (arr.Length > 0)
            {
                currentHtmlBody += "C# Class derived from:";
                foreach (string c in arr)
                {
                    currentHtmlBody += String.Format("<li><a href=#{0}> {0} </a>", c.Replace("<", "&lt;")
                                                                         .Replace(">", "&gt;")
                                                                         );
                }
            }

            currentHtml += currentHtmlBody;
            currentHtml += "</body>";
            currentHtml += "</html>";

            webBrowser1.Document.Write(currentHtml);
            webBrowser1.Refresh();

            Cursor.Current = Cursors.Default;
        }

        private void selectFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = this.path2investigate;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!fbd.SelectedPath.Equals(this.path2investigate))
                {
                    this.path2investigate = fbd.SelectedPath;
                    this.StartInspection();
                }
            }
        }


        private void selectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (Directory.Exists(path2investigate))
                ofd.InitialDirectory = path2investigate;
            else if (File.Exists(path2investigate))
                ofd.InitialDirectory = Path.GetDirectoryName(path2investigate);
            ofd.Filter = "DLL Files (.dll)|*.dll|All Files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!ofd.FileName.Equals(this.path2investigate))
                {
                    this.path2investigate = ofd.FileName;
                    this.StartInspection();
                }
            }

        }

        private void showErrorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErrorLog el = new ErrorLog();
            el.error_log = ar.error_log.Replace("\n", "\r\n");
            el.ShowDialog();
        }

        private void copyDescriptionToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataObject o = new DataObject();
            o.SetData(DataFormats.Html, PrepareHtmlForClipboard(currentHtml));
            Clipboard.SetDataObject(o);  
        }

        private string PrepareHtmlForClipboard(string html)
        {
            StringBuilder sb = new StringBuilder();

            const string header = "Format:HTML  Format\nVersion:1.0\nStartHTML:(*1*)\nEndHTML:(*2*)\nStartFragment:(*3*)\nEndFragment:(*4*)\nStartSelection:(*3*)\nEndSelection:(*3*)";
            sb.Append(header);
            int startHtml = sb.Length;

            sb.Append(@"<!DOCTYPE HTML PUBLIC  ""-//W3C//DTD HTML 4.0  Transitional//EN""><!--StartFragment-->");
            int fragmentStart = sb.Length;

            sb.Append(html);
            int fragmentEnd = sb.Length;

            sb.Append(@"<!--EndFragment-->");
            int endHtml = sb.Length;

            // Backpatch offsets  
            sb.Replace("(*1*)", String.Format("{0,8}", startHtml));
            sb.Replace("(*2*)", String.Format("{0,8}", endHtml));
            sb.Replace("(*3*)", String.Format("{0,8}", fragmentStart));
            sb.Replace("(*4*)", String.Format("{0,8}", fragmentEnd));

            return sb.ToString();
        }  


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
