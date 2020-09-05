using System;

namespace EasySerial
{
    public class Crc16
    {
        public static int ChecksumLength = sizeof(UInt16);

        private readonly UInt16 polinominal;
        private readonly UInt16 init;

        private readonly UInt16[] table;

        public Crc16(UInt16 polinominal, UInt16 init)
        {
            this.polinominal = polinominal;
            this.init = init;
            this.table = GenerateTable(this.polinominal);
        }

        public UInt16 Calculate(ReadOnlySpan<byte> input)
        {
            UInt16 crc = this.init;
            foreach (var b in input)
            {
                byte index = (byte)(b ^ (crc >> 8));
                crc = (UInt16)((crc << 8) ^ (UInt16)(table[index]));
            }

            return crc;
        }

        public bool Verify(ReadOnlySpan<byte> input)
        {
            return Calculate(input) == 0x00;
        }

        private UInt16[] GenerateTable(UInt16 polinominal)
        {
            const UInt16 MsbMask = (UInt16)1 << (sizeof(UInt16) * 8 - 1);

            var table = new UInt16[0xFF + 1];
            for (var inputByte = 0x01; inputByte != 0xFF; inputByte++)
            {
                UInt16 crc = (UInt16)(inputByte << ((sizeof(UInt16) - 1)  * 8));
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
