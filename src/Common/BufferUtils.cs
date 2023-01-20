using System;

namespace CSharpDatabase.Common
{
  public static class BufferUtils
  {

    public static int ReadBufferInt32(byte[] buffer, uint offset)
    {
      var tempBuffer = new byte[4];
      Buffer.BlockCopy(buffer, (int)offset, tempBuffer, 0, sizeof(int));
      return ByteConverter.ToInt32(tempBuffer);
    }

    public static uint ReadBufferUInt32(byte[] buffer, uint offset)
    {
      var tempBuffer = new byte[4];
      Buffer.BlockCopy(buffer, (int)offset, tempBuffer, 0, sizeof(int));
      return ByteConverter.ToUInt32(tempBuffer);
    }

    public static long ReadBufferInt64(byte[] buffer, uint offset)
    {
      var tempBuffer = new byte[8];
      Buffer.BlockCopy(buffer, (int)offset, tempBuffer, 0, sizeof(long));
      return ByteConverter.ToInt64(tempBuffer);
    }


    public static float ReadBufferFloat(byte[] buffer, uint offset)
    {
      var tempBuffer = new byte[4];
      Buffer.BlockCopy(buffer, (int)offset, tempBuffer, 0, sizeof(float));
      return ByteConverter.ToSingle(tempBuffer);
    }

    public static double ReadBufferDouble(byte[] buffer, uint offset)
    {
      var tempBuffer = new byte[8];
      Buffer.BlockCopy(buffer, (int)offset, tempBuffer, 0, sizeof(double));
      return ByteConverter.ToDouble(tempBuffer);
    }

    public static void WriteBuffer(int value, byte[] buffer, uint offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, (int)offset, 4);
    }

    public static void WriteBuffer(uint value, byte[] buffer, uint offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, (int)offset, 4);
    }


    public static void WriteBuffer(long value, byte[] buffer, uint offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, (int)offset, 8);
    }

    public static void WriteBuffer(double value, byte[] buffer, uint offset)
    {
      Buffer.BlockCopy(ByteConverter.ToBytes(value), 0, buffer, (int)offset, 8);
    }

  }
}
