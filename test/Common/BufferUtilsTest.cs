using CSharpDatabase.Common;

namespace test.Common
{
  public class TestBufferUtils
  {

    [Test]
    public void TestWriteInt()
    {
      int value = 12345678;
      byte[] buffer = new byte[4];
      BufferUtils.WriteBuffer(value, buffer, 0);
      Assert.That(BitConverter.ToInt32(buffer, 0), Is.EqualTo(value));
    }

    [Test]
    public void TestWriteUint()
    {
      uint value = 12345678;
      byte[] buffer = new byte[4];
      BufferUtils.WriteBuffer(value, buffer, 0);
      Assert.That(BitConverter.ToInt32(buffer, 0), Is.EqualTo(value));
    }


    [Test]
    public void TestWriteLong()
    {
      long value = 1234567812345675;
      byte[] buffer = new byte[8];
      BufferUtils.WriteBuffer(value, buffer, 0);
      Assert.That(BitConverter.ToInt64(buffer, 0), Is.EqualTo(value));
    }

    [Test]
    public void TestWriteDouble()
    {
      double value = 123.123;
      byte[] buffer = new byte[8];
      BufferUtils.WriteBuffer(value, buffer, 0);
      Assert.That(BitConverter.ToDouble(buffer, 0), Is.EqualTo(value));
    }

    [Test]
    public void TestWriteWithOffset()
    {
      int value = 12345678;
      byte[] buffer = new byte[8];
      BufferUtils.WriteBuffer(value, buffer, 4);
      Assert.That(BitConverter.ToInt32(buffer, 4), Is.EqualTo(value));
    }

  }

}
