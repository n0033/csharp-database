using System;
using System.IO;
using System.Collections.Generic;

namespace CSharpDatabase.Application
{
  class MainClass
  {
    public static void Main(string[] args)
    {
      var dbFile = Path.Combine(".", "bank.data");

      try
      {
        using (var db = new BankAccountsDatabase(dbFile))
        {
          db.Insert(new BankAccountModel
          (
            id: Guid.Parse("8872d8ba-e470-440d-aa9b-071822e8053f"),
            accountNumber: "123465789123456798",
            firstName: "Adamo",
            lastName: "Traore",
            age: 25,
            pesel: "25441236479",
            balance: 102049
          ));
          Console.WriteLine("Inserted first account");
          Console.WriteLine("Found account by id: " + db.Find(Guid.Parse("8872d8ba-e470-440d-aa9b-071822e8053f"))!.ToString());

          db.Insert(new BankAccountModel
          (
            id: Guid.Parse("59ee9033-4ec5-40e0-91a7-6c9ecb6e0465"),
            accountNumber: "111111111111111",
            firstName: "Arturo",
            lastName: "Vidal",
            age: 37,
            pesel: "45716542889",
            balance: 2499999
          ));
          Console.WriteLine("Inserted second account");
        }

        // Reconstruct our database again, to demonstrate that accounts data are persistence
        using (var db = new BankAccountsDatabase(dbFile))
        {
          // Find an account by its Id, 
          // This uses the primary index so the query is an ad-hoc query.
          Console.WriteLine("Found account by id: " + db.Find(Guid.Parse("8872d8ba-e470-440d-aa9b-071822e8053f"))!.ToString());

          Console.WriteLine("Searching for: Arturo, 37 years old: ");
          foreach (var row in db.FindBy(name: "Arturo", age: 37)!)
          {
            Console.WriteLine(row.ToString());
          }
          Console.WriteLine("Deleting Arturo: " + db.Delete(Guid.Parse("59ee9033-4ec5-40e0-91a7-6c9ecb6e0465")));

          Console.WriteLine("Searching for: Adamo, 25 years old: ");
          BankAccountModel adamo;
          foreach (var row in db.FindBy(name: "Adamo", age: 25)!)
          {
            adamo = row;
            Console.WriteLine(row.ToString());
            adamo.Balance = 20;
            adamo.Pesel = "666";
            Console.WriteLine("Updating Adamo: " + db.Update(adamo));
          }

          Console.WriteLine("Searching for: Adamo, 25 years old (updated): ");
          foreach (var row in db.FindBy(name: "Adamo", age: 25)!)
          {
            Console.WriteLine(row.ToString());
          }

        }
      }
      // Clean up stuff after the demo,
      finally
      {
        if (File.Exists(dbFile))
        {
          File.Delete(dbFile);
          Console.WriteLine("Deleted main database file");
        }

        if (File.Exists(dbFile + ".pidx"))
        {
          File.Delete(dbFile + ".pidx");
          Console.WriteLine("Deleted primary index file");
        }

        if (File.Exists(dbFile + ".sidx"))
        {
          File.Delete(dbFile + ".sidx");
          Console.WriteLine("Deleted secondary index file");
        }
      }
    }
  }
}
