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
                new byte[] { 0x04, 0x01, 0x02, 0x03, CobsEncoder.DELIMITER },
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
            Assert.Equal(CobsEncoder.DELIMITER, output.Last());
        }

        [Fact]
        public void Encoder_ShortPacketWithSingleDelimiter_InsertsSeparators()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x01, 0x02, 0x00, 0x03, 0x04 };
            var output = encoder.Encode(input);

            Assert.Equal(
                new byte[] { 0x03, 0x01, 0x02, 0x03, 0x03, 0x04, CobsEncoder.DELIMITER },
                output
            );
        }

        [Fact]
        public void Encoder_ShortPacketWithTwoTrailingDelimiters_InsertsSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[] { 0x01, 0x02, 0x03, 0x00, 0x00 };
            var output = encoder.Encode(input);

            Assert.Equal(0xE3, output[0]);
        }

        [Fact]
        public void Encoder_LongPacketWithTwoTrailingDelimiters_InsertsSpecialLength()
        {
            var encoder = new CobsZpeZreEncoder();

            var input = new byte[32];
            Array.Fill<byte>(input, 0x01, 0, 30);
            Array.Fill<byte>(input, 0x00, 30, 2);

            var output = encoder.Encode(input);

            Assert.Equal(0xFE, output[0]);
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

            Assert.Equal(0xDF, output[0]);
            Assert.Equal(0x00, output[1]);
            Assert.Equal(2, output.Length);
        }

    }
}
