using System;
using System.Linq;
using Xunit;

namespace EasySerial.Tests
{
    public class CobsEncoderTests
    {
        [Fact]
        public void Encoder_ShortPacketWithoutDelimiter_PrependsLengthAndAppendsDelimiter()
        {
            var encoder = new CobsEncoder();
            var output = encoder.Encode(new byte[] { 0x01, 0x02, 0x03 });
            
            Assert.Equal(
                new byte[] { 0x04, 0x01, 0x02, 0x03, CobsEncoder.DELIMITER }, 
                output
            );
        }

        [Fact]
        public void Encoder_ShortPacketWithDelimiter_PrependsLengthAndReplacesDelimiters()
        {
            var encoder = new CobsEncoder();
            var output = encoder.Encode(new byte[] { 0x00 });

            Assert.Equal(
                new byte[] { 0x01, 0x01, CobsEncoder.DELIMITER }, 
                output
            );
        }

        [Fact]
        public void Encoder_ShortPacketWithMultipleDelimiters_PrependsLengthAndReplacesDelimiters()
        {
            var encoder = new CobsEncoder();
            var output = encoder.Encode(new byte[] { 0x00, 0x00 });

            Assert.Equal(
                new byte[] { 0x01, 0x01, 0x01, CobsEncoder.DELIMITER }, 
                output
            );
        }

        [Fact]
        public void Encoder_LongPacketWithoutDelimiters_InsertsSeparators()
        {
            var encoder = new CobsEncoder();

            var input = new byte[255];
            Array.Fill<byte>(input, 0x01);

            var output = encoder.Encode(input);

            Assert.Equal(0xFF, output[0]);
            Assert.Equal(0x02, output[0xFF]);
            Assert.Equal(
                input.Length, 
                output.Count(a => a == 0x01)
            );
            Assert.Equal(CobsEncoder.DELIMITER, output.Last());
        }

        [Fact]
        public void Encoder_LongPacketWitDelimiters_InsertsSeparators()
        {
            var encoder = new CobsEncoder();

            var input = new byte[255];
            Array.Fill<byte>(input, 0x01, 0, 50);
            input[51] = 0x00;
            Array.Fill<byte>(input, 0x01, 52, 50);
            input[102] = 0x00;
            Array.Fill<byte>(input, 0x01, 103, 50);
            input[153] = 0x00;
            Array.Fill<byte>(input, 0x01, 154, 101);

            var output = encoder.Encode(input);

            Assert.Equal(51, output[0]);
            Assert.Equal(51, output[52]);
            Assert.Equal(51, output[103]);
            Assert.Equal(102, output[154]);
            Assert.Equal(
                input.Length - 3, 
                output.Count(a => a == 0x01)
            );
            Assert.Equal(CobsEncoder.DELIMITER, output.Last());
        }
        
        [Fact]
        public void Encoder_WithMaximumLengthInput_Works()
        {
            var encoder = new CobsEncoder();
            var input = new byte[CobsEncoder.MAX_PACKET_SIZE];

            var output = encoder.Encode(input);

            Assert.True(output.Take(output.Length - 1).All(a => a == 0x01));
            Assert.Equal(CobsEncoder.DELIMITER, output.Last());
        }

        [Fact]
        public void Encoder_WithTooLongInput_Throws()
        {
            var encoder = new CobsEncoder();
            var input = new byte[CobsEncoder.MAX_PACKET_SIZE + 1];

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                encoder.Encode(input);
            });
        }
    }
}
