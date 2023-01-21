using System.Collections;

using CSharpDatabase.Core.Indexing.Enums;
using CSharpDatabase.Core.Indexing.Interfaces;

namespace CSharpDatabase.Core.Indexing
{
  public class TreeTraverser<K, V> : IEnumerable<Tuple<K, V>>
  {
    readonly TreeNode<K, V> fromNode;
    readonly uint fromIndex;
    readonly TreeTraverseDirection direction;
    readonly ITreeNodeManager<K, V> nodeManager;

    public TreeTraverser(ITreeNodeManager<K, V> nodeManager,
                         TreeNode<K, V> fromNode,
                         uint fromIndex,
                         TreeTraverseDirection direction)
    {
      this.direction = direction;
      this.fromIndex = fromIndex;
      this.fromNode = fromNode;
      this.nodeManager = nodeManager;
    }

    IEnumerator<Tuple<K, V>> IEnumerable<Tuple<K, V>>.GetEnumerator()
    {
      return new TreeEnumerator<K, V>(nodeManager, fromNode, fromIndex, direction);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Tuple<K, V>>)this).GetEnumerator();
    }
  }
}

