﻿using System;

namespace EasySerial
{
    public class CobsZpeZreDecoder
    {
        private readonly byte[] buffer = new byte[CobsZpeZreEncoder.MAX_PACKET_SIZE];

        private bool hasStart;

        private bool hasDelimiter;
        private int chunkLength;
        private int zeroesRunLength;
        private int writePos;

        public bool NextByte(in byte input, out byte[] output)
        {
            output = null;

            if (!hasStart)
            {
                ProcessFirstControlByte(input);
                return false;
            }

            if (writePos < chunkLength)
            {
                ProcessNextContentByte(input);
                return false;
            }

            AppendRunningZeroes();
            if (input != CobsZpeZreEncoder.DELIMITER)
            {
                ProcessStartOfNextChunk(input);

                return false;
            }
            else
            {
                output = CopyDecodedData();
                
                return true;
            }
        }

        private void ProcessFirstControlByte(byte input)
        {
            if (input != CobsZpeZreEncoder.DELIMITER)
            {
                Reset();
                NextState(input);

                hasStart = true;
            }
        }

        private void ProcessNextContentByte(byte input)
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
        }

        private void AppendDelimiterIfRequired()
        {
            if (hasDelimiter)
            {
                this.AppendZero();
            }
        }

        private void Reset()
        {
            hasDelimiter = false;
            chunkLength = 0;
            zeroesRunLength = 0;
            writePos = 0;
        }

        private void NextState(byte input)
        {
            if (input <= CobsZpeZreEncoder.MAX_CHUNK_LENGTH)
            {
                hasDelimiter = input != CobsZpeZreEncoder.MAX_CHUNK_LENGTH;
                chunkLength += input - 1;
                zeroesRunLength = 0;
            }
            else if (CobsZpeZreEncoder.ZERO_RUN_MIN < input
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
        }

        private void AppendZero()
        {
            buffer[writePos] = CobsZpeZreEncoder.DELIMITER;
            writePos++;
            chunkLength++;
        }

        private void AppendRunningZeroes()
        {
            while (zeroesRunLength > 0)
            {
                this.AppendZero();
                zeroesRunLength--;
            }
        }

        private void ProcessStartOfNextChunk(byte input)
        {
            AppendDelimiterIfRequired();
            NextState(input);
            // prevent buffer overflow: discard data and start over
            ResetDecoderIfChunkTooLong();
        }

        private void ResetDecoderIfChunkTooLong()
        {
            if (chunkLength > CobsZpeZreEncoder.MAX_PACKET_SIZE)
            {
                hasStart = false;
            }
        }

        private byte[] CopyDecodedData()
        {
            var output = new byte[writePos];
            Array.Copy(buffer, output, writePos);

            hasStart = false;

            return output;
        }

    }
}
