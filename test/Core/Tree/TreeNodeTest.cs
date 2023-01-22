using CSharpDatabase.Common.Serializers;
using CSharpDatabase.Core;
using CSharpDatabase.Core.Indexing;



namespace test.Core.Indexing
{

  [TestFixture]
  public class TestTreeDiskNodeManager
  {

    IntSerializer intSerializer = new IntSerializer();
    BlockStorage blockStorage;
    RecordStorage recordStorage;

    TreeDiskNodeManager<int, int> manager;


    [SetUp]
    public void Setup()
    {
      blockStorage = new BlockStorage(new MemoryStream(),
                                      Constants.DEFAULT_BLOCK_SIZE,
                                      Constants.DEFAULT_BLOCK_HEADER_SIZE,
                                      Constants.DEAFULT_DISK_SECTOR_SIZE);
      recordStorage = new RecordStorage(blockStorage);
      manager = new TreeDiskNodeManager<int, int>(intSerializer, intSerializer, recordStorage);
    }


    [Test]
    public void testCreateNode()
    {
      List<Tuple<int, int>> entries = new List<Tuple<int, int>>();
      entries.Add(new Tuple<int, int>(1, 1));
      entries.Add(new Tuple<int, int>(2, 2));
      entries.Add(new Tuple<int, int>(3, 3));
      var node = manager.Create(entries, null);
      Assert.NotNull(node);
    }


    [Test]
    public void testMakeRoot()
    {
      List<Tuple<int, int>> entries = new List<Tuple<int, int>>();
      entries.Add(new Tuple<int, int>(1, 1));
      entries.Add(new Tuple<int, int>(2, 2));
      entries.Add(new Tuple<int, int>(3, 3));

      var node = manager.Create(entries, null);
      manager.MakeRoot(node);
      Assert.That(manager.RootNode, Is.EqualTo(node));
    }


    [Test]
    public void testFind()
    {
      List<Tuple<int, int>> entries = new List<Tuple<int, int>>();
      entries.Add(new Tuple<int, int>(1, 1));
      entries.Add(new Tuple<int, int>(2, 2));
      entries.Add(new Tuple<int, int>(3, 3));

      var node = manager.Create(entries, null);
      manager.MakeRoot(node);
      var foundNode = manager.Find(node.Id);
      Assert.That(foundNode, Is.EqualTo(node));
    }


    [Test]
    public void testDelete()
    {
      List<Tuple<int, int>> entries = new List<Tuple<int, int>>();
      entries.Add(new Tuple<int, int>(1, 1));
      entries.Add(new Tuple<int, int>(2, 2));
      entries.Add(new Tuple<int, int>(3, 3));

      var node = manager.Create(entries, null);
      manager.MakeRoot(node);
      manager.Delete(node);
      var foundNode = manager.Find(node.Id);
      Console.WriteLine(foundNode);
      Assert.That(foundNode, Is.Null);
    }

  }

}
