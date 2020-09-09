using System;

namespace EasySerial
{
    public class CobsZpeZreEncoder
    {
        public const int MAX_PACKET_SIZE = 1 << 10;
        public const int DELIMITER = 0x00;

        public const int MAX_CHUNK_LENGTH = ZERO_RUN_MIN;

        public const int ZERO_RUN_MIN = 0xD0;
        public const int ZERO_RUN_MAX = 0xDF;
        private const int ZERO_RUN_LENGTH = ZERO_RUN_MAX - ZERO_RUN_MIN;

        public const int ZERO_PAIR_MIN = 0xE0;
        public const int ZERO_PAIR_MAX = 0xFE;
        public const int ZERO_PAIR_COUNT = 2;
        private const int ZERO_PAIR_LENGTH = ZERO_PAIR_MAX - ZERO_PAIR_MIN + 1;
        
        
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

            var delimitersInRun = 0;

            byte chunkLength = 1;

            while (readPos < inputLength)
            {
                if (raw[readPos] != DELIMITER)
                {
                    if (delimitersInRun > 0)
                    {
                        buffer[chunkPos] = chunkLength;
                        delimitersInRun = 0;
                        chunkPos = writePos;
                        chunkLength = 1;
                        writePos++;
                        //readPos++;
                    }
                    else if (chunkLength == MAX_CHUNK_LENGTH)
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
                    if (chunkLength == 1)
                    {
                        if (delimitersInRun < ZERO_RUN_LENGTH)
                        {
                            delimitersInRun++;
                            readPos++;
                        }
                        else
                        {
                            buffer[chunkPos] = (byte)(ZERO_RUN_MIN + delimitersInRun);
                            chunkPos = writePos;
                            writePos++;
                            delimitersInRun = 0;
                            chunkLength = 1;
                        }
                    }
                    else if (chunkLength <= ZERO_PAIR_LENGTH)
                    {
                        if (delimitersInRun < ZERO_PAIR_COUNT)
                        {
                            delimitersInRun++;
                            readPos++;
                        }
                        else
                        {
                            buffer[chunkPos] = (byte)(ZERO_PAIR_MIN + (chunkLength - 1));
                            chunkPos = writePos; 
                            writePos++;
                            delimitersInRun = 0;
                            chunkLength = 1;
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
            }

            if (delimitersInRun > 0)
            {
                if (delimitersInRun == ZERO_PAIR_COUNT)
                {
                    buffer[chunkPos] = (byte)(ZERO_PAIR_MIN + (chunkLength - 1));
                }
                else
                {
                    buffer[chunkPos] = (byte)(ZERO_RUN_MIN + delimitersInRun);
                }
            }
            else
            {
                buffer[chunkPos] = chunkLength;
            }

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
