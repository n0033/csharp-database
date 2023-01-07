

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



      }
    }

    bool isDisposed = false;



  }

}
