using CSharpDatabase.Common;
using CSharpDatabase.Common.Serializers;
using CSharpDatabase.Core;
using CSharpDatabase.Core.Indexing;


namespace test.Core.Indexing
{


  [TestFixture]
  public class TestTree
  {

    IntSerializer intSerializer = new IntSerializer();
    BlockStorage blockStorage;
    RecordStorage recordStorage;
    TreeDiskNodeManager<int, int> manager;
    Tree<int, int> tree;

    [SetUp]
    public void Setup()
    {
      blockStorage = new BlockStorage(new MemoryStream(),
                                      Constants.DEFAULT_BLOCK_SIZE,
                                      Constants.DEFAULT_BLOCK_HEADER_SIZE,
                                      Constants.DEAFULT_DISK_SECTOR_SIZE);
      recordStorage = new RecordStorage(blockStorage);
      manager = new TreeDiskNodeManager<int, int>(intSerializer, intSerializer, recordStorage);
      tree = new Tree<int, int>(manager, false);
    }


    [Test]
    public void TestInsert()
    {
      tree.Insert(1, 1);
      tree.Insert(2, 2);
      tree.Insert(3, 3);
      Assert.That(manager.RootNode!.Entries.Count, Is.EqualTo(3));
    }


    [Test]
    public void TestGet()
    {
      tree.Insert(1, 1);
      tree.Insert(2, 2);
      tree.Insert(3, 3);
      Assert.That(tree.Get(1), Is.EqualTo(new Tuple<int, int>(1, 1)));
      Assert.That(tree.Get(2), Is.EqualTo(new Tuple<int, int>(2, 2)));
      Assert.That(tree.Get(3), Is.EqualTo(new Tuple<int, int>(3, 3)));
    }


    [Test]
    public void TestDelete()
    {
      tree.Insert(1, 1);
      tree.Insert(2, 2);
      tree.Insert(3, 3);
      tree.Delete(1);
      tree.Delete(2);
      tree.Delete(3);
      Assert.That(manager.RootNode!.Entries.Count, Is.EqualTo(0));
    }


    [Test]
    public void TestDeleteDuplicateKeys()
    {
      tree = new Tree<int, int>(manager, true);
      tree.Insert(1, 1);
      tree.Insert(1, 2);
      tree.Insert(1, 3);
      tree.Delete(1, 1, Comparer<int>.Default);
      Assert.That(manager.RootNode!.Entries.Count, Is.EqualTo(2));
      tree.Delete(1, 2, Comparer<int>.Default);
      Assert.That(manager.RootNode!.Entries.Count, Is.EqualTo(1));
      var foundTuple = tree.Get(1);
      Assert.That(foundTuple, Is.EqualTo(new Tuple<int, int>(1, 3)));
    }



  }
}
