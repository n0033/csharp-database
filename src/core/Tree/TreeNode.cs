using CSharpDatabase.Core.Indexing.Interfaces;


namespace CSharpDatabase.Core.Indexing
{

  public class TreeNode<K, V>
  {
    protected uint id = 0;
    protected uint parentId = 0;

    protected readonly ITreeNodeManager<K, V> nodeManager;
    protected readonly List<uint> childrenIds;
    protected readonly List<Tuple<K, V>> entries; // sorted in ascending order

    public uint Id { get { return id; } }
    public K MaxKey { get { return entries[entries.Count - 1].Item1; } }
    public K MinKey { get { return entries[0].Item1; } }
    public bool IsEmpty { get { return entries.Count == 0; } }
    public bool IsLeaf { get { return childrenIds.Count == 0; } }
    public bool IsOverflowed { get { return entries.Count > (nodeManager.MinEntriesPerNode * 2); } }
    public Tuple<K, V>[] Entries { get { return entries.ToArray(); } }
    public uint EntriesCount { get { return (uint)entries.Count; } }
    public uint[] ChildrenIds { get { return childrenIds.ToArray(); } }
    public uint ChildrenNodeCount { get { return (uint)childrenIds.Count; } }
    public uint ParentId
    {
      get
      {
        return parentId;
      }
      private set
      {
        parentId = value;
        nodeManager.MarkAsDirty(this);
      }
    }

    public TreeNode(ITreeNodeManager<K, V> nodeManager,
                    uint id,
                    uint parentId,
                    IEnumerable<Tuple<K, V>>? entries,
                    IEnumerable<uint>? childrenIds)
    {
      this.id = id;
      this.parentId = parentId;
      this.nodeManager = nodeManager;
      this.childrenIds = new List<uint>();
      if (childrenIds != null)
      {
        this.childrenIds.AddRange(childrenIds);
      }
      this.entries = new List<Tuple<K, V>>(this.nodeManager.MinEntriesPerNode * 2);
      if (entries != null)
      {
        this.entries.AddRange(entries);
      }
    }

    public TreeNode<K, V> GetChildNode(int atIndex)
    {
      return nodeManager.Find(childrenIds[atIndex])!;
    }

    public Tuple<K, V> GetEntry(int atIndex)
    {
      return entries[atIndex];
    }

    public bool EntryExists(int atIndex)
    {
      return atIndex < entries.Count;
    }


    public int SearchEntriesByKey(K key)
    {
      return entries.BinarySearch(new Tuple<K, V>(key, default(V)!), this.nodeManager.EntryComparer);
    }

    /// <summary>
    /// Performs binary search on entries list
    /// returns index of the first or last entry with specified key
    /// </summary>
    public int SearchEntriesByKey(K key, bool getFirstOccurence)
    {
      int foundIndex = entries.BinarySearch(new Tuple<K, V>(key, default(V)!), this.nodeManager.EntryComparer);
      if (foundIndex < 0)
        return foundIndex;

      if (getFirstOccurence)
      {
        for (int i = foundIndex; i >= 0; i--)
        {
          if (this.nodeManager.EntryComparer.Compare(entries[i], new Tuple<K, V>(key, default(V)!)) != 0)
            return i + 1;
        }
        return foundIndex;
      }
      else
      {
        for (int i = foundIndex; i < entries.Count; i++)
        {
          if (this.nodeManager.EntryComparer.Compare(entries[i], new Tuple<K, V>(key, default(V)!)) != 0)
            return i - 1;
        }
        return foundIndex;
      }
    }

    /// <summary>
    /// Returns index of this node in parent node's children list
    /// </summary>
    public uint IndexInParent()
    {
      var parent = nodeManager.Find(parentId);
      if (parent == null)
      {
        throw new Exception("Failed to find parent node of node " + id);
      }
      var childrenIds = parent.ChildrenIds;
      foreach (var childId in childrenIds)
      {
        if (childId == id) return childId;
      }
      throw new Exception("Failed to find index of node " + id + " in parent node " + parentId);
    }


    /// <summary>
    /// Find the largest entry in this subtree.
    /// Output is written to specified parameters
    /// </summary>
    public void FindLargest(out TreeNode<K, V> node, out int index)
    {
      if (IsLeaf)
      {
        node = this;
        index = this.entries.Count - 1;
        return;
      }
      // continue search on the right most node
      var rightMostNode = nodeManager.Find(this.childrenIds[this.childrenIds.Count - 1]);
      rightMostNode!.FindLargest(out node, out index);
    }

    /// <summary>
    /// Find the smallest entry on this subtree and output it to specified parameters
    /// </summary>
    public void FindSmallest(out TreeNode<K, V> node, out int index)
    {
      // If this node is leave then we reached
      // the bottom of the tree, return this node's max value
      if (IsLeaf)
      {
        node = this;
        index = 0;
        return;
      }
      // continue search on the left most node
      var leftMostNode = nodeManager.Find(this.childrenIds[0]);
      leftMostNode!.FindSmallest(out node, out index);
    }


