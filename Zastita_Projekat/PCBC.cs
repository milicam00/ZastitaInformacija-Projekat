using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Zastita_Projekat
{
    class PCBC
    {

        //PCBC primenjen na XTEA

        private static PCBC instance = null;
     
        public PCBC()
        {
          
        }
        public static PCBC GetInstance()
        {
            if (instance != null)
                return instance;
            instance = new PCBC();
            return instance;
        }
        public byte[] Encrypt(byte[] plaintext)
        {
            byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            byte[] iv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

            
            byte[] previousCipherBlock = iv;
            string result = "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    
                    for (int i = 0; i < plaintext.Length / 16; i ++)
                    {
                        byte[] currentPlainBlock = new byte[16];
                        Array.Copy(plaintext, i*16, currentPlainBlock, 0,16);
                        byte[] niz = new byte[16];
                        
                        for (int j = 0; j < currentPlainBlock.Length; j++)
                        {
                            currentPlainBlock[j] ^= previousCipherBlock[j];
                            niz[j] =(byte) (currentPlainBlock[j] ^ previousCipherBlock[j]);
                        }
                        XTEA xtea = Zastita_Projekat.XTEA.GetInstance();
                  
                        string str = xtea.Encrypt(Encoding.UTF8.GetString(currentPlainBlock));
                        result += str;
                        for(int x=0;x < currentPlainBlock.Length;x++)
                        {
                            niz[x] ^= currentPlainBlock[x];
                        }
                        iv = niz;
                      
                    }
                }
            }
            Console.WriteLine("PCBC je enkriptovao, rezultat:");
            Console.WriteLine(result);
            return Encoding.UTF8.GetBytes(result);
        }

        public byte[] Decrypt(byte[] ciphertext)
        {
            byte[] key = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
            byte[] iv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };
          
            string result = "";
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.None;
                aes.Mode = CipherMode.CBC;
                XTEA xtea = Zastita_Projekat.XTEA.GetInstance();
                for(int i=0;i<ciphertext.Length/16;i++)
                {
                    string str = "";
                    byte[] arr = new byte[16];
                    byte[] niz = new byte[16];
                    Array.Copy(ciphertext, i * 16, niz, 0, 16);
                    str= xtea.Decrypt(Encoding.UTF8.GetString(niz));
                    byte[] array = Encoding.UTF8.GetBytes(str);
                    for(int x=0;x<16;x++)
                    {
                        arr[x] = (byte)(array[x] ^ iv[x]);
                        
                    }
                    result += Encoding.UTF8.GetString(arr);
                    for(int p=0;p<16;p++)
                    {
                        iv[p] =(byte)( arr[p] ^ niz[p]);
                    }
                }
                
            }
           
            return Encoding.UTF8.GetBytes(result);
        }
      


    }
}