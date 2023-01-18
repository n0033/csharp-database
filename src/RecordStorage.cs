using System;
using System.IO;
using System.Collections.Generic;

namespace FooCore
{
    // Record storage service that store data in form of records, each record made up from one or several blocks
    // Blocks are connected in double-linked list (using kNextBlockId and kPreviousBlockId)
    public class RecordStorage : IRecordStorage
    {
        readonly IBlockStorage storage;

        const int MaxRecordSize = 4194304;  // 4MB
        const int kRecordLength = 1;        // Number of blocks that make up the full record
        const int kBlockContentLength = 1;  // Length of block content (it doesn't have to fill all available space)
        const int kNextBlockId = 0;         // If == 0 then block doesn't have successor
        const int kPreviousBlockId = 0;     // If == 0 then block doesn't have predecessor
        const int kIsDeleted = 0;           // If == 1 then block is marked as deleted


        // *** Constructors ***

        public RecordStorage(IBlockStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            this.storage = storage;

            if (storage.BlockHeaderSize < 48)
            {
                throw new ArgumentException("Record storage needs at least 48 header bytes");
            }
        }


        // *** Public Methods ***

        public virtual byte[] Find(uint recordId)
        {
            // First grab the block
            using (var block = storage.Find(recordId))
            {
                if (block == null)
                    return null;

                // If this is a deleted block then ignore it
                if (IsDeleted(block))
                    return null;


                // If this block is a child block then also ignore it
                if (IsChild(block))
                    return null;

                // Grab total record size and allocate coressponded memory
                var totalRecordSize = block.GetHeader(kRecordLength);

                if (totalRecordSize > MaxRecordSize)
                    throw new NotSupportedException("Unexpected record length: " + totalRecordSize);

                var data = new byte[totalRecordSize];
                var bytesRead = 0;

                // Start filling data
                IBlock currentBlock = block;
                while (true)
                {
                    uint nextBlockId;

                    using (currentBlock)
                    {
                        var thisBlockContentLength = currentBlock.GetHeader(kBlockContentLength);

                        if (thisBlockContentLength > storage.BlockContentSize)
                            throw new InvalidDataException("Unexpected block content length: " + thisBlockContentLength);

                        // Read all available content of current block
                        currentBlock.Read(dst: data, dstOffset: bytesRead, srcOffset: 0, count: (int)thisBlockContentLength);

                        // Update number of bytes read
                        bytesRead += (int)thisBlockContentLength;

                        if (IsLastInRecord(currentBlock))
                            return data;
                        else
                            nextBlockId = (uint)currentBlock.GetHeader(kNextBlockId);
                    }

                    currentBlock = this.storage.Find(nextBlockId);

                    if (currentBlock == null)
                        throw new InvalidDataException("Block not found by id: " + nextBlockId);
                }
            }
        }

        public virtual uint Create(Func<uint, byte[]> dataGenerator)
        {
            if (dataGenerator == null)
                throw new ArgumentException();


            using (var firstBlock = AllocateBlock())
            {
                // Returning ID should be ID of the first block in record
                var returnId = firstBlock.Id;

                byte[] data = dataGenerator(returnId);
                int written = 0;
                int dataTobeWritten = data.Length;
                firstBlock.SetHeader(kRecordLength, dataTobeWritten);

                // If no data tobe written, return this block
                if (dataTobeWritten == 0)
                    return returnId;

                // Calculate how many blocks have to been used
                int numOfBlocksToBeWritten = (int)Math.Ceiling((double)dataTobeWritten / storage.BlockContentSize);


                IBlock currentBlock = firstBlock;
                while (numOfBlocksToBeWritten > 0)
                {
                    IBlock nextBlock = null;

                    using (currentBlock)
                    {
                        int dataToBeWrittenInBlock = storage.BlockContentSize;

                        // Last block of record will not always be fully filled
                        if (dataTobeWritten - written < dataToBeWrittenInBlock)
                            dataToBeWrittenInBlock = dataTobeWritten - written;

                        currentBlock.Write(data, written, 0, dataToBeWrittenInBlock);
                        currentBlock.SetHeader(kBlockContentLength, (long)dataToBeWrittenInBlock);
                        written += dataToBeWrittenInBlock;

                        numOfBlocksToBeWritten--;

                        // If still there are some blocks to be written
                        if (numOfBlocksToBeWritten > 0)
                        {
                            nextBlock = AllocateBlock();
                            var success = false;

                            // If something go wrong, nextBlock has to be disposed
                            try
                            {
                                nextBlock.SetHeader(kPreviousBlockId, currentBlock.Id);
                                currentBlock.SetHeader(kNextBlockId, nextBlock.Id);
                                success = true;
                            }
                            finally
                            {
                                if ((false == success) && (nextBlock != null))
                                {
                                    nextBlock.Dispose();
                                    nextBlock = null;
                                }
                            }
                        }
                        else
                            break;
                    }


                    // Move to the next block if possible
                    if (nextBlock != null)
                        currentBlock = nextBlock;
                }

                // return id of the first block that got dequeued
                return returnId;
            }
        }

