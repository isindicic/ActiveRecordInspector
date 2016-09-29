using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ActiveRecordInspector
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Timer t = new Timer();
        Inspector ar = null;
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
                    tn.Nodes.Add(s);
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
            webBrowser1.DocumentText = "0";
            webBrowser1.Document.OpenNew(true);


            string html = @"<html>
                                <style>
                                body { font-family:Consolas;  }
                                .vars td { padding: 8px; 	
                                           vertical-align: top; }
                                </style>
                                <body>";

            html += "<table>";
            html += "<tr><td>Class name:</td><td>" + si + "</td></tr>";
            html += "<tr><td>Filename:</td><td>" + Path.GetFileName(ar.arTypes[si].filename) + "</td></tr>";
            html += "<tr><td>Table name:</td><td>" + ar.arTypes[si].table_name + "</td></tr>";
            if (!String.IsNullOrEmpty(ar.arTypes[si].DiscriminatorColumn))
                html += "<tr><td>Where</td><td>" + ar.arTypes[si].DiscriminatorColumn + "=" + ar.arTypes[si].DiscriminatorValue + "</td></tr>";

            html += "</table><br><br>";

            html += "<table class=vars>";

            html += "<tr bgcolor=#CCCCCC>";
            html += "<td>Type</td>";
            html += "<td>Class property name</td>";
            html += "<td>Database column</td>";
            html += "<td>Comment</td>";
            html += "</tr>";

            int cnt = 0;
            foreach (string k in ar.arTypes[si].columns.Keys)
            {
                html += cnt % 2 == 0 ? "<tr>" : "<tr bgcolor=#EEEEEE>";
                cnt++;

                html += "<td>";
                if (ar.arTypes[si].columns[k].linked_to_table != null && ar.arTypes[si].columns[k].linked_to_table.Length > 0)
                {
                    html += typeof(Int32).Name + "  ";
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

                    html += propType;
                }

                html += "</td><td>";
                html += k;
                html += "</td><td>";

                html += ar.arTypes[si].columns[k].column_name;
                html += "</td><td>";

                if (ar.arTypes[si].columns[k].isPrimaryKey)
                    html += "<i>Primary key</i>";
                else if (ar.arTypes[si].columns[k].linked_to_table!=null && ar.arTypes[si].columns[k].linked_to_table.Length > 0)
                    html += " <i>Linked to table</i> " + ar.arTypes[si].columns[k].linked_to_table;

                html += "</td>";

                html += "\n";
                //if(ar.arTypes[si].columns[k].XmlDoc.Length > 0)
                //    label1.Text += "                                      " + ar.arTypes[si].columns[k].XmlDoc + "\n";

                html += "</tr>";
            }
            html += "</table>";
            html += "<br>";
            html += "<br>";


            string[] arr = ar.arTypes[si].derived.Select(x => x.Name.Replace("`1", "<>")).ToArray();
            if (arr.Length > 0)
            {
                html += "C# Class derived from:";
                foreach (string c in arr)
                {
                    html += String.Format("<li><a href=#{0}> {0} </a>", c.Replace("<", "&lt;")
                                                                         .Replace(">", "&gt;")
                                                                         );
                }
            }
            html += "</body>";
            html += "</html>";

            webBrowser1.Document.Write(html);
            webBrowser1.Refresh();
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