    /// <summary>
    /// Insert leaf entry into this node
    /// </summary>
    public void InsertLeafNode(K key, V value, int insertPosition)
    {
      if (!IsLeaf)
        throw new Exception("Trying to insert into non-leaf node");

      entries.Insert(insertPosition, new Tuple<K, V>(key, value));
      nodeManager.MarkAsDirty(this);
    }

    /// <summary>
    /// Insert parent entry into this node
    /// </summary>
    public void InsertParentNode(K key, V value, uint leftReference, uint rightReference, out int insertPosition)
    {
      if (!IsLeaf)
        throw new Exception("Trying to insert into non-leaf node");

      insertPosition = this.SearchEntriesByKey(key);
      insertPosition = insertPosition >= 0 ? insertPosition : ~insertPosition;

      entries.Insert(insertPosition, new Tuple<K, V>(key, value));

      childrenIds.Insert(insertPosition, leftReference);
      childrenIds[insertPosition + 1] = rightReference;

      nodeManager.MarkAsDirty(this);
    }

    public void Split(out TreeNode<K, V> outLeftNode, out TreeNode<K, V> outRightNode)
    {
      if (!this.IsOverflowed)
        throw new Exception("Trying to split non-overflowed node");


      // node is at full capacity
      // halfCount is the half of the number of entries
      var halfCount = this.nodeManager.MinEntriesPerNode;
      var middleEntry = entries[halfCount];

      // Create new node that holds all values
      // greater than the middle node
      var rightEntries = new Tuple<K, V>[halfCount];
      var rightChildren = (uint[])null!;
      entries.CopyTo(halfCount + 1, rightEntries, 0, rightEntries.Length);

      if (!IsLeaf) // copy children ids
      {
        rightChildren = new uint[halfCount + 1];
        childrenIds.CopyTo(halfCount + 1, rightChildren, 0, rightChildren.Length);
      }
      var newRightNode = nodeManager.Create(rightEntries, rightChildren!);

      // update new children's parent id
      if (rightChildren != null)
      {
        foreach (var childId in rightChildren)
        {
          nodeManager.Find(childId)!.ParentId = newRightNode.Id;
        }
      }

      // Remove all values that larger than the middle 
      // one from current node
      entries.RemoveRange(halfCount, entries.Count - halfCount);

      if (!IsLeaf)
      {
        childrenIds.RemoveRange(halfCount + 1, entries.Count - (halfCount + 1));
      }

      // set middle node as the parent node  
      var parent = parentId == 0 ? null : nodeManager.Find(parentId);

      if (parent == null) // if this node is root
      {
        parent = this.nodeManager.CreateNewRoot(middleEntry.Item1,
                                                middleEntry.Item2,
                                                id,
                                                newRightNode.Id);
        this.ParentId = parent.Id;
        newRightNode.ParentId = parent.Id;
      }

      if (parent != null) // elevate middle entry to parent
      {
        int insertPosition;
        parent.InsertParentNode(middleEntry.Item1,
                                middleEntry.Item2,
                                id,
                                newRightNode.Id,
                                out insertPosition);

        newRightNode.ParentId = parent.id;

        // If parent is overflowed, split it as well
        if (parent.IsOverflowed)
        {
          TreeNode<K, V> left, right;
          parent.Split(out left, out right);
        }
      }

      // write output
      outLeftNode = this;
      outRightNode = newRightNode;
      nodeManager.MarkAsDirty(this);
    }


    public override string ToString()
    {
      if (IsLeaf)
      {
        var keys = (from tuple in this.entries select tuple.Item1.ToString()).ToArray();
        return string.Format("[Node: Id={0}, ParentId={1}, Entries={2}]",
                              Id,
                              ParentId,
                              String.Join(",", keys));
      }
      else
      {
        var keys = (from tuple in this.entries select tuple.Item1.ToString()).ToArray();
        var ids = (from id in this.childrenIds select id.ToString()).ToArray();
        return string.Format("[Node: Id={0}, ParentId={1}, Entries={2}, Children={3}]",
                              Id,
                              ParentId,
                              String.Join(",", keys),
                              String.Join(",", ids));
      }
    }

