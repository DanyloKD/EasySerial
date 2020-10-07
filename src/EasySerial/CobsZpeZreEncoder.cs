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
        
        private const int EMPTY_CHUNK_LENGTH = 1;
        
        private readonly byte[] buffer = new byte[GetMaxOutputLength(MAX_PACKET_SIZE)];

        private int readPos;
        private int writePos;
        private int chunkPos;

        private byte zeroesRunLength;
        private byte chunkLength;
        
        public byte[] Encode(in byte[] raw)
        {
            var inputLength = raw.Length;

            var maxOutputLength = GetMaxOutputLength(inputLength);
            if (maxOutputLength > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(raw), "Input is too long");
            }

            Reset();
            while (readPos < inputLength)
            {
                var nextByte = raw[readPos];
                if (nextByte != DELIMITER)
                {
                    ProcessNextByte(nextByte);
                }
                else
                {
                    ProcessNextDelimiter();
                }
            }

            if (zeroesRunLength == 0)
            {
                buffer[chunkPos] = chunkLength;
            }
            else if (zeroesRunLength == 1 && chunkLength != EMPTY_CHUNK_LENGTH)
            {
                CloseZeroChunk();
                writePos++;
                buffer[chunkPos] = chunkLength;
            }
            else
            {
                ProcessZeroesRun();
            }

            buffer[writePos] = DELIMITER;

            var outputSize = writePos + 1;
            var output = new byte[outputSize];
            Array.Copy(buffer, output, outputSize);

            return output;
        }

        private void ProcessNextByte(byte nextByte)
        {
            if (zeroesRunLength > 0)
            {
                ProcessZeroesRun();
                writePos++;
            }
            else if (chunkLength == MAX_CHUNK_LENGTH)
            {
                buffer[chunkPos] = MAX_CHUNK_LENGTH;
                CloseChunk();
                writePos++;
            }
            else
            {
                buffer[writePos] = nextByte;
                chunkLength++;
                writePos++;
                readPos++;
            }
        }

        private void ProcessNextDelimiter()
        {
            if (chunkLength == EMPTY_CHUNK_LENGTH)
            {
                if (zeroesRunLength < ZERO_RUN_LENGTH)
                {
                    ProcessNextZero();
                }
                else
                {
                    CloseZeroRunChunk();
                    writePos++;
                }
            }
            else if (chunkLength <= ZERO_PAIR_LENGTH)
            {
                if (zeroesRunLength < ZERO_PAIR_COUNT)
                {
                    ProcessNextZero();
                }
                else
                {
                    CloseZeroPairChunk();
                    writePos++;
                }
            }
            else
            {
                CloseZeroChunk();
                writePos++;
            }
        }

        private void Reset()
        {
            readPos = 0;
            writePos = 1;
            chunkPos = 0;

            zeroesRunLength = 0;
            chunkLength = EMPTY_CHUNK_LENGTH;
        }

        private void ProcessNextZero()
        {
            zeroesRunLength++;
            readPos++;
        }

        private void ProcessZeroesRun()
        {
            if (chunkLength == EMPTY_CHUNK_LENGTH)
            {
                CloseZeroRunChunk();
            }
            else if (zeroesRunLength == ZERO_PAIR_COUNT)
            {
                CloseZeroPairChunk();
            }
            else
            {
                CloseZeroChunk();
            }
        }

        private void CloseZeroRunChunk()
        {
            buffer[chunkPos] = (byte)(ZERO_RUN_MIN + zeroesRunLength);
            CloseChunk();
            zeroesRunLength = 0;
        }

        private void CloseZeroPairChunk()
        {
            buffer[chunkPos] = (byte)(ZERO_PAIR_MIN + (chunkLength - ZERO_PAIR_COUNT));
            CloseChunk();
            zeroesRunLength = 0;
        }

        private void CloseZeroChunk()
        {
            buffer[chunkPos] = chunkLength;
            CloseChunk();
            zeroesRunLength = 0;
        }

        private void CloseChunk()
        {
            chunkPos = writePos;
            chunkLength = EMPTY_CHUNK_LENGTH;
        }

        private static int GetMaxOutputLength(int inputLength)
        {
            return inputLength + inputLength % (MAX_CHUNK_LENGTH - 1) + 2;
        }
    }
}
