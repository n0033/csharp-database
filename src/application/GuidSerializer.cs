using System;
using FooCore;

namespace FooApplication
{
  // Just a thin wrapper around Guid.ToByteArray() to make it compatible with ISerializer[Guid]
  public class GuidSerializer : ISerializer<Guid>
  {
    public byte[] Serialize(Guid value)
    {
      return value.ToByteArray();
    }

    public Guid Deserialize(byte[] buffer, int offset, int length)
    {
      if (length != 16)
        throw new ArgumentException("length");

      return BufferHelper.ReadBufferGuid(buffer, offset);
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
        return 16;
      }
    }
  }
}

