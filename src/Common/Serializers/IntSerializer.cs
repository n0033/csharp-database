namespace CSharpDatabase.Common.Serializers
{
  public class IntSerializer : ISerializer<int>
  {
    public byte[] Serialize(int value)
    {
      return ByteConverter.ToBytes(value);
    }

    public int Deserialize(byte[] buffer, uint offset, uint length)
    {
      if (length != 4)
      {
        throw new ArgumentException("int length must be 4 bytes");
      }
      return ByteConverter.ToInt32(buffer);
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
