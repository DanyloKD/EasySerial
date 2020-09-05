using System;

namespace EasySerial
{
    public class Crc8 
    {
        public static int ChecksumLength = sizeof(byte);

        private readonly byte polinominal;
        private readonly byte init;

        private readonly byte[] table;
        
        public Crc8(byte polinominal, byte init)
        {
            this.polinominal = polinominal;
            this.init = init;

            this.table = this.GenerateTable(this.polinominal);
        }

        public byte Calculate(ReadOnlySpan<byte> input)
        {
            byte crc = this.init;
            foreach (var b in input)
            {
                byte index = (byte)(b ^ crc);
                crc = table[index];
            }

            return crc;
        }

        public bool Verify(ReadOnlySpan<byte> input)
        {
            return Calculate(input) == 0x00;
        }

        private byte[] GenerateTable(byte polinominal)
        {
            const byte MsbMask = 1 << (sizeof(byte) * 8 - 1);
            
            var table = new byte[0xFF + 1];
            for (var inputByte = 0x01; inputByte != 0xFF; inputByte++)
            {
                // T crc = (T)(inputByte << ((sizeof(T) - 1)  * 8));
                byte crc = (byte)inputByte;
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
