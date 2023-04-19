using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;

namespace Zastita_Projekat
{
    class FileSystem
    {
        
        private string outputDirectory;
        public bool xtea_checked;  
        public bool railfence_checked;
        public bool a52_checked;
        public bool pcbc_checked;
        public MD5 hash;
        public int brojNiti = 1;

        public FileSystem()
        {
            xtea_checked = false;
            railfence_checked = false;
            pcbc_checked = true;
            a52_checked = false;
            outputDirectory = "";

        }

        public void SetOutputDirectory(string dir)
        {
            outputDirectory = dir;
        }

       
        public void EncodeFileFromPath(string path)
        {
            string all = ReadTextFile(path);
            hash = MD5.Create(all);

            if (outputDirectory.Length == 0)
                throw new Exception("Destination folder not set!");
            if (path.EndsWith(".txt"))
                EncodeTextFile(path, outputDirectory);
            else if (path.EndsWith(".bmp"))
               EncodeBmpFile(path, outputDirectory);
        }

        private byte[] ReadBinaryFile(string path)
        {
            long len = new FileInfo(path).Length;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using BinaryReader br = new BinaryReader(fs);
            byte[] all = br.ReadBytes((int)len);
            return all;
        }

      

        private bool EncodeTextFile(string fullFileName, string outputDirectory)
        {
          
            if (xtea_checked)
            {
                string textForCoding = ReadTextFile(fullFileName);
                string outputFileName1 = fullFileName.Remove(fullFileName.Length - 4, 4) + "Enc.xtea"; 
                int lastBackslashIndex = outputFileName1.LastIndexOf('\\');
                string outputFileName = fullFileName;
                if (lastBackslashIndex >= 0)
                {
                    outputFileName = outputDirectory + "\\" + outputFileName1.Substring(lastBackslashIndex + 1);
                }
                XTEA xtea = Zastita_Projekat.XTEA.GetInstance();
                string encodedText = xtea.Encrypt(textForCoding);
                Console.WriteLine(xtea.Encrypt(textForCoding));
                WriteToTextFile(outputFileName, encodedText);

                
            }
            if (railfence_checked)
            {
                string textForCoding = ReadTextFile(fullFileName);
                string outputFileName1 = fullFileName.Remove(fullFileName.Length - 4, 4) + "Enc.RAIL";
                int lastBackslashIndex = outputFileName1.LastIndexOf('\\');
                string outputFileName = fullFileName;
                if (lastBackslashIndex >= 0)
                {
                    outputFileName = outputDirectory + "\\" + outputFileName1.Substring(lastBackslashIndex + 1);
                }
                RailfenceCypher railfence = new RailfenceCypher();
                string encodedText = railfence.Encrypt(textForCoding);
                WriteToTextFile(outputFileName, encodedText);

                
            }
            if (pcbc_checked)
            {
                byte[] TextForCoding = ReadBinaryFile(fullFileName);
                string outputFileName1 = fullFileName.Remove(fullFileName.Length - 4, 4) + "Enc.pcbc"; 
                int lastBackslashIndex = outputFileName1.LastIndexOf('\\');
                string outputFileName = fullFileName;
                if (lastBackslashIndex >= 0)
                {
                    outputFileName = outputDirectory + "\\" + outputFileName1.Substring(lastBackslashIndex + 1);
                }
                PCBC pcbc = Zastita_Projekat.PCBC.GetInstance();
                byte[] encodedText = pcbc.Encrypt(TextForCoding);
                WriteToBinaryFile(outputFileName, encodedText);


            }
            if (a52_checked)
            {
                byte[] TextForCoding = ReadBinaryFile(fullFileName);
                string outputFileName1 = fullFileName.Remove(fullFileName.Length - 4, 4) + "Enc.a52";
                int lastBackslashIndex = outputFileName1.LastIndexOf('\\');
                string outputFileName = fullFileName;
                if (lastBackslashIndex >= 0)
                {
                    outputFileName = outputDirectory + "\\" + outputFileName1.Substring(lastBackslashIndex + 1);
                }
                A52 a52 = Zastita_Projekat.A52.GetInstance();
                byte[] encodedText = a52.Encrypt(TextForCoding);
                WriteToBinaryFile(outputFileName, encodedText);
            }
           return true;
        }
        private static void WriteToTextFile(string path, string content)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
                using StreamWriter sw = new StreamWriter(fs);
                sw.Write(content);
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }

        private string ReadTextFile(string path)
        {
            ReadBinaryFile(path); 
            using StreamReader sr = new StreamReader(path);
            return sr.ReadToEnd().ToString();
        }

       

