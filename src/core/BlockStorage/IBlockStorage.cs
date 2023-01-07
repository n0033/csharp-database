using core.Block;


namespace core.BlockStorage
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
