using System;

namespace EasySerial
{
    public class CobsDecoder
    {
        private const int MAX_PACKET_SIZE = 1 << 12;
        private readonly byte[] buffer = new byte[MAX_PACKET_SIZE];

        private bool hasStart;
        private int writePos;
        private int chunkLength;

        public bool NextByte(in byte input, out byte[] output)
        {
            output = null;

            if (!hasStart)
            {
                if (input != CobsEncoder.DELIMITER)
                {
                    chunkLength = input - 1;
                    writePos = 0;
                    hasStart = true;
                }

                return false;
            }

            if (writePos < chunkLength)
            {
                if (input != CobsEncoder.DELIMITER)
                {
                    buffer[writePos] = input;
                    writePos++; 
                }
                else 
                {
                    hasStart = false;
                }

                return false;
            }
            else if (writePos == chunkLength)
            {
                if (input != CobsEncoder.DELIMITER)
                {
                    buffer[writePos] = CobsEncoder.DELIMITER;
                    writePos++;
                    chunkLength += input;

                    if (chunkLength > MAX_PACKET_SIZE)
                    {
                        hasStart = false;
                    }

                    return false; 
                }
                else 
                {
                    output = new byte[writePos];

                    Array.Copy(buffer, output, writePos);
                    hasStart = false;

                    return true;
                }
            }
            else 
            {
                // Shall never reach here
                throw new InvalidOperationException();
            }
        }
    }
}
