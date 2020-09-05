using Xunit;

namespace EasySerial.Tests
{
    public class Crc8Tests
    {
        [Fact]
        public void Calculate_Works()
        {
            var calcualtor = new Crc8(0x1D, 0x00);
            var data = new byte[] { 0xC2 };
            var crc = calcualtor.Calculate(data);

            Assert.Equal<byte>(0x0F, crc);
        }

        [Fact]
        public void Calculate_WithEmptyInput_ReturnsInit()
        {
            var calcualtor = new Crc8(0x1D, 0x00);
            var crc1 = calcualtor.Calculate(new byte[0]);
            var crc2 = calcualtor.Calculate(null);

            Assert.Equal<byte>(0, crc1);
            Assert.Equal<byte>(0, crc2);
        }

        [Fact]
        public void Verify_WithValidInput_Pass()
        {
            var calcualtor = new Crc8(0x1D, 0x00);
            var data = new byte[] { 0xC2, 0x0F };
            var result = calcualtor.Verify(data);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WithChecksumOnly_Pass()
        {
            var calcualtor = new Crc8(0x42, 0x00);
            var data = new byte[] { 0x00 };
            var result = calcualtor.Verify(data);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WithInvalidChecksum_Fails()
        {
            var calcualtor = new Crc8(0x1D, 0x00);
            var data = new byte[] { 0xC2, 0xBE /* invalid, correct is 0x0F */ };
            var result = calcualtor.Verify(data);

            Assert.False(result);
        }
    }
}
