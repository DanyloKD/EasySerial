using System;

namespace EasySerial
{
    public class CobsEncoder
    {
        private const int DELIMITER = 0x00;
        private const int MAX_CHUNK_LENGTH = 0xFF;

        public byte[] Encode(in byte[] raw)
        {
            var output = new byte[raw.Length + 2];
            
            var readPos = 0;
            var writePos = 1;
            var chunkPos = 0;

            byte chunkLenth = 1;
            
            while (readPos < raw.Length)
            {
                if (raw[readPos] != DELIMITER)
                {
                    if (chunkLenth == MAX_CHUNK_LENGTH)
                    {
                        output[chunkPos] = MAX_CHUNK_LENGTH;
                        chunkPos = writePos;
                        chunkLenth = 1;
                        writePos++;
                    }

                    output[writePos] = raw[readPos];
                    writePos++;
                    readPos++;
                    chunkLenth++;
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

            return output;
        }

    }
}
