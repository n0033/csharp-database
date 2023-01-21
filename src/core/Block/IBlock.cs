using System;


namespace CSharpDatabase.Core
{

  public interface IBlock : IDisposable
  {

    uint Id { get; }

    uint GetHeader(BlockHeaderId headerId);

    void SetHeader(BlockHeaderId headerId, uint value);

    void Read(byte[] dst, uint dstOffset, uint srcOffset, uint count);

    void Write(byte[] src, uint srcOffset, uint dstOffset, uint count);

  }

}
