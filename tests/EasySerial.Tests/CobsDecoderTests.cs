using System;
using System.Linq;
using Xunit;

namespace EasySerial.Tests
{
    public class CobsDecoderTests
    {
        [Fact]
        public void Decoder_CompletePacket_ParsedSuccessfully()
        {
            var input = new byte[] { 0x01, 0x01, 0x00 };
            var decoder = new CobsDecoder();

            bool result = default;
            byte[] output = null;
            foreach(var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.Equal(new byte[] { 0x00 }, output);
        }

        [Fact]
        public void Decoder_CompletePacketWithPrependDelimiter_ParsedSuccessfully()
        {
            var input = new byte[] { 0x00, 0x00, 0x01, 0x01, 0x00 };
            var decoder = new CobsDecoder();

            bool result = default;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.Equal(new byte[] { 0x00 }, output);
        }

        [Fact]
        public void Decoder_CompletePacketWithMultipleDelimiters_ParsedSuccessfully()
        {
            var input = new byte[] { 0x03, 0x01, 0x02, 0x04, 0x03, 0x04, 0x05, 0x02, 0x06, 0x00 };
            var decoder = new CobsDecoder();

            bool result = default;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.Equal(new byte[] { 0x01, 0x02, 0x00, 0x03, 0x04, 0x05, 0x00, 0x06 }, output);
        }

        [Fact]
        public void Decoder_LongPacketWithMultipleDelimiters_ParsedSuccessfully()
        {
            var input = new byte[512];
            input[0] = 128;
            Array.Fill<byte>(input, 0x04, 1, 127);
            input[128] = 255;
            Array.Fill<byte>(input, 0x42, 129, 254);
            input[383] = 128;
            Array.Fill<byte>(input, 0xAB, 384, 127);
            input[511] = 0;
            
            var decoder = new CobsDecoder();

            bool result = default;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.Equal(510, output.Length);

            Assert.Equal(2, output.Count(a => a == 0x00));
            Assert.Equal(127, output.Count(a => a == 0x04));
            Assert.Equal(254, output.Count(a => a == 0x42));
            Assert.Equal(127, output.Count(a => a == 0xAB));
        }

        [Fact]
        public void Decoder_DelimiterInInput_ResetsParser()
        {
            var input = new byte[32];
            Array.Fill<byte>(input, 0x04);
            input[0] = 0x1F;
            input[31] = 0x00;
            input[12] = 0x00;
            input[13] = 0x12;

            var decoder = new CobsDecoder();

            bool result = default;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.Equal(17, output.Length);
            Assert.Equal(17, output.Count(a => a == 0x04));
        }
    }
}