    void Rebalance()
    {
      var indexInParent = IndexInParent();
      var parent = nodeManager.Find(parentId);

      // if the deficient node's right sibling exists and has more than the minimum number of elements, then rotate left
      var rightSibling = ((indexInParent + 1) < parent!.ChildrenNodeCount) ? parent.GetChildNode((int)(indexInParent + 1)) : null;
      if ((rightSibling != null) && (rightSibling.EntriesCount > nodeManager.MinEntriesPerNode))
      {
        entries.Add(parent.GetEntry((int)indexInParent));
        parent.entries[(int)indexInParent] = rightSibling.entries[0];
        rightSibling.entries.RemoveAt(0);

        // Move the first child reference from right sibling to this node
        if (!rightSibling.IsLeaf)
        {
          var firstSiblingChild = nodeManager.Find(rightSibling.childrenIds[0]);
          firstSiblingChild!.parentId = this.id;
          nodeManager.MarkAsDirty(firstSiblingChild);

          childrenIds.Add(rightSibling.childrenIds[0]);
          rightSibling.childrenIds.RemoveAt(0);
        }

        nodeManager.MarkAsDirty(this);
        nodeManager.MarkAsDirty(parent);
        nodeManager.MarkAsDirty(rightSibling);
        return;
      }

      // if the deficient node's left sibling exists and has more than the minimum number of elements, then rotate right 
      var leftSibling = ((indexInParent - 1) >= 0) ? parent.GetChildNode((int)(indexInParent - 1)) : null;
      if ((leftSibling != null) && (leftSibling.EntriesCount > nodeManager.MinEntriesPerNode))
      {
        entries.Insert(0, parent.GetEntry((int)(indexInParent - 1)));

        parent.entries[(int)(indexInParent - 1)] = leftSibling.entries[leftSibling.entries.Count - 1];
        leftSibling.entries.RemoveAt(leftSibling.entries.Count - 1);

        // Move the last child reference from the left sibing to this node
        if (!leftSibling.IsLeaf) // TODO check it
        {
          var lastSiblingChild = nodeManager.Find(leftSibling.childrenIds[leftSibling.childrenIds.Count - 1]);
          lastSiblingChild!.parentId = this.id;
          nodeManager.MarkAsDirty(lastSiblingChild);

          childrenIds.Insert(0, leftSibling.childrenIds[leftSibling.childrenIds.Count - 1]);
          leftSibling.childrenIds.RemoveAt(leftSibling.childrenIds.Count - 1);
        }

        nodeManager.MarkAsDirty(this);
        nodeManager.MarkAsDirty(parent);
        nodeManager.MarkAsDirty(leftSibling);
        return;
      }

      // if both siblings have only the minimum number of elements,
      var leftChild = rightSibling != null ? this : leftSibling;
      var rightChild = rightSibling != null ? rightSibling : this;
      var seperatorParentIndex = rightSibling != null ? indexInParent : (indexInParent - 1);

      // move separator from parent to the left node
      leftChild!.entries.Add(parent.GetEntry((int)seperatorParentIndex));

      // Move all elements from the right node to the left 
      leftChild.entries.AddRange(rightChild.entries);
      leftChild.childrenIds.AddRange(rightChild.childrenIds);
      // Update parent id of the children that has been moved from rightChild to leftChild
      foreach (var id in rightChild.childrenIds)
      {
        var currentNode = nodeManager.Find(id);
        currentNode!.parentId = leftChild.id;
        nodeManager.MarkAsDirty(currentNode); ;
      }

      // remove the leftovers
      parent.entries.RemoveAt((int)seperatorParentIndex);
      parent.childrenIds.RemoveAt((int)(seperatorParentIndex + 1));
      nodeManager.Delete(rightChild);

      // If the parent is the root and has no elements, then make the left child the new root
      if (parent.parentId == 0 && parent.EntriesCount == 0)
      {
        leftChild.parentId = 0;
        nodeManager.MarkAsDirty(leftChild);
        nodeManager.MakeRoot(leftChild);
        nodeManager.Delete(parent);
      }
      // if parent has more than the minimum number of elements - rebalance it
      else if ((parent.parentId != 0) && (parent.EntriesCount < nodeManager.MinEntriesPerNode))
      {
        nodeManager.MarkAsDirty(leftChild);
        nodeManager.MarkAsDirty(parent);
        parent.Rebalance();
      }
      else
      {
        nodeManager.MarkAsDirty(leftChild);
        nodeManager.MarkAsDirty(parent);
      }
    }


    public void Remove(int removeAt)
    {
      if ((removeAt < 0) || (removeAt > this.entries.Count))
      {
        throw new ArgumentOutOfRangeException();
      }

      if (IsLeaf)
      {
        entries.RemoveAt(removeAt);
        nodeManager.MarkAsDirty(this);

        if ((EntriesCount < nodeManager.MinEntriesPerNode) && (parentId != 0))
        {
          this.Rebalance();
        }
      }
      // if is not leaf, replace it with the largest value in its left subtree
      if (!IsLeaf)
      {
        TreeNode<K, V> largestNode;
        int largestNodeIndex;
        var leftSubTree = nodeManager.Find(this.childrenIds[removeAt]);
        leftSubTree!.FindLargest(out largestNode, out largestNodeIndex);
        var replacementNode = largestNode.GetEntry(largestNodeIndex);

        this.entries[removeAt] = replacementNode;
        nodeManager.MarkAsDirty(this);

        largestNode.Remove(largestNodeIndex);
      }
    }


  }
}
