using System;

namespace CSharpDatabase.Common.Serializers
{
  public class TreeStringSerialzier : ISerializer<string>
  {
    public byte[] Serialize(string value)
    {
      return System.Text.Encoding.UTF8.GetBytes(value);
    }

    public string Deserialize(byte[] buffer, uint offset, uint length)
    {
      return System.Text.Encoding.UTF8.GetString(buffer, (int)offset, (int)length);
    }

    public bool IsFixedSize
    {
      get
      {
        return false;
      }
    }

    public int Length
    {
      get
      {
        throw new InvalidOperationException();
      }
    }
  }
}

