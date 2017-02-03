using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderCompare
{
    public partial class frmMain : Form
    {
        Boolean continueProcessing = true;
        public frmMain()
        {
            
            InitializeComponent();
            string myPath = getMyDataPath();
            if (!Directory.Exists(myPath))
            {
                Directory.CreateDirectory(myPath);
            }
        }
        private string getMyDataPath() { return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\" + Application.ProductName + "\\"; }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void btnShowFolderSource_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtDestination.Text;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtSource.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnShowFolderDestination_Click(object sender, EventArgs e)
        {

          
            folderBrowserDialog1.SelectedPath = txtDestination.Text;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtDestination.Text = folderBrowserDialog1.SelectedPath;
            }
        }

       
        private void btnCompare_Click(object sender, EventArgs e)
        {
            continueProcessing = true;
            lvActivityLog.Items.Clear();
            Queue<string> folders = new Queue<string>();
            string myDateTime = DateTime.Now.ToString();
            string myLogFileName = getMyDataPath() + "FolderCompareLog-" + DateTime.Now.ToString("yyyyMMdHHmmssff") + ".csv";

            if (string.IsNullOrEmpty(txtSource.Text.Trim()))
            {
                MessageBox.Show("Source Directory Required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtSource.Focus();
                return;
            }
            if (!Directory.Exists(txtSource.Text.Trim()))
            {
                MessageBox.Show("Source Directory not Found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtSource.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtDestination.Text.Trim()))
            {
                MessageBox.Show("Destination Directory Required", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDestination.Focus();
                return;
            }
            if (!Directory.Exists(txtDestination.Text.Trim()))
            {
                MessageBox.Show("Destination Directory not Found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDestination.Focus();
                return;
            }
            if (txtSource.Text.Trim().Equals(txtDestination.Text.Trim()))
            {
                MessageBox.Show("Source and Destination Directory are the same", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDestination.Focus();
                return;
            }           
            
            btnCompare.Enabled = false;
            btnStop.Enabled = true;
            btnShowFolderDestination.Enabled = false;
            btnShowFolderSource.Enabled = false;
            txtDestination.Enabled = false;
            txtSource.Enabled = false;
            Application.DoEvents();
            if (File.Exists(myLogFileName))
            {
                File.Delete(myLogFileName);
            }
            StreamWriter logFile = new System.IO.StreamWriter(myLogFileName);
            lvActivityLog.Items.Add( new ListViewItem(new string[] {"Compare Started at" + myDateTime },0));
            lvActivityLog.Items.Add(new ListViewItem(new string[] { "Source Directory:" + txtSource.Text },0));
            lvActivityLog.Items.Add(new ListViewItem(new string[] {  "Destination Directory:" + txtDestination.Text },0));
            lvActivityLog.Items.Add(new ListViewItem(new string[] { "CSV Log File:" + myLogFileName}, 0));
            logFile.WriteLine("Name, Type, Status,\"Source File Size (bytes)\",\"Destination File Size (bytes)\",\"Info\"");
            logFile.WriteLine(",,,,,\"Compare Started at" + myDateTime + "\"");
            logFile.WriteLine(",,,,,\"Source Directory:" + txtSource.Text + "\"");
            logFile.WriteLine(",,,,,\"Destination Directory:" + txtDestination.Text + "\"");
            folders.Enqueue(txtSource.Text);
            Boolean foundFile = true;
            string statusLine = "";
            int imageIndex = 2;
            long destLen = 0;
            long sourceLen = 0;
            while (folders.Count != 0)
            {
                string processFolder = folders.Dequeue();
                try
                {
                    string[] filesInCurrent = Directory.GetFiles(processFolder, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (var vFile in filesInCurrent)
                    {
                        FileInfo infoSource = new FileInfo(vFile);
                        string tmpNameSource = infoSource.FullName.Substring(txtSource.Text.Length + 1);
                        string tmpNameDestination = txtDestination.Text.Trim() + "\\" + infoSource.FullName.Substring(txtSource.Text.Length + 1);
                        FileInfo infoDestination = new FileInfo(tmpNameDestination);
                        foundFile = true;
                        destLen = 0;
                        if (infoDestination.Exists == false)
                        {
                            foundFile = false;
                            statusLine = "File Not Found";
                            imageIndex = 2;
                        }
                        else if (infoSource.Length != infoDestination.Length)
                        {statusLine = "File Size Different";foundFile = true; imageIndex = 1;}
                        else { statusLine = "File Found";foundFile = true; imageIndex = 3; }
                        sourceLen = infoSource.Length;
                        if (foundFile == true)
                        {
                            destLen = infoDestination.Length;
                        }
                        logFile.WriteLine("\"" + vFile + "\",File,\""+ statusLine + "\","+ infoSource.Length+","+ destLen);
                        lvActivityLog.Items.Add(new ListViewItem(new string[] { vFile,"File",statusLine, sourceLen.ToString(), destLen.ToString() }, imageIndex));
                        Application.DoEvents();
                        if (continueProcessing == false)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logFile.WriteLine(",,,," + ex.ToString());
                    lvActivityLog.Items.Add(new ListViewItem(new string[] { ex.ToString() }, 4));
                }
                try
                {           
                        string[] subFolders = Directory.GetDirectories(processFolder, "*.*", SearchOption.TopDirectoryOnly);
                        foreach (string vFolder in subFolders)
                        {
                            folders.Enqueue(vFolder);
                            string tmpFolderDestination = txtDestination.Text.Trim() + "\\" + vFolder.Substring(txtSource.Text.Length + 1);
                            int folderImageImdex = 5;
                            string folderStatus = "Folder Found";
                            if (!Directory.Exists(tmpFolderDestination))
                            {
                                 folderImageImdex = 6;
                                 folderStatus = "Folder Not Found";
                           }
                            logFile.WriteLine("\""+vFolder+"\",Folder,\"" + folderStatus + "\"" );
                            lvActivityLog.Items.Add(new ListViewItem(new string[] {vFolder , "Folder" , folderStatus }, folderImageImdex));
                            Application.DoEvents();
                            if (continueProcessing == false)
                            {
                               logFile.WriteLine("Processing Stopped");
                               lvActivityLog.Items.Add(new ListViewItem(new string[] {"Processing Stopped" }, 0));
                               break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    logFile.WriteLine(",,,," + ex.ToString());
                    lvActivityLog.Items.Add(new ListViewItem(new string[] { ex.ToString() }, 4));
                }
                Application.DoEvents();
                if (continueProcessing == false)
                {
                    logFile.WriteLine("Processing Stopped");
                    lvActivityLog.Items.Add(new ListViewItem(new string[] { "Processing Stopped" }, 0));
                    break;
                }
            }


       //     lbActivityLog.Items.Add("Compare Ended at" + DateTime.Now);
            btnCompare.Enabled = true;
            btnStop.Enabled = false;
            btnShowFolderDestination.Enabled = true;
            btnShowFolderSource.Enabled = true;
            txtDestination.Enabled = true;
            txtSource.Enabled = true;
            Application.DoEvents();
            if (continueProcessing == true)
            {
                string cmpDateTime = DateTime.Now.ToString("yyyyMMdHHmmssff");
                logFile.WriteLine("Compare Complete "+ cmpDateTime);
                lvActivityLog.Items.Add(new ListViewItem(new string[] { "Compare Complete "+ cmpDateTime }, 0));
            }
            logFile.Close();

        }


        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void compareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnCompare.PerformClick();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
             continueProcessing = false;
        }
    }
}
