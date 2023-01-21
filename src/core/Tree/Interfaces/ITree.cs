using System;
using System.Collections.Generic;

namespace CSharpDatabase.Core.Indexing.Interfaces
{
  /// <summary>
  /// Interface for a tree. Tree is mostly used for indexing. 
  /// <summary>
  public interface ITree<K, V>
  {
    void Insert(K key, V value);

    Tuple<K, V>? Get(K key);

    IEnumerable<Tuple<K, V>> FindLargerThanOrEqualTo(K key);

    /// <summary>
    /// Deletes an entry with specified key and value;
    /// can be used with custom comparer
    /// </summary>
    bool Delete(K key, V value, IComparer<V>? valueComparer);

    /// <summary>
    /// Deletes an entry with specified key
    /// </summary>
    bool Delete(K key);
  }
}

