namespace PROJECT
{
    public class Program
    {
        public class BankAccount
        {
            public int AccountNumber { get; }
            public string Owner { get; }
            public decimal Balance { get; private set; }
            public BankAccount(int accountNumber, string owner, decimal initialBalance)
            {
                AccountNumber = accountNumber;
                Owner = owner;
                Balance = initialBalance;
            }

            public bool Withdraw(decimal amount)
            {
                if (amount <= 0)
                    throw new ArgumentException("Withdrawal amount must be greater than zero.");

                if (amount > Balance)
                    return false; // Withdrawal failed due to insufficient funds.

                Balance -= amount;
                return true; // Withdrawal successful.
            }
        }

        public static void Main(string[] args)
        {
            System.Console.WriteLine("Hello, World!");
        }
    }
}

