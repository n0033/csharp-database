using System;
using System.IO;
using System.Collections.Generic;
using FooCore;

namespace FooApplication
{
    class BankAccountsDatabase : IDisposable
    {
        readonly Stream mainDatabaseFile;
        readonly Stream primaryIndexFile;
        readonly Stream secondaryIndexFile;
        readonly Tree<Guid, uint> primaryIndex;
        readonly Tree<Tuple<string, int>, uint> secondaryIndex;
        readonly RecordStorage accountRecords;
        readonly BankAccountsSerializer accountSerializer = new BankAccountsSerializer();


        public BankAccountsDatabase(string pathToAccDb)
        {
            if (pathToAccDb == null)
                throw new ArgumentNullException("pathToAccDb");

            // As soon as BankAccountsDatabase is constructed, open the stream to talk to the underlying files
            this.mainDatabaseFile = new FileStream(pathToAccDb, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            this.primaryIndexFile = new FileStream(pathToAccDb + ".pidx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);
            this.secondaryIndexFile = new FileStream(pathToAccDb + ".sidx", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096);

            // Construct the RecordStorage that use to store main account data
            this.accountRecords = new RecordStorage(new BlockStorage(this.mainDatabaseFile, 4096, 24));

            // Construct the primary index 
            this.primaryIndex = new Tree<Guid, uint>(
              new TreeDiskNodeManager<Guid, uint>(
                new GuidSerializer(),
                new TreeUIntSerializer(),
                new RecordStorage(new BlockStorage(this.primaryIndexFile, 4096))
              ),
              false
            );

            // Construct the secondary index 
            this.secondaryIndex = new Tree<Tuple<string, int>, uint>(
              new TreeDiskNodeManager<Tuple<string, int>, uint>(
                new StringIntSerializer(),
                new TreeUIntSerializer(),
                new RecordStorage(new BlockStorage(this.secondaryIndexFile, 4096))
              ),
              true
            );
        }

        // Update given account
        public void Update(BankAccountModel acc)
        {
            if (disposed)
                throw new ObjectDisposedException("BankAccountsDatabase");

            throw new NotImplementedException();
        }

        // Insert a new account entry into account database
        public void Insert(BankAccountModel account)
        {
            if (disposed)
                throw new ObjectDisposedException("BankAccountsDatabase");

            // Serialize the account and insert it
            var recordId = this.accountRecords.Create(this.accountSerializer.Serialize(account));
            var createdRecord = this.accountRecords.Find(recordId);

            // Primary index
            this.primaryIndex.Insert(account.Id, recordId);

            // Secondary index
            this.secondaryIndex.Insert(new Tuple<string, int>(account.AccountNumber, account.Age), recordId);
        }

        // Find a account by its unique id
        public BankAccountModel Find(Guid accountId)
        {
            if (disposed)
                throw new ObjectDisposedException("BankAccountsDatabase");

            // Look in the primary index for this account
            var entry = this.primaryIndex.Get(accountId);
            if (entry == null)
                return null;

            return this.accountSerializer.Deserializer(this.accountRecords.Find(entry.Item2));
        }

        // Find all accounts that belongs to given name and age
        public IEnumerable<BankAccountModel> FindBy(string name, int age)
        {
            var comparer = Comparer<Tuple<string, int>>.Default;
            var searchKey = new Tuple<string, int>(name, age);

            // Use the secondary index to find this account
            foreach (var entry in this.secondaryIndex.LargerThanOrEqualTo(searchKey))
            {
                // As soon as we reached larger key than the key given by client, stop
                if (comparer.Compare(entry.Item1, searchKey) > 0)
                    break;

                // Still in range, yield return
                yield return this.accountSerializer.Deserializer(this.accountRecords.Find(entry.Item2));
            }
        }

        // Delete specified account from our database
        public void Delete(BankAccountModel account)
        {
            throw new NotImplementedException();
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                this.mainDatabaseFile.Dispose();
                this.secondaryIndexFile.Dispose();
                this.primaryIndexFile.Dispose();
                this.disposed = true;
            }
        }

        ~BankAccountsDatabase()
        {
            Dispose(false);
        }
        #endregion
    }
}

