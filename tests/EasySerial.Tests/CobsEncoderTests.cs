using System;
using Xunit;

namespace EasySerial.Tests
{
    public class CobsEncoderTests
    {
        [Fact]
        public void Encoder_WithShortPacketWithoutDelimiter_PrependsLengthAndAppendsDelimiter()
        {
            var encoder = new CobsEncoder();
            var output = encoder.Encode(new byte[] { 0x01, 0x02, 0x03 });
            
            Assert.Equal(new byte[] { 0x04, 0x01, 0x02, 0x03, 0x00 }, output);
        }

        [Fact]
        public void Encoder_PacketWithDelimiter_PrependsLengthAndReplacesDelimiters()
        {
            var encoder = new CobsEncoder();
            var output = encoder.Encode(new byte[] { 0x00 });

            Assert.Equal(new byte[] { 0x01, 0x01, 0x00 }, output);
        }

        [Fact]
        public void Encoder_PacketWithDelimiter2_PrependsLengthAndReplacesDelimiters()
        {
            var encoder = new CobsEncoder();
            var output = encoder.Encode(new byte[] { 0x00, 0x00 });

            Assert.Equal(new byte[] { 0x01, 0x01, 0x01, 0x00 }, output);
        }
    }
}
