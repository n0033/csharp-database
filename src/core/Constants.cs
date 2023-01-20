

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
}
