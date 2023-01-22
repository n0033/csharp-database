using CSharpDatabase.Common;
using CSharpDatabase.Core;


namespace test.Core
{

  [TestFixture]
  public class TestRecordStorage
  {

    BlockStorage blockStorage;
    RecordStorage recordStorage;

    [SetUp]
    public void Setup()
    {

      blockStorage = new BlockStorage(new MemoryStream(),
                                      Constants.DEFAULT_BLOCK_SIZE,
                                      Constants.DEFAULT_BLOCK_HEADER_SIZE,
                                      Constants.DEAFULT_DISK_SECTOR_SIZE);
      recordStorage = new RecordStorage(blockStorage);
    }

    [Test]
    public void testCreateRecord()
    {
      byte[] data = new byte[1024];
      for (int i = 0; i < data.Length; i++)
      {
        data[i] = (byte)i;
      }

      var recordId = recordStorage.Create(data);
      Assert.NotNull(recordId);
    }

    [Test]
    public void testFindRecord()
    {
      byte[] data = new byte[1024];
      for (int i = 0; i < data.Length; i++)
      {
        data[i] = (byte)i;
      }

      var recordId = recordStorage.Create(data);

      Assert.That(recordStorage.Find(recordId), Is.EqualTo(data));
    }

    [Test]
    public void testNonExistentRecord()
    {
      Assert.That(recordStorage.Find(0), Is.Null);
    }


    [Test]
    public void testUpdateRecord()
    {
      byte[] data = new byte[1024];
      for (int i = 0; i < data.Length; i++)
      {
        data[i] = (byte)i;
      }

      var recordId = recordStorage.Create(data);

      for (int i = 0; i < data.Length; i++)
      {
        data[i] = (byte)(data.Length - i);
      }

      recordStorage.Update(recordId, data);

      Assert.That(recordStorage.Find(recordId), Is.EqualTo(data));
    }


    [Test]
    public void testDeleteRecord()
    {
      byte[] data = new byte[1024];
      for (int i = 0; i < data.Length; i++)
      {
        data[i] = (byte)i;
      }

      var recordId = recordStorage.Create(data);

      recordStorage.Delete(recordId);

      Assert.That(recordStorage.Find(recordId), Is.Null);
    }


  }

}
