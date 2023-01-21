using CSharpDatabase.Core.Indexing.Enums;
using CSharpDatabase.Core.Indexing.Exceptions;
using CSharpDatabase.Core.Indexing.Interfaces;

namespace CSharpDatabase.Core.Indexing
{


  class Tree<K, V> : ITree<K, V>
  {

    readonly ITreeNodeManager<K, V> nodeManager;
    readonly bool allowDuplicateKeys;


    public Tree(ITreeNodeManager<K, V> nodeManager, bool allowDuplicateKeys)
    {
      this.nodeManager = nodeManager;
      this.allowDuplicateKeys = allowDuplicateKeys;
    }


    public void Insert(K key, V value)
    {
      // First find the node where key should be inserted
      var insertionIndex = 0;
      var leafNode = FindNodeForInsertion(key, this.nodeManager.RootNode, ref insertionIndex);

      if (insertionIndex >= 0 && !allowDuplicateKeys)
      {
        throw new TreeKeyExistsException(key);
      }

      leafNode.InsertLeafNode(key, value, insertionIndex >= 0 ? insertionIndex : ~insertionIndex);

      if (leafNode.IsOverflowed)
      {
        TreeNode<K, V> left, right;
        leafNode.Split(out left, out right);
      }

      nodeManager.SaveChanges();
    }


    public bool Delete(K key, V value, IComparer<V> valueComparer)
    {
      if (!allowDuplicateKeys)
      {
        throw new InvalidOperationException("When allowDuplicateKeys is false, use Delete(K key) instead.");
      }

      var deleted = false;
      var continueSearch = true;

      while (continueSearch)
      {
        using (var enumerator = (TreeEnumerator<K, V>)FindLargerThanOrEqualTo(key).GetEnumerator())
        {
          do
          {
            var entry = enumerator.Current;

            // bound reached, stop searching
            if (nodeManager.KeyComparer.Compare(entry.Item1, key) > 0)
            {
              continueSearch = false;
              break;
            }

            if (valueComparer.Compare(entry.Item2, value) == 0)
            {
              enumerator.CurrentNode.Remove((int)enumerator.CurrentEntry);
              deleted = true;
              break; // Get new enumerator
            }

          } while (enumerator.MoveNext());
        }
      }

      nodeManager.SaveChanges();
      return deleted;
    }


    public bool Delete(K key)
    {
      if (allowDuplicateKeys)
        throw new InvalidOperationException("When allowDuplicateKeys is true, use Delete(K key, V value, IComparer<V>? valueComparer) instead.");

      using (var enumerator = (TreeEnumerator<K, V>)FindLargerThanOrEqualTo(key).GetEnumerator())
      {
        if (enumerator.MoveNext() && (nodeManager.KeyComparer.Compare(enumerator.Current.Item1, key) == 0))
        {
          enumerator.CurrentNode.Remove((int)enumerator.CurrentEntry);
          return true;
        }
      }
      // key not found
      return false;
    }

    public Tuple<K, V>? Get(K key)
    {
      int foundIndex = -1;
      TreeNode<K, V> node = this.FindNodeForInsertion(key, this.nodeManager.RootNode, ref foundIndex);
      if (foundIndex < 0)
        return null;

      return node.GetEntry(foundIndex);

    }


    public IEnumerable<Tuple<K, V>> FindLargerThanOrEqualTo(K key)
    {
      var startIterationIndex = 0;
      var node = FindNodeForIteration(key, this.nodeManager.RootNode, true, ref startIterationIndex);

      return new TreeTraverser<K, V>(nodeManager,
                                     node,
                                     (uint)((startIterationIndex >= 0 ? startIterationIndex : ~startIterationIndex) - 1),
                                     TreeTraverseDirection.Forward);
    }


    TreeNode<K, V> FindNodeForIteration(K key, TreeNode<K, V> node, bool moveLeft, ref int startIterationIndex)
    {
      if (node.IsEmpty)
      {
        startIterationIndex = -1;
        return node;
      }

      var foundIndex = node.SearchEntriesByKey(key, moveLeft ? true : false);
      if (foundIndex >= 0)
      {
        if (node.IsLeaf)
        {
          startIterationIndex = foundIndex;
          return node;
        }
        // if found node is not leaf, we continue search in the child node
        return FindNodeForIteration(key, node.GetChildNode(moveLeft ? foundIndex : foundIndex + 1), moveLeft, ref startIterationIndex);
      }
      if (!node.IsLeaf) // node not found and this is not a leaf node
      {
        return FindNodeForIteration(key, node.GetChildNode(~foundIndex), moveLeft, ref startIterationIndex);
      }
      // node not found and this is a leaf node
      startIterationIndex = foundIndex;
      return node;
    }



    // <summary>
    //  Finds a node where the key should be inserted
    // </summary>
    TreeNode<K, V> FindNodeForInsertion(K key, TreeNode<K, V> node, ref int insertionIndex)
    {
      // If this node is empty, we can to this node
      if (node.IsEmpty)
      {
        insertionIndex = -1;
        return node;
      }

      int foundIndex = node.SearchEntriesByKey(key);
      if (foundIndex >= 0)
      {
        if (allowDuplicateKeys && !node.IsLeaf)
        {
          return FindNodeForInsertion(key, node.GetChildNode(foundIndex), ref insertionIndex);
        }

        // either duplicate keys are not allowed, or this is a leaf node
        insertionIndex = foundIndex;
        return node;
      }

      if (!node.IsLeaf)
      {
        return FindNodeForInsertion(key, node.GetChildNode(~foundIndex), ref insertionIndex);
      }

      // this is a leaf node, and we key was not found
      insertionIndex = foundIndex;
      return node;
    }

  }

}
