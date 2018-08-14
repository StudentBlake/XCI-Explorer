using System;
using FluentAssertions;
using NUnit.Framework;
using XCI.Explorer.Helpers;

namespace XCI.UnitTests
{
    [TestFixture]
    public class UtilTests
    {
        [TestCase(248, "2GB")]
        [TestCase(240, "4GB")]
        [TestCase(224, "8GB")]
        [TestCase(225, "16GB")]
        [TestCase(226, "32GB")]
        public void GetCapacity_ValidCapacity(int givenId, string expectedCapacity)
        {
            var result = Util.GetCapacity(givenId);
            result.Should().Be(expectedCapacity);
        }

        [Test]
        public void GetCapacity_InvalidCapacity()
        {
            var result = Util.GetCapacity(0);
            result.Should().Be("Unrecognized Size");
        }

        [TestCase(0, "MasterKey0 (1.0.0-2.3.0)")]
        [TestCase(1, "MasterKey0 (1.0.0-2.3.0)")]
        [TestCase(2, "MasterKey1 (3.0.0)")]
        [TestCase(3, "MasterKey2 (3.0.1-3.0.2)")]
        [TestCase(4, "MasterKey3 (4.0.0-4.1.0)")]
        [TestCase(5, "MasterKey4 (5.0.0+)")]
        [TestCase(6, "MasterKey unknown")]
        public void GetMasterKey_MasterKeyIsKnown(byte givenId, string expectedMasterKey)
        {
            var result = Util.GetMasterKeyVersion(givenId);
            result.Should().Be(expectedMasterKey);
        }

        [Test]
        public void GetMasterKey_MasterKeyIsUnknown()
        {
            var result = Util.GetMasterKeyVersion(6);
            result.Should().Be("MasterKey unknown");
        }

        [Test]
        public void StringToByteArray_ValidConversion()
        {
            var stringToConvert = ("536f6d65537472696e67").ToUpper();
            var result = Util.HexStringToByteArray(stringToConvert);
            var expectedResult = BitConverter.ToString(result).Replace("-", "");

            expectedResult.Should().Be(stringToConvert);
        }

        [Test]
        public void ByteArrayToString_StringNotEmpty()
        {
            var result = Util.ByteArrayToString(new byte[8]);
            result.Should().BeOfType<string>().And.NotBeNullOrEmpty();
        }
    }
}