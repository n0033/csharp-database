

namespace CSharpDatabase.Core
{

  public interface IBlockStorage
  {
    uint BlockContentSize { get; }
    uint BlockHeaderSize { get; }
    uint TotalBlockSize { get; }

    IBlock? Find(uint id);
    IBlock Create();

  }

}
