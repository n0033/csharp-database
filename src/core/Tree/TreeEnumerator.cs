using System;
using System.Collections.Generic;
using System.Collections;

using CSharpDatabase.Core.Indexing.Interfaces;
using CSharpDatabase.Core.Indexing.Enums;

namespace CSharpDatabase.Core.Indexing
{
  public class TreeEnumerator<K, V> : IEnumerator<Tuple<K, V>>
  {
    public ITreeNodeManager<K, V> NodeManager { get; }
    public TreeTraverseDirection Direction { get; }
    bool finished = false;
    public uint Id { get; }
    public TreeNode<K, V> CurrentNode { get; private set; }
    public Tuple<K, V>? Current { get; private set; }
    public uint CurrentEntry { get; private set; }
    object IEnumerator.Current
    {
      get
      {
        return (object)Current!;
      }
    }

    public TreeEnumerator(ITreeNodeManager<K, V> nodeManager,
                          TreeNode<K, V> node,
                          uint startIndex,
                          TreeTraverseDirection direction)
    {
      this.NodeManager = nodeManager;
      this.CurrentNode = node;
      this.CurrentEntry = startIndex;
      this.Direction = direction;
    }

    public bool MoveNext()
    {
      if (finished)
        return false;

      switch (this.Direction)
      {
        case TreeTraverseDirection.Forward:
          return MoveForward();
        case TreeTraverseDirection.Backward:
          return MoveBackward();
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    bool MoveForward()
    {
      CurrentEntry++;

      if (CurrentNode.IsLeaf)
      {
        while (true)
        {
          if (CurrentEntry < CurrentNode.EntriesCount)
          {
            Current = CurrentNode.GetEntry((int)CurrentEntry);
            return true;
          }
          // move up
          if (CurrentNode.ParentId != 0)
          {
            CurrentEntry = CurrentNode.IndexInParent();
            CurrentNode = NodeManager.Find(CurrentNode.ParentId)!; // TODO `!` may cause errors
          }
          else // end of tree
          {
            Current = null;
            finished = true;
            return false;
          }
        }
      }
      // not leaf node - moving right and down
      do
      {
        CurrentNode = CurrentNode.GetChildNode((int)CurrentEntry);
        CurrentEntry = 0;
      } while (!CurrentNode.IsLeaf);

      Current = CurrentNode.GetEntry((int)CurrentEntry);
      return true;
    }

    bool MoveBackward()
    {
      CurrentEntry--;
      if (CurrentNode.IsLeaf)
      {
        while (true)
        {
          if (CurrentEntry >= 0)
          {
            Current = CurrentNode.GetEntry((int)CurrentEntry);
            return true;
          }
          // can't move left - move up
          if (CurrentNode.ParentId != 0)
          {
            CurrentEntry = CurrentNode.IndexInParent() - 1;
            CurrentNode = NodeManager.Find(CurrentNode.ParentId)!; // TODO `!` may cause errors
          }
          // end of tree
          else
          {
            finished = true;
            Current = null;
            return false;
          }
        }
      }
      // not leaf node - move left down
      do
      {
        CurrentNode = CurrentNode.GetChildNode((int)CurrentEntry);
        CurrentEntry = CurrentNode.EntriesCount;
      } while (!CurrentNode.IsLeaf);

      Current = CurrentNode.GetEntry((int)CurrentEntry);
      return true;
    }

    public void Reset()
    {
      throw new NotSupportedException();
    }

    public void Dispose()
    {
    }
  }
}

