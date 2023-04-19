using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zastita_Projekat
{
    public class RailfenceCypher
    {
      
        public RailfenceCypher()
        {
        }
        private static RailfenceCypher instance = null;
        public static RailfenceCypher GetInstance()
        {
            if (instance != null)
                return instance;
            instance = new RailfenceCypher();
            return instance;
        }

       

        
        public string Decrypt(string ciphertext)
        {
            string plaintext = null;
            int j = 0, k = 0, mid;
            char[] ctca = ciphertext.ToCharArray();
            char[,] railarray = new char[2,33500];
            if (ctca.Length % 2 == 0) mid = ((ctca.Length) / 2) - 1;
            else mid = (ctca.Length) / 2;
            for (int i = 0; i < ctca.Length; ++i)
            {
                if (i <= mid)
                {
                    railarray[0, j] = ctca[i];
                    ++j;
                }
                else
                {
                    railarray[1, k] = ctca[i];
                    ++k;
                }
            }
            railarray[0, j] = '\0';
            railarray[1, k] = '\0';
            for (int x = 0; x <= mid; ++x)
            {
                if (railarray[0, x] != '\0') plaintext += railarray[0, x];
                if (railarray[1, x] != '\0') plaintext += railarray[1, x];
            }
            return plaintext;
        }
        public string Encrypt(string plaintext)
        {
            string ciphertext = null;
            int j = 0, k = 0;
            char[] ptca = plaintext.ToCharArray();
            char[,] railarray = new char[2, 33500];
            for (int i = 0; i < ptca.Length; ++i)
            {
                if (i % 2 == 0)
                {
                    railarray[0, j] = ptca[i];
                    ++j;
                }
                else
                {
                    railarray[1, k] = ptca[i];
                    ++k;
                }
            }
            railarray[0, j] = '\0';
            railarray[1, k] = '\0';
            for (int x = 0; x < 2; ++x)
            {
                for (int y = 0; railarray[x, y] != '\0'; ++y) ciphertext += railarray[x, y];
            }
            return ciphertext;
        }


    }
}