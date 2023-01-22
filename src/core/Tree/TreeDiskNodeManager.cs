using System;
using System.Collections.Generic;

using CSharpDatabase.Common;
using CSharpDatabase.Common.Serializers;
using CSharpDatabase.Core.Indexing.Interfaces;

namespace CSharpDatabase.Core.Indexing
{

  public class TreeDiskNodeManager<K, V> : ITreeNodeManager<K, V>
  {

    TreeNode<K, V>? root;
    readonly ushort minEntriesPerNode = Constants.TREE_MINIMUM_ENTIRES_PER_NODE;
    readonly IRecordStorage recordStorage;
    readonly TreeDiskNodeSerializer<K, V> serializer;
    readonly Dictionary<uint, TreeNode<K, V>> dirtyNodes = new Dictionary<uint, TreeNode<K, V>>();
    readonly Dictionary<uint, WeakReference<TreeNode<K, V>>> nodeCache = new Dictionary<uint, WeakReference<TreeNode<K, V>>>();
    uint cacheSize = 0;
    readonly uint maxCacheSize = Constants.TREE_MAXIMUM_CACHE_SIZE;
    public ushort MinEntriesPerNode
    {
      get
      {
        return minEntriesPerNode;
      }
    }

    public IComparer<Tuple<K, V>> EntryComparer { get; private set; }
    public IComparer<K> KeyComparer { get; private set; }
    public TreeNode<K, V>? RootNode
    {
      get
      {
        return root;
      }
    }

    public TreeDiskNodeManager(ISerializer<K> keySerializer,
                               ISerializer<V> valueSerializer,
                               IRecordStorage recordStorage,
                                IComparer<K> keyComparer)
    {
      this.recordStorage = recordStorage;
      this.serializer = new TreeDiskNodeSerializer<K, V>(this, keySerializer, valueSerializer);
      this.KeyComparer = keyComparer;
      this.EntryComparer = Comparer<Tuple<K, V>>.Create((a, b) =>
      {
        return KeyComparer.Compare(a.Item1, b.Item1);
      });

      var firstBlockData = recordStorage.Find(1u);
      if (firstBlockData != null)
      {
        this.root = Find(BufferUtils.ReadBufferUInt32(firstBlockData, 0));
      }

      if (firstBlockData == null)
      {
        this.root = CreateFirstRoot();
      }
    }


    public TreeDiskNodeManager(ISerializer<K> keySerializer,
                               ISerializer<V> valueSerializer,
                               IRecordStorage nodeStorage) :
                               this(keySerializer, valueSerializer, nodeStorage, Comparer<K>.Default)
    { }

    public TreeNode<K, V>? Find(uint id)
    {
      if (nodeCache.ContainsKey(id))
      {
        TreeNode<K, V>? node;
        if (nodeCache[id].TryGetTarget(out node)) return node;
        nodeCache.Remove(id);
      }

      // node not cached, load from disk
      var data = recordStorage.Find(id);
      if (data == null)
      {
        return null;
      }
      var deserializedNode = this.serializer.Deserialize(id, data);

      InitializeNode(deserializedNode);
      return deserializedNode;
    }


    public TreeNode<K, V> Create(IEnumerable<Tuple<K, V>>? entries, IEnumerable<uint>? childrenIds)
    {
      // Create new record
      TreeNode<K, V>? node = null;

      recordStorage.Create(nodeId =>
      {
        node = new TreeNode<K, V>(this, nodeId, 0, entries, childrenIds);
        InitializeNode(node);
        return this.serializer.Serialize(node);
      });

      if (node == null)
      {
        throw new Exception("Node was not created");
      }

      return node;
    }


    public TreeNode<K, V> CreateNewRoot(K key, V value, uint leftNodeId, uint rightNodeId)
    {
      var node = Create(new Tuple<K, V>[] { new Tuple<K, V>(key, value) }, new uint[] {
        leftNodeId,
        rightNodeId
      });
      this.root = node;
      recordStorage.Update(1u, ByteConverter.ToBytes(node.Id));
      return this.root;
    }
    public void MakeRoot(TreeNode<K, V> node)
    {
      this.root = node;
      recordStorage.Update(1u, ByteConverter.ToBytes(node.Id));
    }


    public void Delete(TreeNode<K, V> node)
    {
      if (node == root)
        root = null;

      recordStorage.Delete(node.Id);
      nodeCache.Remove(node.Id);

      if (dirtyNodes.ContainsKey(node.Id))
        dirtyNodes.Remove(node.Id);

    }

    public void MarkAsDirty(TreeNode<K, V> node)
    {
      if (dirtyNodes.ContainsKey(node.Id))
        return;
      dirtyNodes.Add(node.Id, node);
    }


    public void SaveChanges()
    {
      foreach (var dictEntry in dirtyNodes)
      {
        recordStorage.Update(dictEntry.Value.Id, this.serializer.Serialize(dictEntry.Value));
      }
      dirtyNodes.Clear();
    }


    TreeNode<K, V> CreateFirstRoot()
    {
      recordStorage.Create(ByteConverter.ToBytes(2u));
      return Create(null, null); // TODO!
    }

    void InitializeNode(TreeNode<K, V> node)
    {
      nodeCache.Add(node.Id, new WeakReference<TreeNode<K, V>>(node));

      if (this.cacheSize++ >= this.maxCacheSize)
      {
        foreach (var dictEntry in this.nodeCache)
        {
          TreeNode<K, V>? target;
          if (!dictEntry.Value.TryGetTarget(out target))
          {
            this.nodeCache.Remove(dictEntry.Key);
          }
        }
        this.cacheSize = 0;
      }
    }

  }
}
