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

namespace Zastita_Projekat
{
    public partial class Form1 : Form
    {
        private readonly FileSystem fs;

        public Form1()
        {
            InitializeComponent();
            fs = new FileSystem();
        }

        private void ChooseFolder(TextBox tbResult)
        {
            DialogResult result = folderBrowserDialog2.ShowDialog();
            if (result == DialogResult.OK)
                 tbResult.Text = folderBrowserDialog2.SelectedPath;
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (fs.xtea_checked)
                openFileDialog1.Filter = "(*.txt)|*.txt|All files (*.*)|*.*";
            else if (fs.railfence_checked)
                openFileDialog1.Filter = "(*.txt)|*.txt|All files (*.*)|*.*";
            else if (fs.pcbc_checked)
                openFileDialog1.Filter = "(*.txt)|*.txt|All files (*.*)|*.*";
            else if (fs.a52_checked)
                openFileDialog1.Filter = "(*.txt)|*.txt|All files (*.*)|*.*";
          
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
                tbEncodePath.Text += openFileDialog1.FileName + "\r\n";
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void EncriptionAlgorythm_CheckedChanged(object sender, EventArgs e)  
        {
            if (radioButton1.Checked)
            {

                fs.xtea_checked = true;
                fs.railfence_checked = false;
                fs.pcbc_checked = false;
                fs.a52_checked = false;

            }
            else if (radioButton2.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = true;
                fs.pcbc_checked = false;
                fs.a52_checked = false;
            }
            else if (radioButton3.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = false;
                fs.pcbc_checked = false;
                fs.a52_checked = true;
            }
            else if (radioButton4.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = false;
                fs.pcbc_checked = true;
                fs.a52_checked = false;

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ChooseFolder(tbDestEncodeFolder);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {

                fs.xtea_checked = true;
                fs.railfence_checked = false;
                fs.pcbc_checked = false;
                fs.a52_checked = false;

            }
            else if (radioButton2.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = true;
                fs.pcbc_checked = false;
                fs.a52_checked = false;
            }
            else if (radioButton3.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = false;
                fs.pcbc_checked = false;
                fs.a52_checked = true;
            }
            else if (radioButton4.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = false;
                fs.pcbc_checked = true;
                fs.a52_checked = false;

            }
            if (Directory.Exists(tbDestEncodeFolder.Text))

                try
                {
                    if (fs.xtea_checked && XTEA.GetInstance().Key == null)
                    {
                        MessageBox.Show("Key is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else if(fs.pcbc_checked)
                    {
                        byte[] target = Encoding.Unicode.GetBytes(txtKey.Text);
                        uint[] key = new uint[target.Length / 4];
                        Buffer.BlockCopy(target, 0, key, 0, target.Length);
                        XTEA.GetInstance().Key = key;

                    }

                    string str = tbEncodePath.Text;
                    string path = str.Substring(0, str.Length - 2);
                  
                    int index1 = str.IndexOf("\r\n");
                    int index2 = str.IndexOf("\r\n", index1 + 2);

                    if (index1 >= 0 && index2 >= 0)
                    {

                        string[] PathLines = tbEncodePath.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        Array.Resize(ref PathLines, PathLines.Length - 1); 

                        Parallel_Loading(PathLines, fs.brojNiti);

                        MessageBox.Show("Successful encoding!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (!File.Exists(path))
                    {
                        MessageBox.Show("Invalid file path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    fs.SetOutputDirectory(tbDestEncodeFolder.Text);
                    fs.EncodeFileFromPath(path);

                    MessageBox.Show("Successful encoding!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            else
                MessageBox.Show("Invalid directory name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void btnKey_Click(object sender, EventArgs e)
        {

            if (txtKey.Text.Length == 0)
            {
                MessageBox.Show("Key length is 0!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (radioButton1.Checked )
            {
                var XTEA = Zastita_Projekat.XTEA.GetInstance();
                byte[] target = Encoding.Unicode.GetBytes(txtKey.Text);
                if (target.Length % 4 != 0)
                    MessageBox.Show("Invalid key!\nPlease try one with even symbols", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    uint[] key = new uint[target.Length / 4];
                    Buffer.BlockCopy(target, 0, key, 0, target.Length);
                    XTEA.Key = key;

                   
                }
            }
            MessageBox.Show("The key is set", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
        }

    public void Parallel_Loading(string[] PathLines, int num_Threads)
        {
            int numThreads = num_Threads;

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = numThreads;

            Parallel.ForEach(PathLines, options, line => {
                if (!File.Exists(line))
                {
                    MessageBox.Show("Invalid file path:\n" + line, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                fs.SetOutputDirectory(tbDestEncodeFolder.Text);
                fs.EncodeFileFromPath(line);

            });
        }
      

        public void Parallel_Writing(string[] PathLines, int num_Threads)
        {
            int numThreads = num_Threads;

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = numThreads;

            Parallel.ForEach(PathLines, options, line => {
                if (!File.Exists(line))
                {
                    MessageBox.Show("Invalid file path:\n" + line, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                fs.DecodeFile(line, tbDestDecodeFolder.Text);
            });
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fs.xtea_checked)
                openFileDialog3.Filter = "(*.xtea)|*.xtea|All files (*.*)|*.*";
            else if (fs.railfence_checked)
                openFileDialog3.Filter = "(*.RAIL)|*.RAIL|All files (*.*)|*.*";
            else if (fs.pcbc_checked)
                openFileDialog3.Filter = "(*.pcbc)|*.pcbc|All files (*.*)|*.*";
            else if(fs.a52_checked)
                openFileDialog3.Filter = "(*.a52)|*.a52|All files (*.*)|*.*";
          
            DialogResult result = openFileDialog3.ShowDialog();
            if (result == DialogResult.OK)
                tbDecodePath.Text += openFileDialog3.FileName + "\r\n";
            
        }

        private void tbDecodePath_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ChooseFolder(tbDestDecodeFolder);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {

                fs.xtea_checked = true;
                fs.railfence_checked = false;
                fs.pcbc_checked = false;
                fs.a52_checked = false;

            }
            else if (radioButton2.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = true;
                fs.pcbc_checked = false;
                fs.a52_checked = false;
            }
            else if (radioButton3.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = false;
                fs.pcbc_checked = false;
                fs.a52_checked = true;
            }
            else if (radioButton4.Checked)
            {
                fs.xtea_checked = false;
                fs.railfence_checked = false;
                fs.pcbc_checked = true;
                fs.a52_checked = false;

            }
            if (Directory.Exists(tbDestDecodeFolder.Text))
                try
                {
                   if (XTEA.GetInstance().Key == null && fs.xtea_checked)
                    {
                        MessageBox.Show("Key is not set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    string str = tbDecodePath.Text;
                    string path = str.Substring(0, str.Length - 2);
                    int index1 = str.IndexOf("\r\n");
                    int index2 = str.IndexOf("\r\n", index1 + 2);

                    if (index1 >= 0 && index2 >= 0)
                    {
                       
                        string[] PathLines = tbDecodePath.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        Array.Resize(ref PathLines, PathLines.Length - 1);
                        Parallel_Writing(PathLines, fs.brojNiti);
                        MessageBox.Show("Successful decoding!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (!File.Exists(path))
                    {
                        MessageBox.Show("Invalid file path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string outputFileName = fs.DecodeFile(path, tbDestDecodeFolder.Text);

                    bool valid = false;
                    if (outputFileName.Length != 0 && valid)
                        MessageBox.Show("Successful decoding and the file is validated by the MD5!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //TODO
                    else if (outputFileName.Length != 0 && valid == false)
                        MessageBox.Show("Successful decoding, but the file is not valided by MD5!", "Success with warnings", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else
                MessageBox.Show("Invalid file or directory name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value == 0)
            {
                numericUpDown1.Value = 1;
            }
            fs.brojNiti = (int)numericUpDown1.Value;
            txtNiti.Text = "The number of threads you have chosen is: " + ((int)numericUpDown1.Value).ToString();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            openFileDialog2.Filter= "(*.bmp)|*.bmp|(*.txt)|*.txt|All files (*.*)|*.*";
            DialogResult result = openFileDialog2.ShowDialog();
            if (result == DialogResult.OK)
                tbBmpEncodePath.Text += openFileDialog2.FileName + "\r\n";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ChooseFolder(tbBmpDestEncodeFolder);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(tbBmpDestEncodeFolder.Text))

                try
                {
                    string str = tbBmpEncodePath.Text;
                    string path = str.Substring(0, str.Length - 2);
                   
                    int index1 = str.IndexOf("\r\n");
                    int index2 = str.IndexOf("\r\n", index1 + 2);

                    if (index1 >= 0 && index2 >= 0)
                    {
                        string[] PathLines = tbBmpEncodePath.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        Array.Resize(ref PathLines, PathLines.Length - 1);
                        MessageBox.Show("Successful encoding!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (!File.Exists(path))
                    {
                        MessageBox.Show("Invalid file path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    fs.SetOutputDirectory(tbBmpDestEncodeFolder.Text);
                    fs.EncodeFileFromPath(path);

                    MessageBox.Show("Successful encoding!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            else
                MessageBox.Show("Invalid directory name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            openFileDialog3.Filter = "(*.bmp)|*.bmp|All files (*.*)|*.*";
            DialogResult result = openFileDialog3.ShowDialog();
            if (result == DialogResult.OK)
                tbBmpDecodePath.Text += openFileDialog2.FileName + "\r\n";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ChooseFolder(tbBmpDestDecodeFolder);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(tbBmpDestDecodeFolder.Text))
                try
                {
                    string str = tbBmpDecodePath.Text;
                    string path = str.Substring(0, str.Length - 2);

                    int index1 = str.IndexOf("\r\n");
                    int index2 = str.IndexOf("\r\n", index1 + 2);

                    if (index1 >= 0 && index2 >= 0)
                    {
                        string[] PathLines = tbBmpDecodePath.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        Array.Resize(ref PathLines, PathLines.Length - 1); 
                        MessageBox.Show("Successful decoding!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    else if (!File.Exists(path))
                    {
                        MessageBox.Show("Invalid file path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string outputFileName = fs.DecodeFile(path, tbBmpDestDecodeFolder.Text);

                    bool valid = false;

                    if (outputFileName.Length != 0 && valid)
                        MessageBox.Show("Successful decoding and the file is validated by the MD5!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //TODO
                    else if (outputFileName.Length != 0 && valid == false)
                        MessageBox.Show("Successful decoding but the file is not valided by MD5!", "Success with warnings", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else
                MessageBox.Show("Invalid file or directory name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
