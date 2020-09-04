using System;

namespace EasySerial
{
    public class CobsEncoder
    {
        public const int MAX_PACKET_SIZE = 1 << 10;
        public const int DELIMITER = 0x00;
        
        private const int MAX_CHUNK_LENGTH = byte.MaxValue;

        private readonly byte[] buffer = new byte[GetMaxOutputLength(MAX_PACKET_SIZE)];

        public byte[] Encode(in byte[] raw)
        {
            var inputLength = raw.Length;

            var maxOutputLength = GetMaxOutputLength(inputLength);
            if (maxOutputLength > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(raw), "Input is too long");
            }

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
                        buffer[chunkPos] = MAX_CHUNK_LENGTH;
                        chunkPos = writePos;
                        chunkLength = 1;
                        writePos++;
                        // readPos++;
                    }
                    else
                    {
                        buffer[writePos] = raw[readPos];
                        chunkLength++;
                        writePos++;
                        readPos++;
                    }
                }
                else
                {
                    buffer[chunkPos] = chunkLength;
                    chunkPos = writePos;
                    chunkLength = 1;
                    writePos++;
                    readPos++;
                }
            }

            buffer[chunkPos] = chunkLength;
            buffer[writePos] = DELIMITER;

            var outputSize = writePos + 1;
            var output = new byte[outputSize];
            Array.Copy(buffer, output, outputSize);

            return output;
        }

        private static int GetMaxOutputLength(int inputLength)
        {
            return inputLength + inputLength % (MAX_CHUNK_LENGTH - 1) + 2;
        }
    }
}
