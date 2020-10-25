using System;

namespace EasySerial
{
    public class CobsDecoder
    {
        private readonly byte[] buffer = new byte[CobsEncoder.MAX_PACKET_SIZE];

        private bool hasStart;
        private int writePos;

        private bool hasDelimiter;
        private int chunkLength;

        public byte[] NextByte(in byte input)
        {
            if (!hasStart)
            {
                if (input != CobsEncoder.DELIMITER)
                {
                    hasDelimiter = input != CobsEncoder.MAX_CHUNK_LENGTH;
                    chunkLength = input - 1;
                    
                    writePos = 0;
                    hasStart = true;
                }

                return null;
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
                    // unexpected packet delimiter - reset and read next packet 
                    hasStart = false;
                }

                return null;
            }

            // (writePos == chunkLength)
            if (input != CobsEncoder.DELIMITER)
            {
                if (hasDelimiter)
                {
                    buffer[writePos] = CobsEncoder.DELIMITER;
                    writePos++;
                    
                    hasDelimiter = input != CobsEncoder.MAX_CHUNK_LENGTH;
                    chunkLength += input;
                }
                else 
                {
                    hasDelimiter = input != CobsEncoder.MAX_CHUNK_LENGTH;
                    chunkLength += input - 1;
                }

                if (chunkLength > CobsEncoder.MAX_PACKET_SIZE)
                {
                    // prevent buffer overflow: discard data and start over
                    hasStart = false;
                }

                return null; 
            }
            else 
            {
                var output = new byte[writePos];

                Array.Copy(buffer, output, writePos);
                hasStart = false;

                return output;
            }
        }

    }
}
