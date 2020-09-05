using System;

namespace EasySerial
{
    public class Crc32
    {
        public static int ChecksumLength = sizeof(UInt32);

        private readonly UInt32 polinominal;
        private readonly UInt32 init;

        private readonly UInt32[] table;

        public Crc32(UInt32 polinominal, UInt32 init)
        {
            this.polinominal = polinominal;
            this.init = init;
            this.table = GenerateTable(this.polinominal);
        }

        public UInt32 Calculate(ReadOnlySpan<byte> input)
        {
            UInt32 crc = this.init;
            foreach (var b in input)
            {
                byte index = (byte)((crc ^ (b << 24)) >> 24);
                crc = (UInt32)((crc << 8) ^ (UInt32)(table[index]));
            }

            return crc;
        }

        public bool Verify(ReadOnlySpan<byte> input)
        {
            return Calculate(input) == 0x00;
        }

        private UInt32[] GenerateTable(UInt32 polinominal)
        {
            const UInt32 MsbMask = (UInt32)1 << (sizeof(UInt32) * 8 - 1);

            var table = new UInt32[0xFF + 1];
            for (var inputByte = 0x01; inputByte != 0xFF; inputByte++)
            {
                UInt32 crc = (UInt32)(inputByte << ((sizeof(UInt32) - 1)  * 8));
                for (int j = 8; j > 0; j--)
                {
                    if ((crc & MsbMask) != 0)
                    {
                        crc <<= 1;
                        crc ^= polinominal;
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }

                table[inputByte] = crc;
            }

            return table;
        }
    }
}
