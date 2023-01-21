namespace CSharpDatabase.Common.Serializers
{
  public class UintSerializer : ISerializer<uint>
  {
    public byte[] Serialize(uint value)
    {
      return ByteConverter.ToBytes(value);
    }

    public uint Deserialize(byte[] buffer, uint offset, uint length)
    {
      if (length != 4)
      {
        throw new ArgumentException("uint length must be 4 bytes");
      }

      return ByteConverter.ToUInt32(buffer);
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
