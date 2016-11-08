using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace SindaSoft.ActiveRecordInspector
{
    public class RecentFilesManager
    {
        private Form frmParent;
        private ToolStripMenuItem recentToolStripMenuItem;
        private List<string> recentFiles = null;
        private string rfFilename = ".recentFiles";
        private string lastSelectedFilename = null;
        private int maxNumOfFiles = 5;

        public event EventHandler OnRecentFileSelected = null;


        public RecentFilesManager(Form parent, ToolStripMenuItem rfMenuItem, int maxnumoffiles)
        {
            maxNumOfFiles = maxnumoffiles;
            frmParent = parent;
            recentToolStripMenuItem = rfMenuItem;

            this.InitializeRecentFiles();
        }


        public RecentFilesManager(Form parent, ToolStripMenuItem rfMenuItem) : this(parent, rfMenuItem, 5)
        {
        }


        private void InitializeRecentFiles()
        {
            try
            {
                Uri u = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase));
                string appDirectory = u.LocalPath;
                recentFiles = new List<string>(File.ReadAllLines(Path.Combine(appDirectory, rfFilename)));
            }
            catch
            {
                recentFiles = new List<string>();
            }
            RebuildRecentFiles();
        }

        private void SaveRecentFiles()
        {
            Uri u = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase));
            string appDirectory = u.LocalPath;

            FileInfo fi = new FileInfo(Path.Combine(appDirectory, rfFilename));
            if(fi.Exists)
                fi.Attributes &= ~FileAttributes.Hidden;
            File.WriteAllLines(fi.FullName, recentFiles.ToArray());
            fi.Attributes = FileAttributes.Hidden | FileAttributes.Archive;
        }

        private void RebuildRecentFiles()
        {
            recentToolStripMenuItem.DropDownItems.Clear();
            if (recentFiles.Count > 0)
            {
                ToolStripMenuItem ttsm;
                foreach (string fn in recentFiles)
                {
                    ttsm = new ToolStripMenuItem(fn);
                    ttsm.Click += new EventHandler(ttsm_Click);
                    recentToolStripMenuItem.DropDownItems.Add(ttsm);
                }

                recentToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                ttsm = new ToolStripMenuItem("Clear recent files list");
                recentToolStripMenuItem.DropDownItems.Add(ttsm);
                ttsm.Click += new EventHandler(ttsmClear_Click);


                recentToolStripMenuItem.Text = "Recent files";
                recentToolStripMenuItem.Enabled = true;
            }
            else
            {
                recentToolStripMenuItem.Text = "No recent files";
                recentToolStripMenuItem.Enabled = false;
            }
        }

        private void ttsm_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem ttsm = sender as ToolStripMenuItem;

            lastSelectedFilename = ttsm.Text;

            // Try to load... 
            if (this.OnRecentFileSelected != null)
                this.OnRecentFileSelected(this, EventArgs.Empty);

            InsertFileToRecentList(ttsm.Text);
            RebuildRecentFiles();
            SaveRecentFiles();
        }

        private void ttsmClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear recent files list?",
                                "Recent files",
                                MessageBoxButtons.OKCancel,
                                MessageBoxIcon.Question) == DialogResult.OK)
            {
                recentFiles.Clear();
                RebuildRecentFiles();
                SaveRecentFiles();
            }
        }

        public void InsertFileToRecentList(string fn)
        {
            if (recentFiles == null)
                this.InitializeRecentFiles();

            if (recentFiles.Contains(fn))
            {
                recentFiles.Remove(fn);
            }
            else if (recentFiles.Count > maxNumOfFiles)
            {
                recentFiles.RemoveAt(recentFiles.Count);
            }
            recentFiles.Insert(0, fn);
            RebuildRecentFiles();
            SaveRecentFiles();
        }

        public string GetSelectedRecentFile()
        {
            return lastSelectedFilename;
        }
    }
}
