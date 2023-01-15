using System;


namespace CSharpDatabase.Core
{

  public class BlockStorage : IBlockStorage
  {
    public Stream Stream { get; }
    public uint TotalBlockSize { get; }
    public uint BlockContentSize { get; }
    public uint BlockHeaderSize { get; }
    public uint DiskSectorSize { get; }

    // public Dictionary<uint, Block> blocks { get; }

    public BlockStorage(Stream storage, int blockSize = 4096, int blockHeaderSize = 8, int diskSectorSize = 4096)
    {
      if (blockSize < blockHeaderSize)
        throw new ArgumentException("Block size must be greater than block header size");

      if (blockSize % diskSectorSize != 0)
        throw new ArgumentException("Block size must be a multiple of disk sector size");

      this.Stream = storage;
      this.TotalBlockSize = blockSize;
      this.BlockHeaderSize = blockHeaderSize;
      this.BlockContentSize = blockSize - blockHeaderSize;
      this.DiskSectorSize = diskSectorSize;
    }


    public IBlock? Find(uint id)
    {

    }



    IBlock Create()
    {

    }

  }

}
