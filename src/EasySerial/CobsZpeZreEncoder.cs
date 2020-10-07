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

        public const int ZERO_PAIR_MIN = 0xE0;
        public const int ZERO_PAIR_MAX = 0xFE;
        public const int ZERO_PAIR_COUNT = 2;

        private const int ZERO_RUN_LENGTH = ZERO_RUN_MAX - ZERO_RUN_MIN;
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
            
            if (!HasEnoughtBufferToEncode(inputLength))
            {
                throw new ArgumentOutOfRangeException(nameof(raw), "Input is too long");
            }

            Reset();
            while (readPos < inputLength)
            {
                var nextByte = raw[readPos];
                if (nextByte != DELIMITER)
                {
                    ProcessRunningZeroesIfAny();
                    ProcessNextByte(nextByte);
                }
                else
                {
                    ProcessNextDelimiter();
                }
            }

            CloseLastChunk();
            ClosePacketWithDelimiter();

            return CopyEncodedData();
        }
        
        private bool HasEnoughtBufferToEncode(int inputLength)
        {
            return buffer.Length >= GetMaxOutputLength(inputLength);
        }

        private void ProcessNextByte(byte nextByte)
        {
            if (chunkLength == MAX_CHUNK_LENGTH)
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

        private void ProcessRunningZeroesIfAny()
        {
            if (zeroesRunLength > 0)
            {
                ProcessZeroesRun();
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
            if (zeroesRunLength < ZERO_PAIR_COUNT)
            {
                CloseZeroChunk();
            }
            else if (zeroesRunLength == ZERO_PAIR_COUNT)
            {
                CloseZeroPairChunk();
            }
            else if (chunkLength == EMPTY_CHUNK_LENGTH)
            {
                CloseZeroRunChunk();
            }            
            else
            {
                CloseZeroChunk();
            }
        }

        private void CloseZeroRunChunk()
        {
            buffer[chunkPos] = (byte)(ZERO_RUN_MIN + zeroesRunLength);
            zeroesRunLength = 0;
            CloseChunk();
        }

        private void CloseZeroPairChunk()
        {
            buffer[chunkPos] = (byte)(ZERO_PAIR_MIN + (chunkLength - ZERO_PAIR_COUNT));
            zeroesRunLength = 0;
            CloseChunk();
        }

        private void CloseZeroChunk()
        {
            buffer[chunkPos] = chunkLength;
            zeroesRunLength = 0;
            CloseChunk();
        }

        private void CloseChunk()
        {
            chunkPos = writePos;
            chunkLength = EMPTY_CHUNK_LENGTH;
        }

        private void CloseLastChunk()
        {
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
        }

        private void ClosePacketWithDelimiter()
        {
            buffer[writePos] = DELIMITER;
            writePos++;
        }

        private byte[] CopyEncodedData()
        {
            var output = new byte[writePos];
            Array.Copy(buffer, output, writePos);
            return output;
        }

        private static int GetMaxOutputLength(int inputLength)
        {
            return inputLength + inputLength % (MAX_CHUNK_LENGTH - 1) + 2;
        }
    }
}
