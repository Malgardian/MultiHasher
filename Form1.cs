using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace MultiHasher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listView1.DoubleBuffered(true);
        }

        string logfilepath = Directory.GetCurrentDirectory() + "\\multihasher.log";

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox1.Text = folderBrowserDialog1.SelectedPath;
            listView1.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.DefaultExt = ".txt";
            saveFileDialog1.Title = "Choose and name your output file...";
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.ValidateNames = true;
            saveFileDialog1.Filter = "All files (.*) | *.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filepath = saveFileDialog1.FileName;
                //MessageBox.Show(filepath);
                //code to write data to file
                if (listView1.Items.Count > 0)
                {
                    int max_str_length = 0;
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        if (lvi.Text.Length > max_str_length)
                        {
                            max_str_length = lvi.Text.Length;
                        }
                    }
                    //MessageBox.Show("Number " + max_str_length.ToString());
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        string line_entry = lvi.Text.PadRight(max_str_length, ' ');
                        //foreach (ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
                        //{
                        //    line_entry += "\t" + lvsi.Text;
                        //}
                        for (int i = 1; i < lvi.SubItems.Count; i++)
                        {
                            line_entry += "\t" + lvi.SubItems[i].Text;
                        }
                        line_entry += Environment.NewLine;
                        //MessageBox.Show(line_entry);
                        File.AppendAllText(filepath, line_entry);
                        //File.WriteAllText(filepath, line_entry);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            //string dirpath = textBox1.Text;
            //calculate_hashes_directory(dirpath);
            backgroundWorker1.RunWorkerAsync();
        }

        private void calculate_hashes_directory(string dirpath)
        {
            DirectoryInfo dir = new DirectoryInfo(dirpath);
            DirectoryInfo[] subdirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Couldn't find supplied directory:" + dirpath);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string filepath = file.FullName;
                string filehash_md5 = calc_hash_md5(file.FullName);
                string filehash_sha256 = calc_hash_sha256(file.FullName);
                string[] filedata = { filepath, filehash_md5, filehash_sha256 };
                backgroundWorker1.ReportProgress(0, filedata);
                //ListViewItem lvi = new ListViewItem(filedata);
                //listView1.Items.Add(lvi);
                //writeToLog(filepath + "\t" + filehash_md5 + "\t" + filehash_sha256);
            }

            foreach (DirectoryInfo subdir in subdirs)
            {
                calculate_hashes_directory(subdir.FullName);
            }
        }

        private string calc_hash_md5(string filepath)
        {
            using (FileStream fs = File.OpenRead(filepath))
            {
                MD5 sha = MD5.Create();
                byte[] hash = sha.ComputeHash(fs);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }

        private string calc_hash_sha256(string filepath)
        {
            using (FileStream fs = File.OpenRead(filepath))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(fs);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }

        private void writeToLog(string text)
        {
            string logging = "[" + DateTime.Now.ToString() + "]\t" + text + Environment.NewLine;
            File.AppendAllText(logfilepath, logging);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            calculate_hashes_directory(textBox1.Text);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            label2.Text = listView1.Items.Count + " entries";
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string[] filedata = (string[])e.UserState;
            ListViewItem lvi = new ListViewItem(filedata);
            //listView1.BeginUpdate();
            listView1.Items.Add(lvi);
            //listView1.EndUpdate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                //writeToLog(lvi.Text);
                //foreach (ListViewItem.ListViewSubItem lvsi in lvi.SubItems)
                //{
                //    writeToLog(lvsi.Text);
                //}
                for (int i = 1; i < lvi.SubItems.Count; i++)
                {
                    writeToLog(lvi.SubItems[i].Text);
                }
            }

            //foreach (ListViewItem lvsi in listView1.Items)
            //{
            //    writeToLog(lvsi.SubItems.Text);
            //}
        }
    }

    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
    }
}