        public virtual uint Create(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentException();
            }
            return Create(recordId => data);
        }

        public virtual uint Create()
        {
            using (var firstBlock = AllocateBlock())
            {
                return firstBlock.Id;
            }
        }

        public virtual void Delete(uint recordId)
        {
            using (var block = storage.Find(recordId))
            {
                IBlock currentBlock = block;
                while (true)
                {
                    IBlock nextBlock = null;

                    using (currentBlock)
                    {
                        MarkAsFree(currentBlock.Id);
                        currentBlock.SetHeader(kIsDeleted, 1);

                        if (IsLastInRecord(currentBlock))
                            break;
                        else
                        {
                            var nextBlockId = (uint)currentBlock.GetHeader(kNextBlockId);
                            nextBlock = storage.Find(nextBlockId);
                        }

                    }

                    // Move to next block
                    if (nextBlock != null)
                        currentBlock = nextBlock;
                }
            }
        }

        public virtual void Update(uint recordId, byte[] data)
        {
            var blocks = FindBlocks(recordId);

            try
            {
                var written = 0;
                var dataTobeWritten = data.Length;

                var blocksUsed = 0;
                int numOfBlocksToBeWritten = (int)Math.Ceiling((double)dataTobeWritten / storage.BlockContentSize);

                var previousBlock = (IBlock)null;


                while (blocksUsed < numOfBlocksToBeWritten)
                {
                    int dataToBeWrittenInBlock = storage.BlockContentSize;

                    // Last block of record will not always be fully filled
                    if (dataTobeWritten - written < dataToBeWrittenInBlock)
                        dataToBeWrittenInBlock = dataTobeWritten - written;

                    // Get the block where the first byte of remaining data will be written to
                    var blockIndex = (int)Math.Floor((double)written / (double)storage.BlockContentSize);

                    // Find the block to write to:
                    // If `blockIndex` exists in `blocks`, then write into it,
                    // otherwise allocate a new one for writting
                    IBlock target = null;
                    if (blockIndex < blocks.Count)
                    {
                        target = blocks[blockIndex];
                    }
                    else
                    {
                        target = AllocateBlock();

                        if (target == null)
                            throw new Exception("Failed to allocate new block");

                        blocks.Add(target);
                    }

                    // Link with previous block
                    if (previousBlock != null)
                    {
                        previousBlock.SetHeader(kNextBlockId, target.Id);
                        target.SetHeader(kPreviousBlockId, previousBlock.Id);
                    }

                    // Write data
                    target.Write(src: data, srcOffset: written, dstOffset: 0, count: dataToBeWrittenInBlock);
                    target.SetHeader(kBlockContentLength, dataToBeWrittenInBlock);
                    target.SetHeader(kNextBlockId, 0);

                    if (written == 0)
                        target.SetHeader(kRecordLength, dataTobeWritten);


                    blocksUsed++;
                    written += dataToBeWrittenInBlock;
                    previousBlock = target;
                }

                // After writing, delete off any unused blocks
                if (blocksUsed < blocks.Count)
                {
                    for (int i = blocksUsed; i < blocks.Count; i++)
                        MarkAsFree(blocks[i].Id);
                }
            }
            finally
            {
                // Always dispose all fetched blocks after finish using them
                foreach (var block in blocks)
                    block.Dispose();
            }
        }


