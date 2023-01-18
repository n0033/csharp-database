using CSharpDatabase.Common;

namespace test.Common
{
  public class TestByteConverter
  {


    [Test]
    public void TestEndianess()
    {
      string endianness = BitConverter.IsLittleEndian ? "little" : "big";
      Assert.AreEqual(ByteConverter.computerArchitecture, endianness);
    }

    [Test]
    public void TestBytesReverse()
    {
      var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
      var bytesCopy = new byte[8];
      Array.Copy(bytes, bytesCopy, 8);
      Array.Reverse(bytesCopy);
      var reversed = ByteConverter.Reverse(bytes);
      Assert.AreEqual(bytesCopy, reversed);
    }


    [Test]
    public void TestIntToBytes()
    {
      int value = 1;
      var bytes = ByteConverter.ToBytes(value);
      Assert.AreEqual(bytes, new byte[] { 1, 0, 0, 0 });
      bytes = ByteConverter.ToBytes(value, "big");
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 1 });
      bytes = ByteConverter.ToBytes(value, "little");
      Assert.AreEqual(bytes, new byte[] { 1, 0, 0, 0 });
    }

    [Test]
    public void TestUintToBytes()
    {
      uint value = 2147483648;
      var bytes = ByteConverter.ToBytes(value);
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 128 });
      bytes = ByteConverter.ToBytes(value, "big");
      Assert.AreEqual(bytes, new byte[] { 128, 0, 0, 0 });
      bytes = ByteConverter.ToBytes(value, "little");
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 128 });
    }

    [Test]
    public void TestLongToBytes()
    {
      long value = 2147483648;
      var bytes = ByteConverter.ToBytes(value);
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 128, 0, 0, 0, 0 });
      bytes = ByteConverter.ToBytes(value, "big");
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 0, 128, 0, 0, 0 });
      bytes = ByteConverter.ToBytes(value, "little");
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 128, 0, 0, 0, 0 });
    }

    [Test]
    public void TestSingleToBytes()
    {
      float value = 1.0f;
      var bytes = ByteConverter.ToBytes(value);
      Assert.AreEqual(bytes, new byte[] { 0, 0, 128, 63 });
      bytes = ByteConverter.ToBytes(value, "big");
      Assert.AreEqual(bytes, new byte[] { 63, 128, 0, 0 });
      bytes = ByteConverter.ToBytes(value, "little");
      Assert.AreEqual(bytes, new byte[] { 0, 0, 128, 63 });
    }

    [Test]
    public void TestDoubleToBytes()
    {
      double value = 1.0;
      var bytes = ByteConverter.ToBytes(value);
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 0, 0, 0, 240, 63 });
      bytes = ByteConverter.ToBytes(value, "big");
      Assert.AreEqual(bytes, new byte[] { 63, 240, 0, 0, 0, 0, 0, 0 });
      bytes = ByteConverter.ToBytes(value, "little");
      Assert.AreEqual(bytes, new byte[] { 0, 0, 0, 0, 0, 0, 240, 63 });
    }

    [Test]
    public void TestBytesToInt()
    {
      int value = 1;
      var bytes = BitConverter.GetBytes(value);
      int convertedValue = ByteConverter.ToInt32(bytes);
      Assert.AreEqual(value, convertedValue);
    }

    [Test]
    public void TestBytesToUint()
    {
      uint value = 2147483648;
      var bytes = BitConverter.GetBytes(value);
      uint convertedValue = ByteConverter.ToUInt32(bytes);
      Assert.AreEqual(value, convertedValue);
    }

    [Test]
    public void TestBytesToLong()
    {
      long value = 2147483648;
      var bytes = BitConverter.GetBytes(value);
      long convertedValue = ByteConverter.ToInt64(bytes);
      Assert.AreEqual(value, convertedValue);
    }

    [Test]
    public void TestBytesToSingle()
    {
      float value = 1.0f;
      var bytes = BitConverter.GetBytes(value);
      float convertedValue = ByteConverter.ToSingle(bytes);
      Assert.AreEqual(value, convertedValue);
    }

    [Test]
    public void TestBytesToDouble()
    {
      double value = 1.0;
      var bytes = BitConverter.GetBytes(value);
      double convertedValue = ByteConverter.ToDouble(bytes);
      Assert.AreEqual(value, convertedValue);
    }

  }

}
