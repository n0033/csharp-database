using System;
using System.IO;
using CSharpDatabase.Common;
namespace CSharpDatabase.Core
{

  public class Block : IBlock
  {

    bool isDisposed = false;
    bool isFirstSectorWritten = false;
    public event EventHandler? Disposed;
    private uint?[] Headers = new uint?[5];
    BlockStorage Storage { get; }
    /// <summary>
    /// A reference to a persistent storage.
    /// </summary>
    Stream Stream { get; }
    public uint Id { get; }
    /// <summary>
    /// This is the first sector of the block, which contains the block header.
    /// This is stored in memory, to avoid reading and writing to stream every time there is a need to access the header.
    /// First sector is written to the stream when the block is disposed.
    /// </summary>
    byte[] FirstSector { get; }



    public Block(BlockStorage storage, uint id, byte[] firstSector, Stream stream)
    {
      // TODO analyse below line
      if (firstSector.Length != storage.DiskSectorSize)
        throw new ArgumentException("firstSector.Length != storage.DiskSectorSize");

      this.Storage = storage;
      this.Id = id;
      this.FirstSector = firstSector;
      this.Stream = stream;
    }

    public void SetHeader(BlockHeaderId headerId, uint value)
    {
      if (isDisposed)
        throw new ObjectDisposedException("Block");

      Headers[(int)headerId] = value;
      BufferUtils.WriteBuffer(value, FirstSector, (uint)headerId * sizeof(int));
      this.isFirstSectorWritten = true;
    }

    public uint GetHeader(BlockHeaderId headerId)
    {
      if (isDisposed)
        throw new ObjectDisposedException("Block");


      if (Headers[(int)headerId] == null)
        Headers[(int)headerId] = BufferUtils.ReadBufferUInt32(FirstSector, (uint)headerId * sizeof(int));

      return (uint)Headers[(int)headerId]!;
    }

    /// <summary>
    /// Read content of the block to a buffer.
    /// </summary>
    public void Read(byte[] dst, uint dstOffset, uint srcOffset, uint count)
    {
      if (isDisposed)
        throw new ObjectDisposedException("Block");

      if (srcOffset + count > Storage.BlockContentSize)
        throw new ArgumentOutOfRangeException("(srcOffset + count) > Storage.BlockContentSize: Data is too large to fit in the block");

      if (dstOffset + count > dst.Length)
        throw new ArgumentOutOfRangeException("(dstOffset + count) > dst.Length: Data is too large to fit in the destination buffer");

      // Read data from the first sector.
      if (Storage.BlockHeaderSize + srcOffset < Storage.DiskSectorSize)
      {
        uint firstSectorReadCount = Math.Min(count, Storage.DiskSectorSize - Storage.BlockHeaderSize - srcOffset);
        Buffer.BlockCopy(FirstSector, (int)(Storage.BlockHeaderSize + srcOffset), dst, (int)dstOffset, (int)firstSectorReadCount);
        srcOffset += firstSectorReadCount;
        dstOffset += firstSectorReadCount;
        count -= firstSectorReadCount;
      }

      // Read data from the remaining sectors.
      if (count > 0)
      {
        this.Stream.Position = this.Id * Storage.TotalBlockSize + Math.Max(Storage.DiskSectorSize, Storage.BlockHeaderSize + srcOffset);

        while (count > 0)
        {
          uint sectorReadCount = Math.Min(count, Storage.DiskSectorSize);
          int readBytesCount = this.Stream.Read(dst, (int)dstOffset, (int)sectorReadCount);
          if (readBytesCount == 0)
            throw new EndOfStreamException();
          srcOffset += sectorReadCount;
        }
      }
    }

    /// <summary>
    /// Method used to write content data to a block.
    /// The header is not affected.
    /// </summary>
    public void Write(byte[] src, uint srcOffset, uint dstOffset, uint count)
    {
      if (isDisposed)
        throw new ObjectDisposedException("Block");

      if (dstOffset + count > Storage.BlockContentSize)
        throw new ArgumentOutOfRangeException("(dstOffset + count) > Storage.BlockContentSize: Data is too large to fit in the block");

      if (srcOffset + count > src.Length)
        throw new ArgumentOutOfRangeException("(srcOffset + count) > src.Length: Data is too large to fit in the source buffer");

      // Write data to the first sector.
      if (Storage.BlockHeaderSize + dstOffset < Storage.DiskSectorSize)
      {
        uint firstSectorWriteCount = Math.Min(count, Storage.DiskSectorSize - Storage.BlockHeaderSize - dstOffset);
        Buffer.BlockCopy(src, (int)srcOffset, FirstSector, (int)(Storage.BlockHeaderSize + dstOffset), (int)firstSectorWriteCount);
        this.isFirstSectorWritten = true;
        srcOffset += firstSectorWriteCount;
        dstOffset += firstSectorWriteCount;
        count -= firstSectorWriteCount;
      }

      // Write data to the remaining sectors.
      if (count > 0)
      {
        this.Stream.Position = this.Id * Storage.TotalBlockSize + Math.Max(Storage.DiskSectorSize, Storage.BlockHeaderSize + dstOffset);

        while (count > 0)
        {
          uint sectorWriteCount = Math.Min(count, Storage.DiskSectorSize);
          this.Stream.Write(src, (int)srcOffset, (int)sectorWriteCount);
          // ensure data is written to the disk
          this.Stream.Flush();
          srcOffset += sectorWriteCount;
          count -= sectorWriteCount;
        }

      }

    }



    public override string ToString()
    {
      return $"Block(Id={Id}, " +
      $"ContentLength={GetHeader(BlockHeaderId.ContentLength)}, " +
      $"NextBlockId={GetHeader(BlockHeaderId.NextBlockId)}, " +
      $"PreviousBlockId={GetHeader(BlockHeaderId.PreviousBlockId)})";
    }

    protected virtual void OnDisposed(EventArgs e)
    {
      if (Disposed != null)
      {
        Disposed(this, e);
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing && !isDisposed)
      {
        isDisposed = true;

        if (isFirstSectorWritten)
        {
          this.Stream.Position = (Id * Storage.TotalBlockSize);
          this.Stream.Write(FirstSector, 0, (int)Storage.DiskSectorSize);
          this.Stream.Flush();
          isFirstSectorWritten = false;
        }

        OnDisposed(EventArgs.Empty);
      }
    }

    ~Block()
    {
      Dispose(false);
    }


  }

}
