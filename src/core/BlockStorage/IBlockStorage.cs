using CSharpDatabase.Core.Block;


namespace CSharpDatabase.Core.BlockStorage
{

  public interface IBlockStorage
  {
    int BlockContentSize { get; }
    int BlockHeaderSize { get; }
    int BlockTotalSize { get; }

    IBlock? Find(uint id);
    IBlock Create();

  }

}