        public string DecodeFile(string fullFileName, string targetFolder)
        {

            if (fullFileName.Contains("Enc.xtea"))
            {
                int lastBackslashIndex = fullFileName.LastIndexOf('\\');
                string charsAfterLastBackslash = fullFileName.Substring(lastBackslashIndex + 1);
                string outputFileName = targetFolder + "\\" + charsAfterLastBackslash.Remove(charsAfterLastBackslash.Length - 7, 7) + "DecXTEA.txt";
                string codedText = ReadTextFile(fullFileName);
                var XTEA = Zastita_Projekat.XTEA.GetInstance();
                string decodedText = XTEA.Decrypt(codedText);
                WriteToTextFile(outputFileName, decodedText);

                return outputFileName;
            }
            else if (fullFileName.Contains("Enc.RAIL"))
            {
                int lastBackslashIndex = fullFileName.LastIndexOf('\\');
                string charsAfterLastBackslash = fullFileName.Substring(lastBackslashIndex + 1);
                string outputFileName = targetFolder + "\\" + charsAfterLastBackslash.Remove(charsAfterLastBackslash.Length - 7, 7) + "DecRailfenceCypher.txt";
                string codedText = ReadTextFile(fullFileName);
                var railfence = new RailfenceCypher();
                string decodedText = railfence.Decrypt(codedText);
                WriteToTextFile(outputFileName, decodedText);

                return outputFileName;
            }
            else if(fullFileName.Contains(".bmp"))
            {
                string IM = DecodeBmpFile(fullFileName, targetFolder);
                return IM;
            }

            else if (fullFileName.Contains("Enc.pcbc"))
            {
                int lastBackslashIndex = fullFileName.LastIndexOf('\\');
                string charsAfterLastBackslash = fullFileName.Substring(lastBackslashIndex + 1);
                string outputFileName = targetFolder + "\\" + charsAfterLastBackslash.Remove(charsAfterLastBackslash.Length - 7, 7) + "DecPCBC.txt";
                byte[] codedText = ReadBinaryFile(fullFileName);
                var pcbc = Zastita_Projekat.PCBC.GetInstance();
                byte[] decodedText = pcbc.Decrypt(codedText);
                string kript = decodedText.ToString();
                WriteToTextFile(outputFileName, kript);

                return outputFileName;
            }
            else if (fullFileName.Contains("Enc.a52"))
            {
                int lastBackslashIndex = fullFileName.LastIndexOf('\\');
                string charsAfterLastBackslash = fullFileName.Substring(lastBackslashIndex + 1);
                string outputFileName = targetFolder + "\\" + charsAfterLastBackslash.Remove(charsAfterLastBackslash.Length - 7, 7) + "DecA52.txt";
                byte[] codedText = ReadBinaryFile(fullFileName);
                var a52 = new A52();
                byte[] decodedText = a52.Decrypt(codedText);
                string kript = decodedText.ToString();
                WriteToTextFile(outputFileName, kript);

                return outputFileName;
            }

            return "";
    }

        private static void WriteToBinaryFile(string path, byte[] content)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Create);
                using BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(content, 0, content.Length);

            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }

        public void EncodeBmpFile(string fullFileName, string outputDirectory)
        {
            byte[] all_data = File.ReadAllBytes(fullFileName);
            byte[] header = new byte[54];
            Array.Copy(all_data, header, 54);
            var pc = A52.GetInstance();          
            byte[] data = pc.Encrypt(all_data.Skip(54).ToArray());
            byte[] encryptedData = new byte[header.Length + data.Length];
            Array.Copy(header, encryptedData, header.Length);
            Array.Copy(data, 0, encryptedData, header.Length, data.Length); 

            using (MemoryStream stream = new MemoryStream(encryptedData))
            {
                Bitmap img = new Bitmap(stream);
                string outputFileName1 = fullFileName.Remove(fullFileName.Length - 4, 4) + "Enc.bmp"; 
                int lastBackslashIndex = outputFileName1.LastIndexOf('\\');

                string outputFileName = fullFileName;
                if (lastBackslashIndex >= 0)
                {
                    outputFileName = outputDirectory + "\\" + outputFileName1.Substring(lastBackslashIndex + 1);
                }
                img.Save(outputFileName);
            }

        }

        public string DecodeBmpFile(string fullFileName, string outputDirectory)
        {
            byte[] all_data = File.ReadAllBytes(fullFileName);
            byte[] header = new byte[54];
            Array.Copy(all_data, header, 54);
            var pc = A52.GetInstance();
            byte[] data = pc.Decrypt(all_data.Skip(54).ToArray());
            byte[] decryptedData = new byte[header.Length + data.Length];
            Array.Copy(header, decryptedData, header.Length);
            Array.Copy(data, 0, decryptedData, header.Length, data.Length);

            using (MemoryStream stream = new MemoryStream(decryptedData))
            {
                Bitmap img = new Bitmap(stream);
                string outputFileName1 = fullFileName.Remove(fullFileName.Length - 7, 7) + "Dec.bmp"; 
                int lastBackslashIndex = outputFileName1.LastIndexOf('\\');
                string outputFileName = fullFileName;
                if (lastBackslashIndex >= 0)
                {
                    outputFileName = outputDirectory + "\\" + outputFileName1.Substring(lastBackslashIndex + 1);
                }

                img.Save(outputFileName);
                return outputFileName;
            }

        }
        public bool Valid(string path)
        {
            string fajl = File.ReadAllText(path);
            MD5 A = MD5.Create(fajl);
            if (hash != null && A.Equals(hash)) 
                return true;
            else
                return false;
        }



    }
}
