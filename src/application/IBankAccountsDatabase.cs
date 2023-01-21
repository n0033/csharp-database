using System;
using System.Collections.Generic;

namespace CSharpDatabase.Application
{
  public interface ICowDatabase
  {
    void Insert(BankAccountModel account);
    bool Delete(Guid id);
    bool Update(BankAccountModel account);
    BankAccountModel Find(Guid id);
    IEnumerable<BankAccountModel> FindBy(string accNumber, int age);
  }
}

