using CSharpDatabase.Common;

namespace CSharpDatabase.Core.Indexing.Interfaces
{
  public interface ITreeNodeManager<K, V>
  {
    /// <summary>
    /// Minimum number of entries per node. Maximum number of entries
    /// must be equal to MinEntriesCountPerNode*2
    /// </summary>
    ushort MinEntriesPerNode { get; }
    IComparer<K> KeyComparer { get; }

    /// <summary>
    /// This should use KeyComparer declared above
    /// </summary>
    IComparer<Tuple<K, V>> EntryComparer { get; }

    /// <summary>
    /// Root node should be cached, because it called very often
    /// </summary>
    TreeNode<K, V>? RootNode { get; }

    /// <summary>
    /// Creates a new node that carries given entries, and keep references to given children nodes
    /// </summary>
    TreeNode<K, V> Create(IEnumerable<Tuple<K, V>> entries, IEnumerable<uint> childrenIds);

    TreeNode<K, V>? Find(uint nodeId);

    /// <summary>
    /// Called by the tree to split a current root node to a new root node
    /// </summary>
    TreeNode<K, V> CreateNewRoot(K key, V value, uint leftNodeId, uint rightNodeId);

    /// <summary>
    /// Splits current root node and makes a new root node
    /// </summary>
    void MakeRoot(TreeNode<K, V> node);

    /// <summary>
    /// Mark a given node as dirty. Dirty nodes are written to disk when SaveChanges() is called
    /// </summary>
    void MarkAsDirty(TreeNode<K, V> node);

    /// <summary>
    /// Delete specified node straight away
    /// </summary>
    void Delete(TreeNode<K, V> node);

    /// <summary>
    /// Write all dirty nodes to disk
    /// </summary>
    void SaveChanges();
  }
}

