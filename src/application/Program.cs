using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Demo application on how to use FooCore to define a database and query data
/// </summary>
namespace FooApplication
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // For demonstartion purpose, our cow database is saved into a temp file
            var dbFile = Path.Combine(".", "cowdb.data");

            try
            {
                // Init the cow db based on a file on temp directory
                using (var db = new BankAccountsDatabase(dbFile))
                {
                    // Insert some cows into our database..
                    db.Insert(new BankAccountModel
                    {
                        Id = Guid.Parse("8872d8ba-e470-440d-aa9b-071822e8053f"),
                        AccountNumber = "123465789123456798",
                        FirstName = "Adamo",
                        LastName = "Traore",
                        Age = 25,
                        Pesel = "25441236479",
                        Balance = 102049
                    });
                    Console.WriteLine("Inserted first account");
                    Console.WriteLine("Found account by id: " + db.Find(Guid.Parse("8872d8ba-e470-440d-aa9b-071822e8053f")).ToString());

                    db.Insert(new BankAccountModel
                    {
                        Id = Guid.Parse("59ee9033-4ec5-40e0-91a7-6c9ecb6e0465"),
                        AccountNumber = "111111111111111",
                        FirstName = "Arturo",
                        LastName = "Vidal",
                        Age = 37,
                        Pesel = "45716542889",
                        Balance = 2499999
                    });
                    Console.WriteLine("Inserted second account");
                }

                // Reconstruct our database again, to demonstrate that accounts data are persistence
                using (var db = new BankAccountsDatabase(dbFile))
                {
                    // Find an account by its Id, 
                    // This uses the primary index so the query is an ad-hoc query.
                    Console.WriteLine("Found account by id: " + db.Find(Guid.Parse("8872d8ba-e470-440d-aa9b-071822e8053f")).ToString());

                    Console.WriteLine("Searching for: Arturo, 37 years old: ");
                    foreach (var row in db.FindBy(name: "Arturo", age: 37))
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
