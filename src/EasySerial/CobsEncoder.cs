using System;

namespace EasySerial
{
    public class CobsEncoder
    {
        private const int DELIMITER = 0x00;
        private const int MAX_CHUNK_LENGTH = 0xFF;

        public int Encode(in byte[] raw, out byte[] output)
        {
            var inputLength = raw.Length;

            var outputLength = GetMaxOutputLength(inputLength);
            output = new byte[outputLength];

            var readPos = 0;
            var writePos = 1;
            var chunkPos = 0;

            byte chunkLenth = 1;

            while (readPos < inputLength)
            {
                if (raw[readPos] != DELIMITER)
                {
                    if (chunkLenth == MAX_CHUNK_LENGTH)
                    {
                        output[chunkPos] = MAX_CHUNK_LENGTH;
                        chunkPos = writePos;
                        chunkLenth = 1;
                        writePos++;
                        // readPos++;
                    }
                    else
                    {
                        output[writePos] = raw[readPos];
                        chunkLenth++;
                        writePos++;
                        readPos++;
                    }
                }
                else
                {
                    output[chunkPos] = chunkLenth;
                    chunkPos = writePos;
                    chunkLenth = 1;
                    writePos++;
                    readPos++;
                }
            }

            output[chunkPos] = chunkLenth;
            output[writePos] = DELIMITER;

            return writePos + 1;
        }

        private static int GetMaxOutputLength(int inputLength)
        {
            return inputLength + inputLength % (MAX_CHUNK_LENGTH - 1) + 2;
        }
    }
}
