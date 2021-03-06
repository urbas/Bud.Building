using System;
using NUnit.Framework;
using static Bud.HexUtils;

namespace Bud {
  public class HexUtilsTest {
    [Test]
    public void ToBytesFromHexString_empty_string()
      => Assert.AreEqual(new byte[] {}, ToBytesFromHexString(string.Empty));

    [Test]
    public void ToBytesFromHexString_throws_when_given_null()
      => Assert.Throws<ArgumentNullException>(() => ToBytesFromHexString(null));

    [Test]
    public void ToBytesFromHexString_some_chars()
      => Assert.AreEqual(new byte[] {0x13, 0x37, 0xca, 0xfe, 0xba, 0xbe},
                         ToBytesFromHexString("1337cafebabe"));

    [Test]
    public void ToBytesFromHexString_throws_when_given_string_of_odd_length() {
      var ex = Assert.Throws<ArgumentException>(() => ToBytesFromHexString("a"));
      Assert.That(ex.Message,
                  Contains.Substring("The given string has an odd length. Hex strings must be of even length."));
    }

    [Test]
    public void ToBytesFromHexString_throws_when_given_non_hex_digits() {
      var ex = Assert.Throws<ArgumentException>(() => ToBytesFromHexString("Zl"));
      Assert.That(ex.Message,
                  Contains.Substring("The character 'Z' is not a valid hexadecimal digit. " +
                                     "Allowed characters: 0-9, a-f, A-F."));
    }

    [Test]
    public void ToBytesFromHexString_uppercase()
      => Assert.AreEqual(new byte[] {0x13, 0x37, 0xca, 0xfe, 0xba, 0xbe},
                         ToBytesFromHexString("1337CAFEBABE"));

    [Test]
    public void ToHexStringFromBytes_empty_bytes()
      => Assert.AreEqual(string.Empty, ToHexStringFromBytes(Array.Empty<byte>()));

    [Test]
    public void ToHexStringFromBytes_throws_when_given_null()
      => Assert.Throws<ArgumentNullException>(() => ToHexStringFromBytes(null));

    [Test]
    public void ToHexStringFromBytes_some_bytes()
      => Assert.AreEqual("1337CAFEBABE",
                         ToHexStringFromBytes(new byte[] {0x13, 0x37, 0xca, 0xfe, 0xba, 0xbe}));
  }
}