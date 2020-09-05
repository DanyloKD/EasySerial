using System;
using Xunit;

namespace EasySerial.Tests
{
    public class Crc32Tests
    {
        [Fact]
        public void Calculate_Works()
        {
            var calcualtor = new Crc32(0x04C11DB7, 0xFFFFFFFF);
            var data = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            var crc = calcualtor.Calculate(data);

            Assert.Equal<UInt32>(0x81DA1A18, crc);
        }

        [Fact]
        public void Calculate_WithEmptyInput_ReturnsInit()
        {
            var calcualtor = new Crc32(0x04C11DB7, 0x00);
            var crc1 = calcualtor.Calculate(new byte[0]);
            var crc2 = calcualtor.Calculate(null);

            Assert.Equal<UInt32>(0, crc1);
            Assert.Equal<UInt32>(0, crc2);
        }

        [Fact]
        public void Verify_WithValidInput_Pass()
        {
            var calcualtor = new Crc32(0x04C11DB7, 0xFFFFFFFF);
            var data = new byte[] { 
                0xDE, 0xAD, 0xBE, 0xEF,
                0x81, 0xDA, 0x1A, 0x18
            };
            var result = calcualtor.Verify(data);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WithChecksumOnly_Pass()
        {
            var calcualtor = new Crc32(0x04C11DB7, 0xFFFFFFFF);
            var data = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
            var result = calcualtor.Verify(data);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WithInvalidChecksum_Fails()
        {
            var calcualtor = new Crc32(0x04C11DB7, 0xFFFFFFFF);
            var data = new byte[] {
                0xDE, 0xAD, 0xBE, 0xEF,
                0x11, 0xAA, 0x11, 0x11
                /* invalid, correct is 0x81, 0xDA, 0x1A, 0x18 */ 
            };
            var result = calcualtor.Verify(data);

            Assert.False(result);
        }
    }
}
