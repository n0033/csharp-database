namespace CSharpDatabase.Common
{

  public static class ByteConverter
  {
    // TODO consider guarding endianess when casting to int, uint, long, etc.

    public static string computerArchitecture = BitConverter.IsLittleEndian ? "little" : "big";

    public static byte[] Reverse(byte[] bytes)
    {
      Array.Reverse(bytes);
      return bytes;
    }

    public static byte[] ToBytes(int value, string byteOrder = "little")
    {
      var bytes = BitConverter.GetBytes(value);
      if (computerArchitecture != byteOrder)
        Reverse(bytes);
      return bytes;
    }

    public static byte[] ToBytes(uint value, string byteOrder = "little")
    {
      var bytes = BitConverter.GetBytes(value);
      if (computerArchitecture != byteOrder)
        Reverse(bytes);
      return bytes;
    }

    public static byte[] ToBytes(long value, string byteOrder = "little")
    {
      var bytes = BitConverter.GetBytes(value);
      if (computerArchitecture != byteOrder)
        Reverse(bytes);
      return bytes;
    }

    public static byte[] ToBytes(float value, string byteOrder = "little")
    {
      var bytes = BitConverter.GetBytes(value);
      if (computerArchitecture != byteOrder)
        Reverse(bytes);
      return bytes;
    }

    public static byte[] ToBytes(double value, string byteOrder = "little")
    {
      var bytes = BitConverter.GetBytes(value);
      if (computerArchitecture != byteOrder)
        Reverse(bytes);
      return bytes;
    }


    public static int ToInt32(byte[] bytes)
    {
      return BitConverter.ToInt32(bytes, 0);
    }

    public static uint ToUInt32(byte[] bytes)
    {
      return BitConverter.ToUInt32(bytes, 0);
    }

    public static long ToInt64(byte[] bytes)
    {
      return BitConverter.ToInt64(bytes, 0);
    }

    public static float ToSingle(byte[] bytes)
    {
      return BitConverter.ToSingle(bytes, 0);
    }

    public static double ToDouble(byte[] bytes)
    {
      return BitConverter.ToDouble(bytes, 0);
    }

  }
}
