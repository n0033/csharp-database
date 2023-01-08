namespace CSharpDatabase.Common
{
  public static class BufferUtils
  {

    public static void WriteBuffer(int value, byte[] buffer, int offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, offset, 4);
    }

    public static void WriteBuffer(uint value, byte[] buffer, int offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, offset, 4);
    }


    public static void WriteBuffer(long value, byte[] buffer, int offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, offset, 8);
    }

    public static void WriteBuffer(double value, byte[] buffer, int offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, offset, 8);
    }

  }
}
