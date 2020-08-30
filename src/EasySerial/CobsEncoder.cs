using System;

namespace EasySerial
{
    public class CobsEncoder
    {
        public const int DELIMITER = 0x00;
        
        private const int MAX_CHUNK_LENGTH = 0xFF;

        public int Encode(in byte[] raw, out byte[] output)
        {
            var inputLength = raw.Length;

            var outputLength = GetMaxOutputLength(inputLength);
            output = new byte[outputLength];

            var readPos = 0;
            var writePos = 1;
            var chunkPos = 0;

            byte chunkLength = 1;

            while (readPos < inputLength)
            {
                if (raw[readPos] != DELIMITER)
                {
                    if (chunkLength == MAX_CHUNK_LENGTH)
                    {
                        output[chunkPos] = MAX_CHUNK_LENGTH;
                        chunkPos = writePos;
                        chunkLength = 1;
                        writePos++;
                        // readPos++;
                    }
                    else
                    {
                        output[writePos] = raw[readPos];
                        chunkLength++;
                        writePos++;
                        readPos++;
                    }
                }
                else
                {
                    output[chunkPos] = chunkLength;
                    chunkPos = writePos;
                    chunkLength = 1;
                    writePos++;
                    readPos++;
                }
            }

            output[chunkPos] = chunkLength;
            output[writePos] = DELIMITER;

            return writePos + 1;
        }

        private static int GetMaxOutputLength(int inputLength)
        {
            return inputLength + inputLength % (MAX_CHUNK_LENGTH - 1) + 2;
        }
    }
}
