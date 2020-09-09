using System;
using System.Linq;
using Xunit;

namespace EasySerial.Tests
{
    public class CobsZpeZreDecoderTests
    {
        [Fact]
        public void Decoder_ShortRegularPacket_Works()
        {
            var decoder = new CobsZpeZreDecoder();
            var input = new byte[] { 0x03, 0x01, 0x02, 0x00 };
            
            bool result = false;
            byte[] output = null;
            foreach(var b in input)
            {
                result = decoder.NextByte(b, out output);    
            }

            Assert.True(result);
            Assert.NotNull(output);
            Assert.Equal(new byte[] { 0x01, 0x02 }, output);
        }

        [Fact]
        public void Decoder_LongRegularPacket_Works()
        {
            var decoder = new CobsZpeZreDecoder();
            var input = new byte[0xD5];
            input[0] = 0xD0;
            Array.Fill<byte>(input, 0x01, 1, 0xD0 - 1);
            input[0xD0] = 4;
            Array.Fill<byte>(input, 0x01, 0xD0 + 1, 3);
            input[0xD4] = 0x00;

            bool result = false;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.NotNull(output);
            Assert.Equal(0xD0 - 1 + 3, output.Length);
            Assert.Equal(0xD0 - 1 + 3, output.Count(b => b == 0x01));
        }

        [Fact]
        public void Decoder_ZeroRunEncoded_Works()
        {
            var decoder = new CobsZpeZreDecoder();
            var input = new byte[] { 0xD7, 0x00 };

            bool result = false;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.NotNull(output);
            Assert.Equal(7, output.Length);
            Assert.True(output.All(b => b == 0x00));
        }

        [Fact]
        public void Decoder_ZeroEndEncoded_Works()
        {
            var decoder = new CobsZpeZreDecoder();
            var input = new byte[] { 0xE5, 0x01, 0x02, 0x03, 0x04, 0x00 };

            bool result = false;
            byte[] output = null;
            foreach (var b in input)
            {
                result = decoder.NextByte(b, out output);
            }

            Assert.True(result);
            Assert.NotNull(output);
            Assert.Equal(6, output.Length);
            Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x00, 0x00 }, output);
        }
    }
}
