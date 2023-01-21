using System;
using System.Collections.Generic;

namespace FooApplication
{
    public interface ICowDatabase
    {
        void Insert(BankAccountModel account);
        void Delete(BankAccountModel account);
        void Update(BankAccountModel account);
        BankAccountModel Find(Guid id);
        IEnumerable<BankAccountModel> FindBy(string accNumber, int age);
    }
}

