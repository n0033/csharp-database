using CSharpDatabase.Common;
using CSharpDatabase.Core;

namespace test.Core
{

  [TestFixture]
  public class TestBlockStorage
  {
    BlockStorage storage;

    [SetUp]
    public void Setup()
    {
      storage = new BlockStorage(new MemoryStream());
    }

    [Test]
    public void TestInit()
    {
      Assert.That(storage.TotalBlockSize, Is.EqualTo(Constants.DEFAULT_BLOCK_SIZE));
      Assert.That(storage.BlockContentSize, Is.EqualTo(Constants.DEFAULT_BLOCK_SIZE - Constants.DEFAULT_BLOCK_HEADER_SIZE));
      Assert.That(storage.BlockHeaderSize, Is.EqualTo(Constants.DEFAULT_BLOCK_HEADER_SIZE));
      Assert.That(storage.DiskSectorSize, Is.EqualTo(Constants.DEAFULT_DISK_SECTOR_SIZE));
    }

    [Test]
    public void TestInitCustomParameters()
    {
      storage = new BlockStorage(new MemoryStream(), 100, 10, 100);
      Assert.That(storage.TotalBlockSize, Is.EqualTo(100));
      Assert.That(storage.BlockContentSize, Is.EqualTo(90));
      Assert.That(storage.BlockHeaderSize, Is.EqualTo(10));
      Assert.That(storage.DiskSectorSize, Is.EqualTo(100));
    }

    [Test]
    public void TestCreateBlock()
    {
      var block = storage.Create();
      Assert.That(block.Id, Is.EqualTo(0));
      Assert.That(block.GetHeader(BlockHeaderId.ContentLength), Is.EqualTo(0));
      Assert.That(block.GetHeader(BlockHeaderId.IsDeleted), Is.EqualTo(0));
    }

    [Test]
    public void TestFindBlock()
    {
      var block = storage.Create();
      var foundBlock = storage.Find(block.Id);
      Assert.That(foundBlock, Is.Not.Null);
      Assert.That(foundBlock!.Id, Is.EqualTo(block.Id));
    }

    [Test]
    public void TestFindNonExistentBlock()
    {
      var foundBlock = storage.Find(0);
      Assert.That(foundBlock, Is.Null);
    }


  }

}
