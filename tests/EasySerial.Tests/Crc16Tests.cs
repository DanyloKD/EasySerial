using System;
using Xunit;

namespace EasySerial.Tests
{
    public class Crc16Tests
    {
        [Fact]
        public void Calculate_Works()
        {
            var calcualtor = new Crc16(0x1021, 0xFFFF);
            var data = new byte[] { 0xC2 };
            var crc = calcualtor.Calculate(data);

            Assert.Equal<UInt16>(0x18FE, crc);
        }

        [Fact]
        public void Calculate_WithEmptyInput_ReturnsInit()
        {
            var calcualtor = new Crc16(0x1021, 0x0000);
            var crc1 = calcualtor.Calculate(new byte[0]);
            var crc2 = calcualtor.Calculate(null);

            Assert.Equal<UInt16>(0, crc1);
            Assert.Equal<UInt16>(0, crc2);
        }

        [Fact]
        public void Verify_WithValidInput_Pass()
        {
            var calcualtor = new Crc16(0x1021, 0xFFFF);
            var data = new byte[] { 0xC2, 0x18, 0xFE };
            var result = calcualtor.Verify(data);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WithChecksumOnly_Pass()
        {
            var calcualtor = new Crc16(0x1021, 0xFFFF);
            var data = new byte[] { 0xFF, 0xFF };
            var result = calcualtor.Verify(data);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WithInvalidChecksum_Fails()
        {
            var calcualtor = new Crc16(0x1021, 0xFFFF);
            var data = new byte[] { 0xC2, 0x08, 0x0E /* invalid, correct is 0x18, 0xFE */ };
            var result = calcualtor.Verify(data);

            Assert.False(result);
        }
    }
}
