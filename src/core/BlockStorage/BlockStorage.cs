using System;
using System.Collections.Generic;
using System.IO;


namespace CSharpDatabase.Core
{

  public class BlockStorage : IBlockStorage
  {
    public Stream Stream { get; }
    public uint TotalBlockSize { get; }
    public uint BlockContentSize { get; }
    public uint BlockHeaderSize { get; }
    public uint DiskSectorSize { get; }
    readonly Dictionary<uint, IBlock> blocks = new Dictionary<uint, IBlock>();

    public BlockStorage(Stream storage,
                        uint blockSize = Constants.DEFAULT_BLOCK_SIZE,
                        uint blockHeaderSize = Constants.DEFAULT_BLOCK_HEADER_SIZE,
                        uint diskSectorSize = Constants.DEAFULT_DISK_SECTOR_SIZE)
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
      if (blocks.ContainsKey(id))
      {
        return blocks[id];
      }

      if (id * TotalBlockSize + TotalBlockSize > Stream.Length)
      {
        return null;
      }

      var firstSector = new byte[DiskSectorSize];
      Stream.Position = id * TotalBlockSize;
      Stream.Read(firstSector, 0, (int)DiskSectorSize);

      var block = new Block(this, id, firstSector, Stream);

      blocks[block.Id] = block;
      block.Disposed += DisposeBlock;

      return block;
    }


    /// <summary>
    /// Creates a new block
    /// </summary>
    public IBlock Create()
    {
      if ((this.Stream.Length % TotalBlockSize) != 0)
      {
        throw new DataMisalignedException("Stream length is not a multiple of block size");
      }

      var blockId = (uint)this.Stream.Length / TotalBlockSize;

      // extend stream
      this.Stream.SetLength((long)(blockId * TotalBlockSize + TotalBlockSize));
      this.Stream.Flush();

      var block = new Block(this, blockId, new byte[DiskSectorSize], this.Stream);

      blocks[block.Id] = block;
      block.Disposed += DisposeBlock;

      return block;
    }



    protected virtual void DisposeBlock(object sender, EventArgs e)
    {
      var block = (Block)sender;
      block.Disposed -= DisposeBlock;

      blocks.Remove(block.Id);
    }

  }

}
