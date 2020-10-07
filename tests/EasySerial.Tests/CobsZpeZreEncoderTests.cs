using System;
using System.Linq;
using Xunit;

namespace EasySerial.Tests
{
    public class CobsZpeZreEncoderTests
    {
        [Fact]
        public void Encoder_ShortPacketWithoutDelimiter_PrependsLengthAndAppendsDelimiter()
        {
            var encoder = new CobsZpeZreEncoder();
            var output = encoder.Encode(new byte[] { 0x01, 0x02, 0x03 });

            Assert.Equal(
                new byte[] { 0x04, 0x01, 0x02, 0x03, CobsZpeZreEncoder.DELIMITER },
                output
            );
        }

        [Fact]
        public void Encoder_LongPacketWithoutDelimiters_InsertsSeparators()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[255];
            Array.Fill<byte>(input, 0x01);

            var output = encoder.Encode(input);

            Assert.Equal(0xD0, output[0]);
            Assert.Equal(0x31, output[0xD0]);
            Assert.Equal(
                input.Length,
                output.Count(a => a == 0x01)
            );
            Assert.Equal(CobsZpeZreEncoder.DELIMITER, output.Last());
        }

        [Fact]
        public void Encoder_ShortPacketWithSingleDelimiter_InsertsSeparators()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x01, 0x02, 0x00, 0x03, 0x04 };
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0x03, 0x01, 0x02, 0x03, 0x03, 0x04, CobsZpeZreEncoder.DELIMITER },
                output
            );
        }

        [Fact]
        public void Encoder_ShortPacketEndsWithZero_InsertsSeparators()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x01, 0x00 };
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0x02, 0x01, 0x01, CobsZpeZreEncoder.DELIMITER },
                output
            );
        }

        [Fact]
        public void Encoder_ZeroOne_EncodedSimple()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x00, 0x01 };
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0xD1, 0x02, 0x01, CobsZpeZreEncoder.DELIMITER },
                output
            );
        }

        [Fact]
        public void Encoder_ShortPacketWithTwoTrailingDelimiters_InsertsSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x01, 0x02, 0x03, 0x00, 0x00 };
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0xE2, 0x01, 0x02, 0x03, CobsZpeZreEncoder.DELIMITER }, 
                output
            );
        }

        [Fact]
        public void Encoder_LongPacketWithTwoTrailingDelimiters_InsertsSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[32];
            Array.Fill<byte>(input, 0x01, 0, 30);
            Array.Fill<byte>(input, 0x00, 30, 2);

            var output = encoder.Encode(input);

            Assert.Equal(0xFD, output[0]);
            Assert.Equal(0x00, output[31]);
            Assert.Equal(
                30,
                output.Count(a => a == 0x01)
            );
        }

        [Fact]
        public void Encoder_DelimitersRun_ReplacedWithSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[15];
            var output = encoder.Encode(input);

            Assert.Equal(2, output.Length);
            Assert.Equal(0xDF, output[0]);
            Assert.Equal(0x00, output[1]);
        }

        [Fact]
        public void Encoder_ByteAndDelimitersRun_ReplacedWithSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[15];
            input[0] = 0x01;
            
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0xE0, 0x01, 0xDC, 0x00 },
                output
            );
        }

        [Fact]
        public void Encoder_SingleDelimiter_ReplacedWithSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x00 };
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0xD1, 0x00 },
                output
            );
        }

        [Fact]
        public void Encoder_WithZeroesRun_CompressZeroes()
        {
            var encoder = new CobsZpeZreEncoder();
            var input = new byte[30];
            var output = encoder.Encode(input);

            Assert.Equal(new byte[] { 0xDF, 0xDF, 0x00 }, output);
        }

        [Fact]
        public void Encoder_WithZeroPair_CompressZeroes()
        {
            var encoder = new CobsZpeZreEncoder();
            var input = new byte[] { 0x42, 0x00, 0x00};
            var output = encoder.Encode(input);

            Assert.Equal(new byte[] { 0xE0, 0x42, 0x00 }, output);
        }

        [Fact]
        public void Encoder_TwoBlocksZeroPairEnd_CompressZeroes()
        {
            var encoder = new CobsZpeZreEncoder();
            var input = new byte[64];
            Array.Fill<byte>(input, 0x42, 0, 30);
            Array.Fill<byte>(input, 0x42, 32, 30);

            var output = encoder.Encode(input);

            Assert.Equal(63, output.Length);
            Assert.Equal(60, output.Count(a => a == 0x42));
            Assert.Equal(0xFD, output[0]);
            Assert.Equal(0xFD, output[31]);
            Assert.Equal(CobsZpeZreEncoder.DELIMITER, output.Last());
        }

        [Fact]
        public void Encoder_LongPacketWithDataExceedZeroPairCompressionLimit_DontCompressZeroPair()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[33];
            Array.Fill<byte>(input, 0x42, 0, 31);

            var output = encoder.Encode(input);

            Assert.Equal(34, output.Length);
            Assert.Equal(32, output[0]);
            Assert.Equal(0xD2, output[32]);
            Assert.Equal(CobsZpeZreEncoder.DELIMITER, output.Last());
            Assert.Equal(31, output.Count(a => a == 0x42));
        }

        [Fact]
        public void Encoder_WithTooLongInput_Throws()
        {
            var encoder = new CobsZpeZreEncoder();
            var input = new byte[CobsZpeZreEncoder.MAX_PACKET_SIZE + 1];
            Array.Fill<byte>(input, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                encoder.Encode(input);
            });
        }

    }
}
