using System;
using System.Diagnostics;
using System.IO;

using CSharpDatabase.Common;
using CSharpDatabase.Common.Serializers;
using CSharpDatabase.Core.Indexing.Interfaces;

namespace CSharpDatabase.Core.Indexing
{
  public sealed class TreeDiskNodeSerializer<K, V>
  {
    ISerializer<K> keySerializer;
    ISerializer<V> valueSerializer;
    ITreeNodeManager<K, V> nodeManager;

    public TreeDiskNodeSerializer(ITreeNodeManager<K, V> nodeManager,
                                  ISerializer<K> keySerializer,
                                  ISerializer<V> valueSerializer)
    {
      this.nodeManager = nodeManager;
      this.keySerializer = keySerializer;
      this.valueSerializer = valueSerializer;
    }

    public byte[] Serialize(TreeNode<K, V> node)
    {
      if (keySerializer.IsFixedSize && valueSerializer.IsFixedSize)
      {
        return FixedLengthSerialize(node);
      }
      else if (valueSerializer.IsFixedSize)
      {
        return VariableKeyLengthSerialize(node);
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    public TreeNode<K, V> Deserialize(uint assignId, byte[] record)
    {
      if (keySerializer.IsFixedSize && valueSerializer.IsFixedSize)
      {
        return FixedLengthDeserialize(assignId, record);
      }
      else if (valueSerializer.IsFixedSize)
      {
        return VariableKeyLengthDeserialize(assignId, record);
      }
      else
      {
        throw new NotSupportedException();
      }
    }

    byte[] FixedLengthSerialize(TreeNode<K, V> node)
    {
      uint entrySize = (uint)(this.keySerializer.Length + this.valueSerializer.Length);
      uint size = (uint)(16 + node.Entries.Length * entrySize + node.ChildrenIds.Length) * sizeof(uint);
      if (size >= (1024 * 64))
      {
        throw new Exception("Serialized node size too large: " + size);
      }
      var buffer = new byte[size];

      uint offset = 0;
      BufferUtils.WriteBuffer(node.ParentId, buffer, offset);
      offset += 4;
      BufferUtils.WriteBuffer((uint)node.EntriesCount, buffer, offset);
      offset += 4;
      BufferUtils.WriteBuffer((uint)node.ChildrenNodeCount, buffer, offset);

      for (var i = 0; i < node.EntriesCount; i++)
      {
        var entry = node.GetEntry(i);
        Buffer.BlockCopy(this.keySerializer.Serialize(entry.Item1),
                         0,
                         buffer,
                         (int)(12 + i * entrySize),
                         this.keySerializer.Length);
        Buffer.BlockCopy(this.valueSerializer.Serialize(entry.Item2),
                         0,
                         buffer,
                         (int)(12 + i * entrySize + this.keySerializer.Length),
                         this.valueSerializer.Length);
      }

      var childrenIds = node.ChildrenIds;
      for (var i = 0; i < node.ChildrenNodeCount; i++)
      {
        BufferUtils.WriteBuffer(childrenIds[i], buffer, (uint)(12 + entrySize * node.EntriesCount + (i * 4)));
      }

      return buffer;
    }

    TreeNode<K, V> FixedLengthDeserialize(uint assignId, byte[] buffer)
    {
      uint entrySize = (uint)(this.keySerializer.Length + this.valueSerializer.Length);
      uint parentId = BufferUtils.ReadBufferUInt32(buffer, 0);
      uint entriesCount = BufferUtils.ReadBufferUInt32(buffer, 4);
      uint childrenCount = BufferUtils.ReadBufferUInt32(buffer, 8);

      var entries = new Tuple<K, V>[entriesCount];
      for (var i = 0; i < entriesCount; i++)
      {
        var key = this.keySerializer.Deserialize(buffer,
                                                 (uint)(12 + i * entrySize),
                                                 (uint)this.keySerializer.Length);
        var value = this.valueSerializer.Deserialize(buffer,
                                                     (uint)(12 + i * entrySize + this.keySerializer.Length),
                                                     (uint)this.valueSerializer.Length);
        entries[i] = new Tuple<K, V>(key, value);
      }

      var children = new uint[childrenCount];
      for (var i = 0; i < childrenCount; i++)
      {
        children[i] = BufferUtils.ReadBufferUInt32(buffer, (uint)(12 + entrySize * entriesCount + (i * 4)));
      }

      return new TreeNode<K, V>(nodeManager, assignId, parentId, entries, children);
    }

    TreeNode<K, V> VariableKeyLengthDeserialize(uint assignId, byte[] buffer)
    {
      uint parentId = BufferUtils.ReadBufferUInt32(buffer, 0);
      uint entriesCount = BufferUtils.ReadBufferUInt32(buffer, 4);
      uint childrenCount = BufferUtils.ReadBufferUInt32(buffer, 8);

      var entries = new Tuple<K, V>[entriesCount];
      uint currentOffset = 12;
      for (var i = 0; i < entriesCount; i++)
      {
        var keyLength = BufferUtils.ReadBufferInt32(buffer, currentOffset);
        var key = this.keySerializer.Deserialize(buffer,
                                                 currentOffset + 4,
                                                 (uint)keyLength);
        var value = this.valueSerializer.Deserialize(buffer,
                                                     currentOffset + 4 + (uint)keyLength,
                                                     (uint)this.valueSerializer.Length);

        entries[i] = new Tuple<K, V>(key, value);

        currentOffset += (uint)(4 + keyLength + this.valueSerializer.Length);
      }

      var children = new uint[childrenCount];
      for (var i = 0; i < childrenCount; i++)
      {
        children[i] = BufferUtils.ReadBufferUInt32(buffer, (uint)(currentOffset + (i * 4)));
      }
      return new TreeNode<K, V>(nodeManager, assignId, parentId, entries, children);
    }

    byte[] VariableKeyLengthSerialize(TreeNode<K, V> node)
    {
      using (var stream = new MemoryStream())
      {
        stream.Write(ByteConverter.ToBytes(node.ParentId), 0, 4);
        stream.Write(ByteConverter.ToBytes(node.EntriesCount), 0, 4);
        stream.Write(ByteConverter.ToBytes(node.ChildrenNodeCount), 0, 4);
        for (var i = 0; i < node.EntriesCount; i++)
        {
          var entry = node.GetEntry(i);
          var key = this.keySerializer.Serialize(entry.Item1);
          var value = this.valueSerializer.Serialize(entry.Item2);

          stream.Write(ByteConverter.ToBytes(key.Length), 0, 4);
          stream.Write(key, 0, key.Length);
          stream.Write(value, 0, value.Length);
        }

        var childrenIds = node.ChildrenIds;
        for (var i = 0; i < node.ChildrenNodeCount; i++)
        {
          stream.Write(ByteConverter.ToBytes(childrenIds[i]), 0, 4);
        }

        return stream.ToArray();
      }
    }
  }
}

