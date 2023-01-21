

namespace CSharpDatabase.Core
{
  public enum BlockHeaderId : ushort
  {

    RecordLength = 0, // length of the record which consists of blocks
    ContentLength = 1, // length of the content in the block
    NextBlockId = 2, // id of the next block in the record
    PreviousBlockId = 3, // id of the previous block in the record
    IsDeleted = 4 // a flag indicating whether the block is deleted

  }

  public static class Constants
  {
    public const int DEFAULT_BLOCK_SIZE = 4096;
    public const int DEFAULT_BLOCK_HEADER_SIZE = 24;
    public const int DEAFULT_DISK_SECTOR_SIZE = 4096;
    public const int MAX_RECORD_SIZE = 4 * 1024 * 1024;
    public const int TREE_MINIMUM_ENTIRES_PER_NODE = 36;
    public const int TREE_MAXIMUM_CACHE_SIZE = 1024;
  }

}