        // *** Private Methods ***

        // Find all blocks of given record and return them in right order
        List<IBlock> FindBlocks(uint recordId)
        {
            var blocks = new List<IBlock>();
            var success = false;

            try
            {
                var nextBlockId = recordId;
                var block = storage.Find(nextBlockId);

                while (true)
                {
                    if (block == null)
                    {
                        // Special case: if block #0 never created, then attempt to create it
                        if (nextBlockId == 0)
                        {
                            block = storage.CreateNew();
                        }
                        else
                            throw new Exception("Block not found by id: " + nextBlockId);

                    }

                    blocks.Add(block);

                    if (IsDeleted(block))
                        throw new InvalidDataException("Block not found: " + nextBlockId);


                    if (IsLastInRecord(block))
                        break;
                    else
                    {
                        nextBlockId = (uint)block.GetHeader(kNextBlockId);
                        block = storage.Find(nextBlockId);
                    }
                }

                success = true;
                return blocks;
            }
            finally
            {
                // If something went wrong, all blocks have to be disposed
                if (success == false)
                {
                    foreach (var block in blocks)
                        block.Dispose();
                }
            }
        }

        /// Allocate new block for use, either by dequeueing an exising non-used block or creating a new one
        /// Returns newly allocated block ready to use
        IBlock AllocateBlock()
        {
            uint resuableBlockId;
            IBlock newBlock;

            // If there isn't any freed block - create new one
            if (TryFindFreeBlock(out resuableBlockId) == false)
            {
                newBlock = storage.CreateNew();
                if (newBlock == null)
                    throw new Exception("Failed to create new block");

            }
            // Else - find it by Id, and prepare it for reuse
            else
            {
                newBlock = storage.Find(resuableBlockId);

                if (newBlock == null)
                    throw new InvalidDataException("Block not found by id: " + resuableBlockId);

                newBlock.SetHeader(kBlockContentLength, 0);
                newBlock.SetHeader(kNextBlockId, 0);
                newBlock.SetHeader(kPreviousBlockId, 0);
                newBlock.SetHeader(kRecordLength, 0);
                newBlock.SetHeader(kIsDeleted, 0);
            }

            return newBlock;
        }

        // Check if there is any freed block, and if so, return its Id
        bool TryFindFreeBlock(out uint blockId)
        {
            blockId = 0;
            IBlock lastBlock, preLastBlock;
            GetSpaceTrackingBlock(out lastBlock, out preLastBlock);

            using (lastBlock)
            using (preLastBlock)
            {
                var currentBlockContentLength = lastBlock.GetHeader(kBlockContentLength);

                // If last block is empty, try to take pre last block
                if (currentBlockContentLength == 0)
                {
                    // If there is no previous block, return false to indicate we can't
                    if (preLastBlock == null)
                        return false;

                    // Dequeue an uint from previous block, then mark current block as free
                    blockId = ReadUInt32FromTrailingContent(preLastBlock);

                    // Back off 4 bytes (after dequeue) before calling AppendUInt32ToContent
                    preLastBlock.SetHeader(kBlockContentLength, preLastBlock.GetHeader(kBlockContentLength) - 4);
                    AppendUInt32ToContent(preLastBlock, lastBlock.Id);

                    // Add Id of (empty) last block to pre last block
                    // (this is done, because pre last block now has 4B released, last block is empty)
                    preLastBlock.SetHeader(kBlockContentLength, preLastBlock.GetHeader(kBlockContentLength) + 4);
                    preLastBlock.SetHeader(kNextBlockId, 0);
                    lastBlock.SetHeader(kPreviousBlockId, 0);

                    // Success
                    return true;
                }
                // If this block is not empty then dequeue an UInt32 from it and correct BlockContentLength
                else
                {
                    blockId = ReadUInt32FromTrailingContent(lastBlock);
                    lastBlock.SetHeader(kBlockContentLength, currentBlockContentLength - 4);

                    // Success
                    return true;
                }
            }
        }

