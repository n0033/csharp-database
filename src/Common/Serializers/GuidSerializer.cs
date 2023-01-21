namespace CSharpDatabase.Common.Serializers
{
  public class GuidSerializer : ISerializer<Guid>
  {
    public byte[] Serialize(Guid value)
    {
      return value.ToByteArray();
    }

    public Guid Deserialize(byte[] buffer, uint offset, uint length)
    {
      if (length != 16)
      {
        throw new ArgumentException("Guid length must be 16 bytes");
      }
      return BufferUtils.ReadBufferGuid(buffer, offset);
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

