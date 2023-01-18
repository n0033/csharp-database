using System;

namespace FooCore
{
	// While IBlockStorage allows client to see a Stream as individual equal length blocks,
	// IRecordStorage creates another layer on top of IBlockStorage that uses the blocks to make up variable length records.
	public interface IRecordStorage
	{
		// Effectively update an record with new data
		void Update (uint recordId, byte[] data);

		// Grab a record's data
		byte[] Find (uint recordId);

		// Create new empty record
		uint Create ();

		// Create new record with given data and returns its ID
		uint Create (byte[] data);

		// Similar to Create(byte[] data), but with dataGenerator which generates data after a record is allocated
		uint Create (Func<uint, byte[]> dataGenerator);

		// Delete a record by its id
		void Delete (uint recordId);
	}
}

