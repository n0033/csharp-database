using CSharpDatabase.Core;


namespace test.Core
{

  [TestFixture]
  public class TestBlock
  {

    BlockStorage storage;
    MemoryStream stream;
    Block block;

    [SetUp]
    public void Setup()
    {
      stream = new MemoryStream();
      storage = new BlockStorage(stream);
      block = new Block(storage, 0, new byte[Constants.DEAFULT_DISK_SECTOR_SIZE], stream);
    }

    [Test]
    public void TestInit()
    {
      block = new Block(storage, 0, new byte[Constants.DEAFULT_DISK_SECTOR_SIZE], stream);
      Assert.That(block.Id, Is.EqualTo(0));
      Assert.That(block.GetHeader(BlockHeaderId.ContentLength), Is.EqualTo(0));
      Assert.That(block.GetHeader(BlockHeaderId.IsDeleted), Is.EqualTo(0));
    }

    [Test]
    public void TestGetSetHeader()
    {
      block.SetHeader(BlockHeaderId.ContentLength, 123);
      Assert.That(block.GetHeader(BlockHeaderId.ContentLength), Is.EqualTo(123));
    }


    [Test]
    public void testReadWriteData()
    {
      var data = new byte[] { 1, 2, 3, 4, 5 };
      var readData = new byte[5];
      block.Write(data, 0, 0, 5);
      Assert.That(block.GetHeader(BlockHeaderId.IsDeleted), Is.EqualTo(0));
      block.Read(readData, 0, 0, 5);
      Assert.That(readData, Is.EqualTo(data));
    }

  }

}
