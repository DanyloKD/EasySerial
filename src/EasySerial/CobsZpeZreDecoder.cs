using System;

namespace EasySerial
{
    public class CobsZpeZreDecoder
    {
        private readonly byte[] buffer = new byte[CobsEncoder.MAX_PACKET_SIZE];

        private bool hasStart;
        private int writePos;

        private bool hasDelimiter;
        private int chunkLength;
        private int zeroesRunLength;

        public bool NextByte(in byte input, out byte[] output)
        {
            output = null;

            if (!hasStart)
            {
                if (input != CobsZpeZreEncoder.DELIMITER)
                {
                    if (input <= CobsZpeZreEncoder.MAX_CHUNK_LENGTH) 
                    {
                        hasDelimiter = input != CobsZpeZreEncoder.MAX_CHUNK_LENGTH;
                        chunkLength = input - 1;
                        zeroesRunLength = 0;

                        writePos = 0;
                        hasStart = true;
                    }
                    else if (CobsZpeZreEncoder.ZERO_RUN_MIN < input  
                        && input < CobsZpeZreEncoder.ZERO_RUN_MAX
                    ){
                        hasDelimiter = false;
                        chunkLength = 0;
                        zeroesRunLength = input - CobsZpeZreEncoder.ZERO_RUN_MIN;

                        writePos = 0;
                        hasStart = true;
                    }
                    else if (CobsZpeZreEncoder.ZERO_PAIR_MIN <= input
                        && input < CobsZpeZreEncoder.ZERO_PAIR_MAX
                    ){
                        hasDelimiter = false;
                        chunkLength = input - CobsZpeZreEncoder.ZERO_PAIR_MIN - 1;
                        zeroesRunLength = CobsZpeZreEncoder.ZERO_PAIR_COUNT;

                        writePos = 0;
                        hasStart = true;
                    }              
                }

                return false;
            }

            if (writePos < chunkLength)
            {
                if (input != CobsZpeZreEncoder.DELIMITER)
                {
                    buffer[writePos] = input;
                    writePos++;
                }
                else
                {
                    // unexpected packet delimiter - reset and read next packet 
                    hasStart = false;
                }

                return false;
            }
            else if (writePos == chunkLength)
            {
                if (zeroesRunLength > 0)
                {
                    while (zeroesRunLength > 0)
                    {
                        buffer[writePos] = CobsZpeZreEncoder.DELIMITER;
                        writePos++;
                        zeroesRunLength--;
                    }
                }

                if (input != CobsZpeZreEncoder.DELIMITER)
                {
                    if (hasDelimiter)
                    {
                        buffer[writePos] = CobsZpeZreEncoder.DELIMITER;
                        writePos++;

                        chunkLength++;
                    }

                    if (input < CobsZpeZreEncoder.MAX_CHUNK_LENGTH)
                    {
                        hasDelimiter = input != CobsZpeZreEncoder.MAX_CHUNK_LENGTH;
                        chunkLength += input - 1;
                        zeroesRunLength = 0;
                    }
                    else if (CobsZpeZreEncoder.ZERO_RUN_MIN <= input
                        && input < CobsZpeZreEncoder.ZERO_RUN_MAX
                    )
                    {
                        hasDelimiter = false;
                        chunkLength += 0;
                        zeroesRunLength = input - CobsZpeZreEncoder.ZERO_RUN_MIN;
                    }
                    else if (CobsZpeZreEncoder.ZERO_PAIR_MIN <= input
                        && input < CobsZpeZreEncoder.ZERO_PAIR_MAX
                    )
                    {
                        hasDelimiter = false;
                        chunkLength += input - CobsZpeZreEncoder.ZERO_PAIR_MIN - 1;
                        zeroesRunLength = CobsZpeZreEncoder.ZERO_PAIR_COUNT;
                    }

                    if (chunkLength > CobsZpeZreEncoder.MAX_PACKET_SIZE)
                    {
                        // prevent buffer overflow: discard data and start over
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

            // Shall never reach here
            throw new InvalidOperationException();
        }
    }
}
