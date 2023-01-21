namespace CSharpDatabase.Common.Serializers
{
  public interface ISerializer<K>
  {
    byte[] Serialize(K value);

    K Deserialize(byte[] buffer, uint offset, uint length);

    bool IsFixedSize
    {
      get;
    }

    int Length
    {
      get;
    }
  }

}
