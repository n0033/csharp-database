using System;

namespace FooApplication
{
	// Our database stores cows, first we define our Cow model
	public class BankAccountModel
	{
		public Guid Id
		{
			get;
			set;
		}

		public string AccountNumber
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

        public string LastName
        {
            get;
            set;
        }

        public int Age
        {
            get;
            set;
        }

        public string Pesel
        {
            get;
            set;
        }

        public ulong Balance
        {
            get;
            set;
        }


        public override string ToString ()
		{
			return string.Format ("[BankAccountModel: Id={0}, Account number={1}, FirstName={2}, LastName={3}, Age={4}, Pesel={5}, Balance={6}]",
									Id, AccountNumber, FirstName, LastName, Age, Pesel, Balance);
		}
	}
}

