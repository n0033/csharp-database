using CSharpDatabase.Common;
namespace CSharpDatabase.Core.Block
{

  public class Block : IBlock
  {

    byte[] FirstSector { get; }
    long Header
    {
      get { return Header; }

      set
      {
        if (isDisposed)
          throw new ObjectDisposedException("Block");
        BufferUtils.WriteBuffer(value, FirstSector, 0);
      }
    }

    uint Id { get; }

    public Block(BlockStorage storage, uint id, byte[] firstSector, Stream stream)
    {
      // TODO analyse below line
      if (firstSector.Length != storage.SectorSize)
        throw new ArgumentException("firstSector.Length != storage.SectorSize");

      this.storage = storage;
      this.id = id;
      this.firstSector = firstSector;
      this.stream = stream;
    }

    bool isDisposed = false;



  }

}