        // Append Uint32 (Id of some block) to destination_block
        void AppendUInt32ToContent(IBlock destination_block, uint IdValue)
        {
            var contentLength = destination_block.GetHeader(kBlockContentLength);

            if ((contentLength % 4) != 0)
                throw new DataMisalignedException("Block content length not %4: " + contentLength);

            // Write value (first it has to be converted to byte[]) to destination_block
            destination_block.Write(src: LittleEndianByteOrder.GetBytes(IdValue), srcOffset: 0, dstOffset: (int)contentLength, count: 4);
        }

        // Read UInt32 (Id of some block) from source_block
        uint ReadUInt32FromTrailingContent(IBlock source_block)
        {
            var buffer = new byte[4];
            var contentLength = source_block.GetHeader(kBlockContentLength);

            if ((contentLength % 4) != 0)
                throw new DataMisalignedException("Block content length not %4: " + contentLength);

            if (contentLength == 0)
                throw new InvalidDataException("Trying to dequeue UInt32 from an empty block");

            // Read value from source_block, convert it to UInt32 and return
            source_block.Read(dst: buffer, dstOffset: 0, srcOffset: (int)contentLength - 4, count: 4);
            return LittleEndianByteOrder.GetUInt32(buffer);
        }

        // Mark block as free by adding blockId to content of record_0
        // record_0 is used as stack with Id's of freed blocks
        void MarkAsFree(uint blockId)
        {
            IBlock lastBlock, preLastBlock, targetBlock = null;
            GetSpaceTrackingBlock(out lastBlock, out preLastBlock);

            using (lastBlock)
            using (preLastBlock)
            {
                try
                {
                    var contentLength = lastBlock.GetHeader(kBlockContentLength);

                    // If there is some space left in last record 0's block just use it
                    if ((contentLength + 4) <= storage.BlockContentSize)
                    {
                        targetBlock = lastBlock;
                    }
                    // If there isn't - allocate new block for record 0 and set header's
                    // Note that we allocate fresh new block, if we reuse it may fuck things up
                    else
                    {
                        targetBlock = storage.CreateNew();

                        targetBlock.SetHeader(kPreviousBlockId, lastBlock.Id);
                        lastBlock.SetHeader(kNextBlockId, targetBlock.Id);

                        contentLength = 0;
                    }

                    // Write Id of freed block to end of record 0
                    AppendUInt32ToContent(targetBlock, blockId);

                    // Extend the length of the block by 4 - that's how long the block id takes
                    targetBlock.SetHeader(kBlockContentLength, contentLength + 4);
                }
                finally
                {
                    // Always dispose targetBlock
                    if (targetBlock != null)
                    {
                        targetBlock.Dispose();
                    }
                }
            }
        }

        // Get the last 2 blocks from the free space tracking record_0
        void GetSpaceTrackingBlock(out IBlock lastBlock, out IBlock preLastBlock)
        {
            lastBlock = null;
            preLastBlock = null;

            // Grab all blocks that make a record 0 
            var blocks = FindBlocks(0);

            try
            {
                if (blocks == null || (blocks.Count == 0))
                    throw new Exception("Failed to find blocks of record 0");


                // Assign last block
                lastBlock = blocks[blocks.Count - 1];

                // If record_0 contains more than one block - assign also pre last
                if (blocks.Count > 1)
                    preLastBlock = blocks[blocks.Count - 2];
 
            }
            finally
            {
                // Always dispose unused blocks
                if (blocks != null)
                {
                    foreach (var block in blocks)
                    {
                        // Dispose all blocks except last and 
                        if ((lastBlock == null || block != lastBlock) && (preLastBlock == null || block != preLastBlock))
                            block.Dispose();
 
                    }
                }
            }
        }

        // Check if the block is deleted
        bool IsDeleted(IBlock block)
        {
            if (block.GetHeader(kIsDeleted) == 1)
                return true;
            
            return false;
        }

        // Check if the block is last in record
        bool IsLastInRecord(IBlock block)
        {
            if (block.GetHeader(kNextBlockId) == 0)
                return true;

            return false;
        }

        // Check if the block is child
        bool IsChild(IBlock block)
        {
            if (block.GetHeader(kPreviousBlockId) != 0)
                return true;

            return false;
        }
    }
}