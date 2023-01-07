
namespace CSharpDatabase.Core.Block
{

  public interface IBlock : IDisposable
  {

    uint id { get; }

    long GetHeader();

    void SetHeader(long value);

    void Read(byte[] dst, int dstOffset, int srcOffset, int count);

    void Write(byte[] src, int srcOffset, int dstOffset, int count);

  }

}
