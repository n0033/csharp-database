using System;

namespace CSharpDatabase.Common.Serializers
{
  public class StringIntSerializer : ISerializer<Tuple<string, int>>
  {
    public byte[] Serialize(Tuple<string, int> value)
    {
      var stringBytes = System.Text.Encoding.UTF8.GetBytes(value.Item1);

      var data = new byte[
        4 +                    // First 4 bytes indicate length of the string
        stringBytes.Length +   // another X bytes of actual string content
        4                      // Ends with 4 bytes int value
      ];

      BufferUtils.WriteBuffer((int)stringBytes.Length, data, 0);
      Buffer.BlockCopy(src: stringBytes, srcOffset: 0, dst: data, dstOffset: 4, count: stringBytes.Length);
      BufferUtils.WriteBuffer(value.Item2, data, (uint)(4 + stringBytes.Length));
      return data;
    }

    public Tuple<string, int> Deserialize(byte[] buffer, uint offset, uint length)
    {
      var stringLength = BufferUtils.ReadBufferInt32(buffer, offset);
      if (stringLength < 0 || stringLength > (16 * 1024))
      {
        throw new Exception("Invalid string length: " + stringLength);
      }
      var stringValue = System.Text.Encoding.UTF8.GetString(buffer, (int)(offset + 4), stringLength);
      var integerValue = BufferUtils.ReadBufferInt32(buffer, (uint)(offset + 4 + stringLength));
      return new Tuple<string, int>(stringValue, integerValue);
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

