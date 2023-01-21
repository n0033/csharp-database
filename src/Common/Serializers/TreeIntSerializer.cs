using System;
using CSharpDatabase.Common;

namespace CSharpDatabase.Common.Serializers
{
  public class TreeIntSerializer : ISerializer<int>
  {
    public byte[] Serialize(int value)
    {
      return ByteConverter.ToBytes(value);
    }

    public int Deserialize(byte[] buffer, uint offset, uint length)
    {
      if (length != 4)
      {
        throw new ArgumentException("Invalid length: " + length);
      }

      return BufferUtils.ReadBufferInt32(buffer, offset);
    }

    public bool IsFixedSize
    {
      get
      {
        return true;
      }
    }

    public int Length
    {
      get
      {
        return 4;
      }
    }
  }

  public class TreeUIntSerializer : ISerializer<uint>
  {
    public byte[] Serialize(uint value)
    {
      return ByteConverter.ToBytes(value);
    }

    public uint Deserialize(byte[] buffer, uint offset, uint length)
    {
      if (length != 4)
      {
        throw new ArgumentException("Invalid length: " + length);
      }

      return BufferUtils.ReadBufferUInt32(buffer, offset);
    }

    public bool IsFixedSize
    {
      get
      {
        return true;
      }
    }

    public int Length
    {
      get
      {
        return 4;
      }
    }
  }
}

