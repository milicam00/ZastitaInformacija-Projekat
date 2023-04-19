using System;
using System.Collections.Generic;
using System.Text;

namespace Zastita_Projekat
{
    class A52
    {
        private const uint R1MASK = 0x07FFFF;
        private const uint R2MASK = 0x3FFFFF;
        private const uint R3MASK = 0x7FFFFF;
        private const uint R4MASK = 0x01FFFF;
        private const uint R4TAP1 = 0x000400;
        private const uint R4TAP2 = 0x000008;
        private const uint R4TAP3 = 0x000080;
        private const uint R1TAPS = 0x072000;
        private const uint R2TAPS = 0x300000;
        private const uint R3TAPS = 0x700080;
        private const uint R4TAPS = 0x010800;

        private static uint R1, R2, R3, R4;

        private static A52 instance = null;

        public A52()
        {

        }
        public static A52 GetInstance()
        {
            if (instance != null)
                return instance;
            instance = new A52();
            return instance;
        }

        public static bool parity(uint x)
        {
            x ^= x >> 16;
            x ^= x >> 8;
            x ^= x >> 4;
            x ^= x >> 2;
            x ^= x >> 1;
            return (x & 1) == 1;
        }

        public static uint clockone(uint reg, uint mask, uint taps, uint loaded_bit)
        {
            uint t = reg & taps;
            reg = (reg << 1) & mask;
            reg |= (uint)(parity(t) ? 1 : 0);
            reg |= loaded_bit;
            return reg;
        }

        public static bool majority(uint w1, uint w2, uint w3)
        {
            int sum = (w1 != 0 ? 1 : 0) + (w2 != 0 ? 1 : 0) + (w3 != 0 ? 1 : 0);
            return sum >= 2;
        }

        public static void clock(bool allP, int loaded)
        {
            bool maj = majority(R4 & R4TAP1, R4 & R4TAP2, R4 & R4TAP3);
            if (allP || (((R4 & R4TAP1) != 0) == maj))
                R1 = clockone(R1, R1MASK, R1TAPS, (uint)(loaded << 15));
            if (allP || (((R4 & R4TAP2) != 0) == maj))
                R2 = clockone(R2, R2MASK, R2TAPS, (uint)(loaded << 16));
            if (allP || (((R4 & R4TAP3) != 0) == maj))
                R3 = clockone(R3, R3MASK, R3TAPS, (uint)(loaded << 18));
            R4 = clockone(R4, R4MASK, R4TAPS, (uint)(loaded << 10));
        }

        public static bool delaybit = false;

        public static bool getbit()
        {
            uint topbits = (((R1 >> 18) ^ (R2 >> 21) ^ (R3 >> 22)) & 0x01);
            bool nowbit = delaybit;
            delaybit = Convert.ToBoolean(
                topbits
                ^ Convert.ToInt32(majority(Convert.ToUInt32((R1 & 0x8000) != 0), Convert.ToUInt32((R1 & 0x4000) == 0), Convert.ToUInt32((R1 & 0x1000) != 0)))
                ^ Convert.ToInt32(majority(Convert.ToUInt32((R2 & 0x10000) == 0), Convert.ToUInt32((R2 & 0x2000) != 0), Convert.ToUInt32((R2 & 0x200) != 0)))
                ^ Convert.ToInt32(majority(Convert.ToUInt32((R3 & 0x40000) != 0), Convert.ToUInt32((R3 & 0x10000) != 0), Convert.ToUInt32((R3 & 0x2000) == 0)))
            );
            return nowbit;
        }

        public static void keysetup(byte[] key, uint frame)
        {
            R1 = R2 = R3 = R4 = 0;

            for (int i = 0; i < 64; i++)
            {
                clock(true, 0);
                byte keybit = (byte)((key[i / 8] >> (i & 7)) & 1);
                R1 ^= keybit; R2 ^= keybit; R3 ^= keybit; R4 ^= keybit;
            }

            for (int i = 0; i < 22; i++)
            {
                clock(true, Convert.ToInt32(i == 21));
                byte framebit = (byte)((frame >> i) & 1);
                R1 ^= framebit; R2 ^= framebit; R3 ^= framebit; R4 ^= framebit;
            }

            for (int i = 0; i < 100; i++)
            {
                clock(false, 0);
            }

            getbit();
        }

        public byte[] Encrypt(byte[] AtoBkeystream)
        {
            byte[] key = { 0x00, 0xfc, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            uint frame = 0x21;
            byte[] AtoB = { 0xf4, 0x51, 0x2c, 0xac, 0x13, 0x59, 0x37, 0x64, 0x46, 0x0b, 0x72, 0x2d, 0xad, 0xd5, 0x00 };
            keysetup(key, frame);
            bool failed = false;
            for (int i = 0; i <= 113 / 8; i++)
            {
                AtoBkeystream[i]= 0;
            }
            for (int i = 0; i < 114; i++)
            {
                clock(false, 0);
                AtoBkeystream[i / 8] |= Convert.ToByte((getbit() ? 1 : 0) << (7 - (i & 7)));
            }
            for (int i = 0; i < 15; i++)
            {
                if (AtoBkeystream[i] != AtoB[i])
                    failed = true;
            }
            return AtoBkeystream;
        }
        public byte[] Decrypt(byte[] BtoAkeystream)
        {
            byte[] key = { 0x00, 0xfc, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
            uint frame = 0x21;
            byte[] BtoA = { 0x48, 0x00, 0xd4, 0x32, 0x8e, 0x16, 0xa1, 0x4d, 0xcd, 0x7b, 0x97, 0x22, 0x26, 0x51, 0x00 };
            bool failed = false;
            keysetup(key, frame);
            for (int i = 0; i <= 113 / 8; i++)
            {
                BtoAkeystream[i] = 0;
            }
            for (int i = 0; i < 114; i++)
            {
                clock(false, 0);
                BtoAkeystream[i / 8] |= Convert.ToByte((getbit() ? 1 : 0) << (7 - (i & 7)));
            }
            for (int i = 0; i < 15; i++)
            {
                if (BtoAkeystream[i] != BtoA[i])
                    failed = true;
            }
            return BtoAkeystream;
        }

       /* public void run(byte[] AtoBkeystream, byte[] BtoAkeystream)
         {
             for (int i = 0; i <= 113 / 8; i++)
             {
                 AtoBkeystream[i] = BtoAkeystream[i] = 0;
             }

             for (int i = 0; i < 114; i++)
             {
                 clock(false, 0);
                 AtoBkeystream[i / 8] |= Convert.ToByte((getbit() ? 1 : 0) << (7 - (i & 7)));
             }

             for (int i = 0; i < 114; i++)
             {
                 clock(false, 0);
                 BtoAkeystream[i / 8] |= Convert.ToByte((getbit() ? 1 : 0) << (7 - (i & 7)));
             }

         }*/

     
       
        


    }
}
